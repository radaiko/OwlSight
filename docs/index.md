---
title: Home
---

# OwlSight

**On-premise AI-powered code review for CI/CD pipelines.**

OwlSight diffs your changes against a base branch, sends them to an OpenAI-compatible LLM with agentic tool-use capabilities, and produces detailed review findings — all without sending code to third-party services.

```
$ owlsight review --base main --model gpt-4o

  src/UserService.cs
  CRITICAL  SQL injection vulnerability (src/UserService.cs:42-45)
    User input is interpolated directly into SQL query.
    Suggestion: Use parameterized queries instead.

  WARNING   Missing null check (src/UserService.cs:28)
    GetUser() may return null but caller does not check.

  ╭──────────────────────────────╮
  │ Review Summary               │
  ├──────────────┬───────────────┤
  │ Files        │ 4             │
  │ Findings     │ 3             │
  │ Critical     │ 1             │
  │ Warning      │ 1             │
  │ Info         │ 1             │
  ╰──────────────┴───────────────╯

  Review FAILED — critical issues found.
```

---

## Why OwlSight?

| Need | OwlSight |
|------|----------|
| Keep code on-premise | Runs against any OpenAI-compatible API — local Ollama, Azure OpenAI, self-hosted vLLM |
| CI/CD gating | Exit code 1 on critical findings — standard pipeline gate |
| Context-aware reviews | Agentic loop lets the LLM read files, search code, check git blame |
| Custom rules | Drop markdown files in `.owlsight/rules/` to enforce team standards |
| No vendor lock-in | Works with any model that supports tool calling |

---

## Quick Start

### Install

```bash
# Clone and build
git clone https://github.com/radaiko/OwlSight.git
cd OwlSight
dotnet build

# Or pull the Docker image
docker build -t owlsight .
```

### Initialize

```bash
owlsight init
```

Creates `.owlsight/config.json` and `.owlsight/rules/` with an example rule.

### Run a Review

```bash
owlsight review --base main --api-key $OPENAI_API_KEY --model gpt-4o
```

See [Getting Started](./guide/getting-started) for the full setup walkthrough.

---

## Features

### AI Code Review

| Feature | Description |
|---------|-------------|
| [`owlsight review`](./cli/review) | Review changes against a base branch |
| [`owlsight init`](./cli/init) | Scaffold configuration and example rules |
| [Custom rules](./guide/custom-rules) | Project-specific review rules in markdown |
| [Agentic loop](./guide/agentic-loop) | LLM investigates code context via tool calls |
| [JSON reports](./cli/review#json-output) | Machine-readable output for CI integration |

### LLM Tools

The LLM can call these tools to investigate code context during review:

| Tool | Description |
|------|-------------|
| `read_file` | Read entire file contents |
| `read_file_lines` | Read specific line range |
| `list_files` | List files with optional glob pattern |
| `search_text` | Regex search across files |
| `get_file_structure` | Directory tree view |
| `get_git_blame` | Git blame for authorship info |
| `get_git_log` | Recent commit history |

See [LLM Tools](./guide/llm-tools) for details.

### Output

- **Console** — Spectre.Console colored output with severity, file:line, title, description, suggestion
- **JSON** — Structured report with version, timestamp, summary, findings array
- **Exit code** — `0` = pass, `1` = critical findings, `2` = error

---

## Supported LLM Providers

Any OpenAI-compatible API endpoint works. Tested with:

| Provider | `--base-url` |
|----------|-------------|
| OpenAI | `https://api.openai.com/v1` (default) |
| Azure OpenAI | `https://<name>.openai.azure.com/openai/deployments/<deployment>` |
| Ollama | `http://localhost:11434/v1` |
| vLLM | `http://localhost:8000/v1` |
| LM Studio | `http://localhost:1234/v1` |

---

## Requirements

| Requirement | Version |
|-------------|---------|
| .NET | 10.0+ |
| Git | Any recent version |
| LLM API | OpenAI-compatible with tool-calling support |

---

## Learn More

- [Getting Started](./guide/getting-started) — Full setup walkthrough
- [Configuration](./guide/configuration) — Config file, environment variables, CLI args
- [Custom Rules](./guide/custom-rules) — Writing project-specific review rules
- [CI/CD Integration](./guide/ci-cd) — GitHub Actions, GitLab CI, Azure DevOps
- [Docker](./guide/docker) — Running OwlSight in containers
- [CLI Reference](./cli/review) — Complete command reference
- [Agentic Loop](./guide/agentic-loop) — How the AI review process works
- [LLM Tools](./guide/llm-tools) — Tools available to the LLM during review
- [Source Code](https://github.com/radaiko/OwlSight)
