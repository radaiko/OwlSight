---
title: Docker
---

# Docker

OwlSight ships with a Dockerfile for containerized usage. The image includes the .NET runtime and Git.

---

## Build

```bash
cd OwlSight
docker build -t owlsight .
```

The multi-stage Dockerfile:

1. **Build stage** — Uses `dotnet/sdk:10.0-preview` to restore and publish
2. **Runtime stage** — Uses `dotnet/runtime:10.0-preview` with Git installed

---

## Usage

Mount your repository at `/repo` and pass review options as arguments:

```bash
docker run --rm \
  -v $(pwd):/repo \
  -e OWLSIGHT_API_KEY=$OPENAI_API_KEY \
  owlsight review --base main
```

### With JSON Output

```bash
docker run --rm \
  -v $(pwd):/repo \
  -e OWLSIGHT_API_KEY=$OPENAI_API_KEY \
  owlsight review --base main --output /repo/report.json
```

The report is written inside the mounted volume, so it persists after the container exits.

### With a Local LLM

If your LLM runs on the host machine, use `host.docker.internal` (macOS/Windows) or `--network host` (Linux):

```bash
# macOS / Windows
docker run --rm \
  -v $(pwd):/repo \
  owlsight review --base main \
  --base-url http://host.docker.internal:11434/v1 \
  --api-key ollama \
  --model llama3

# Linux
docker run --rm --network host \
  -v $(pwd):/repo \
  owlsight review --base main \
  --base-url http://localhost:11434/v1 \
  --api-key ollama \
  --model llama3
```

### Initialize Config

```bash
docker run --rm \
  -v $(pwd):/repo \
  owlsight init
```

---

## CI/CD with Docker

See the [CI/CD Integration](./ci-cd) guide for pipeline-specific examples using Docker.

```yaml
# GitHub Actions example
- name: Run Review
  run: |
    docker run --rm \
      -v ${{ github.workspace }}:/repo \
      -e OWLSIGHT_API_KEY=${{ secrets.OPENAI_API_KEY }} \
      owlsight review --base origin/main --output /repo/report.json
```

---

## Environment Variables

Pass environment variables with `-e`:

```bash
docker run --rm \
  -v $(pwd):/repo \
  -e OWLSIGHT_API_KEY=sk-... \
  -e OWLSIGHT_MODEL=gpt-4o-mini \
  -e OWLSIGHT_BASE_URL=https://api.openai.com/v1 \
  owlsight review --base main
```

See [Configuration](./configuration#environment-variables) for all supported variables.
