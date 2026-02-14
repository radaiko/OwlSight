# OwlSight

AI-powered code review tool that integrates into your development workflow. OwlSight analyzes code changes using LLMs and provides actionable feedback through a CLI or CI/CD pipeline.

## Features

- **CLI Interface**: Run reviews locally with `owlsight review`
- **CI/CD Integration**: Automate reviews in GitHub Actions, GitLab CI, etc.
- **Custom Rules**: Define project-specific review guidelines
- **Agentic Loop**: Iterative review process with LLM-powered tools
- **Multiple Output Formats**: Console and JSON output
- **Docker Support**: Run in containers for consistent environments

## Quick Start

```bash
owlsight init    # Initialize configuration
owlsight review  # Review current changes
```

## Documentation

Full documentation is available at the [OwlSight docs site](https://radaiko.github.io/OwlSight/), covering:

- [Getting Started](https://radaiko.github.io/OwlSight/guide/getting-started.html)
- [Configuration](https://radaiko.github.io/OwlSight/guide/configuration.html)
- [Custom Rules](https://radaiko.github.io/OwlSight/guide/custom-rules.html)
- [CI/CD Setup](https://radaiko.github.io/OwlSight/guide/ci-cd.html)
- [Docker Usage](https://radaiko.github.io/OwlSight/guide/docker.html)

## Tech Stack

- C# / .NET
- VitePress (documentation)
