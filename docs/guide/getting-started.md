---
title: Getting Started
---

# Getting Started

Set up OwlSight and run your first AI code review in under five minutes.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Git installed and available on `PATH`
- Access to an OpenAI-compatible LLM API (OpenAI, Ollama, Azure OpenAI, etc.)

---

## Install

### From Source

```bash
git clone https://github.com/radaiko/OwlSight.git
cd OwlSight
dotnet build
```

The CLI binary is at `src/OwlSight.Cli/bin/Debug/net10.0/OwlSight.Cli.dll`. Run with:

```bash
dotnet run --project src/OwlSight.Cli -- <command>
```

### Docker

```bash
docker build -t owlsight .
```

See [Docker guide](./docker) for usage.

---

## Initialize a Project

Navigate to your git repository and run:

```bash
owlsight init
```

```
Initialized .owlsight/ directory
  Config:  .owlsight/config.json
  Rules:   .owlsight/rules/

Edit .owlsight/config.json to customize your settings.
Add review rules as markdown files in .owlsight/rules/.
```

This creates:

```
.owlsight/
├── config.json                 # Review configuration
└── rules/
    └── no-console-writeline.md # Example review rule
```

---

## Run Your First Review

Make some changes on a branch, then run:

```bash
owlsight review --base main --api-key $OPENAI_API_KEY
```

OwlSight will:

1. **Diff** your current branch against `main`
2. **Load** custom rules from `.owlsight/rules/`
3. **Send** the diff to the LLM with tool-calling capabilities
4. **Review** — the LLM analyzes changes, uses tools to read surrounding code, and produces findings
5. **Output** findings to the console with severity, file:line, description, and suggestions

### Example Output

```
  src/Services/AuthService.cs
  CRITICAL  Hardcoded API key (src/Services/AuthService.cs:15)
    API key is hardcoded in source code.
    Suggestion: Use environment variables or a secrets manager.

  WARNING   Missing input validation (src/Controllers/UserController.cs:42)
    User input passed directly to service without validation.
    Suggestion: Add input validation before processing.

  ╭──────────────────────────────╮
  │ Review Summary               │
  ├──────────────┬───────────────┤
  │ Files        │ 3             │
  │ Findings     │ 2             │
  │ Critical     │ 1             │
  │ Warning      │ 1             │
  ╰──────────────┴───────────────╯

  Review FAILED — critical issues found.
```

---

## Using a Local LLM

OwlSight works with any OpenAI-compatible API. To use [Ollama](https://ollama.ai):

```bash
# Start Ollama with a model that supports tool calling
ollama serve

# Run review against the local endpoint
owlsight review --base main \
  --base-url http://localhost:11434/v1 \
  --model llama3 \
  --api-key ollama
```

---

## Saving a JSON Report

Add `--output` to write a machine-readable JSON report:

```bash
owlsight review --base main --api-key $KEY --output report.json
```

The JSON report includes version, timestamp, summary counts, and the full findings array. See [JSON Output](../cli/review#json-output) for the schema.

---

## Next Steps

- [Configuration](./configuration) — Customize the config file, use environment variables
- [Custom Rules](./custom-rules) — Write project-specific review rules
- [CI/CD Integration](./ci-cd) — Add OwlSight to your pipeline
- [CLI Reference](../cli/review) — Full command options
