---
title: CI/CD Integration
---

# CI/CD Integration

OwlSight is designed for CI/CD pipelines. It exits with code `1` when critical findings are found, making it a natural quality gate.

| Exit Code | Meaning |
|-----------|---------|
| `0` | Review passed — no critical findings |
| `1` | Review failed — critical findings found |
| `2` | Error — configuration issue, LLM failure, etc. |

---

## GitHub Actions

```yaml
name: Code Review

on:
  pull_request:
    branches: [main]

jobs:
  review:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0  # Full history for accurate diff

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Build OwlSight
        run: dotnet build path/to/OwlSight.sln

      - name: Run Review
        env:
          OWLSIGHT_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          dotnet run --project path/to/OwlSight/src/OwlSight.Cli -- \
            review \
            --base origin/main \
            --output review-report.json

      - name: Upload Report
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: review-report
          path: review-report.json
```

### With Docker

```yaml
      - name: Run Review
        run: |
          docker run --rm \
            -v ${{ github.workspace }}:/repo \
            -e OWLSIGHT_API_KEY=${{ secrets.OPENAI_API_KEY }} \
            owlsight review --base origin/main --output /repo/report.json
```

---

## GitLab CI

```yaml
code-review:
  image: mcr.microsoft.com/dotnet/sdk:10.0-preview
  stage: test
  before_script:
    - apt-get update && apt-get install -y git
    - dotnet build path/to/OwlSight.sln
  script:
    - |
      dotnet run --project path/to/OwlSight/src/OwlSight.Cli -- \
        review \
        --base origin/$CI_MERGE_REQUEST_TARGET_BRANCH_NAME \
        --api-key $OPENAI_API_KEY \
        --output review-report.json
  artifacts:
    when: always
    paths:
      - review-report.json
  rules:
    - if: $CI_PIPELINE_SOURCE == "merge_request_event"
```

---

## Azure DevOps

```yaml
trigger: none

pr:
  branches:
    include:
      - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    inputs:
      version: '10.0.x'

  - checkout: self
    fetchDepth: 0

  - script: dotnet build path/to/OwlSight.sln
    displayName: Build OwlSight

  - script: |
      dotnet run --project path/to/OwlSight/src/OwlSight.Cli -- \
        review \
        --base origin/main \
        --api-key $(OPENAI_API_KEY) \
        --output $(Build.ArtifactStagingDirectory)/review-report.json
    displayName: Run Code Review

  - task: PublishBuildArtifacts@1
    condition: always()
    inputs:
      pathToPublish: $(Build.ArtifactStagingDirectory)/review-report.json
      artifactName: review-report
```

---

## Using a Self-Hosted LLM

For on-premise deployments where code must not leave the network:

```yaml
      - name: Run Review
        run: |
          dotnet run --project path/to/OwlSight/src/OwlSight.Cli -- \
            review \
            --base origin/main \
            --base-url http://internal-llm-server:8000/v1 \
            --api-key $INTERNAL_API_KEY \
            --model codellama
```

---

## Tips

- **Always use `fetch-depth: 0`** (or equivalent) so Git has the full history for accurate diffs.
- **Use `origin/main`** (not just `main`) as the base branch in CI, since the local `main` may not be up to date.
- **Store API keys as secrets** — never hardcode them in pipeline files.
- **Upload the JSON report as an artifact** so you can review findings even if the pipeline fails.
- **Set `--min-severity Warning`** if you don't want info-level findings to clutter the output.
