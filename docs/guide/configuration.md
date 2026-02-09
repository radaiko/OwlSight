---
title: Configuration
---

# Configuration

OwlSight loads configuration from three sources, with later sources overriding earlier ones:

```
.owlsight/config.json  →  Environment variables  →  CLI arguments
```

---

## Config File

The config file lives at `.owlsight/config.json` in your repository root. Create it with `owlsight init` or write it manually.

```json
{
  "llm": {
    "baseUrl": "https://api.openai.com/v1",
    "model": "gpt-4o",
    "maxTokens": 4096,
    "temperature": 0.2,
    "maxToolRoundtrips": 15
  },
  "review": {
    "minSeverity": "Info",
    "excludePatterns": [
      "**/bin/**",
      "**/obj/**",
      "**/*.generated.cs",
      "**/node_modules/**"
    ],
    "maxFilesPerBatch": 10
  }
}
```

---

## Config Reference

### LLM Settings

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `llm.baseUrl` | `string` | `https://api.openai.com/v1` | OpenAI-compatible API base URL |
| `llm.apiKey` | `string` | — | API key (prefer env var or CLI arg) |
| `llm.model` | `string` | `gpt-4o` | Model name |
| `llm.maxTokens` | `int` | `4096` | Maximum output tokens per LLM call |
| `llm.temperature` | `float` | `0.2` | Sampling temperature (lower = more deterministic) |
| `llm.maxToolRoundtrips` | `int` | `15` | Maximum tool-calling iterations per batch |

### Review Settings

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `review.minSeverity` | `string` | `Info` | Minimum severity to report: `Critical`, `Warning`, `Info`, `Nitpick` |
| `review.excludePatterns` | `string[]` | `[]` | Glob patterns for files to skip |
| `review.maxFilesPerBatch` | `int` | `10` | Maximum files sent to the LLM per batch |

---

## Environment Variables

| Variable | Maps to |
|----------|---------|
| `OWLSIGHT_API_KEY` | `llm.apiKey` |
| `OWLSIGHT_BASE_URL` | `llm.baseUrl` |
| `OWLSIGHT_MODEL` | `llm.model` |

Environment variables override the config file but are overridden by CLI arguments.

```bash
export OWLSIGHT_API_KEY=sk-...
export OWLSIGHT_MODEL=gpt-4o-mini
owlsight review --base main
```

---

## CLI Arguments

CLI arguments have the highest priority. See the [review command reference](../cli/review) for all options.

```bash
owlsight review --base main \
  --api-key sk-... \
  --base-url http://localhost:11434/v1 \
  --model llama3 \
  --min-severity Warning \
  --max-files-per-batch 5
```

---

## Exclude Patterns

Use glob patterns to skip files from review. Patterns are matched against the relative file path.

```json
{
  "review": {
    "excludePatterns": [
      "**/bin/**",
      "**/obj/**",
      "**/node_modules/**",
      "**/*.generated.cs",
      "**/*.Designer.cs",
      "**/Migrations/**",
      "**/*.min.js",
      "**/*.lock"
    ]
  }
}
```

---

## Batching

When many files change, OwlSight groups them into batches to manage the LLM's context window. Each batch is reviewed independently, and findings are aggregated at the end.

The `maxFilesPerBatch` setting controls batch size. Lower values mean smaller context per call (better for models with limited context windows), while higher values provide more cross-file context.

| Model | Recommended `maxFilesPerBatch` |
|-------|-------------------------------|
| GPT-4o (128k context) | 10–15 |
| GPT-4o-mini | 8–10 |
| Llama 3 (8k) | 3–5 |
| Local models | 3–5 |

---

## Precedence Example

Given this config file:

```json
{ "llm": { "model": "gpt-4o", "baseUrl": "https://api.openai.com/v1" } }
```

And this environment:

```bash
export OWLSIGHT_MODEL=gpt-4o-mini
```

And this command:

```bash
owlsight review --base main --model llama3
```

The effective model is `llama3` (CLI wins over env var, which wins over config file).
