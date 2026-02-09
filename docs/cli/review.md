---
title: review
---

# `owlsight review`

Run AI-powered code review on changes between the current branch and a base branch.

```bash
owlsight review --base <branch> [options]
```

---

## Options

| Option | Alias | Required | Default | Description |
|--------|-------|----------|---------|-------------|
| `--base` | `-b` | Yes | — | Base branch to diff against |
| `--base-url` | | No | `https://api.openai.com/v1` | LLM API base URL |
| `--api-key` | | No | — | LLM API key (or set `OWLSIGHT_API_KEY`) |
| `--model` | `-m` | No | `gpt-4o` | LLM model name |
| `--output` | `-o` | No | — | JSON output file path |
| `--min-severity` | | No | `Info` | Minimum severity: `Critical`, `Warning`, `Info`, `Nitpick` |
| `--max-files-per-batch` | | No | `10` | Maximum files per review batch |
| `--max-tool-roundtrips` | | No | `15` | Maximum tool-calling iterations |
| `--working-dir` | `-d` | No | Current directory | Working directory |

---

## Examples

### Basic Review

```bash
$ owlsight review --base main --api-key sk-...
```

### Review with Local LLM

```bash
$ owlsight review -b main \
    --base-url http://localhost:11434/v1 \
    --model llama3 \
    --api-key ollama
```

### Review with JSON Report

```bash
$ owlsight review -b origin/develop \
    --api-key sk-... \
    --output review-report.json
```

### Only Show Warnings and Above

```bash
$ owlsight review -b main \
    --api-key sk-... \
    --min-severity Warning
```

### Review a Different Directory

```bash
$ owlsight review -b main \
    --api-key sk-... \
    -d /path/to/repo
```

---

## Console Output

Findings are grouped by file, colored by severity:

```
  src/UserService.cs
  CRITICAL  SQL injection vulnerability (src/UserService.cs:42-45)
    User input is interpolated directly into SQL query.
    Suggestion: Use parameterized queries instead.
    Rule: sql-injection

  WARNING   Missing null check (src/UserService.cs:28)
    GetUser() may return null but caller does not check.
    Suggestion: Add null check or use the null-conditional operator.

  src/Controllers/UserController.cs
  INFO      Consider using async/await (src/Controllers/UserController.cs:15)
    Synchronous database call in controller action.
    Suggestion: Use the async version of the database method.

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

## JSON Output

When `--output` is specified, a JSON report is written with this schema:

```json
{
  "version": "1.0.0",
  "timestamp": "2026-02-09T15:30:00+00:00",
  "summary": {
    "totalFindings": 3,
    "bySeverity": {
      "Critical": 1,
      "Warning": 1,
      "Info": 1
    },
    "reviewedFilesCount": 4,
    "batchCount": 1
  },
  "findings": [
    {
      "file": "src/UserService.cs",
      "line": 42,
      "endLine": 45,
      "severity": "Critical",
      "title": "SQL injection vulnerability",
      "description": "User input is interpolated directly into SQL query.",
      "suggestion": "Use parameterized queries instead.",
      "ruleId": "sql-injection"
    }
  ]
}
```

### Finding Fields

| Field | Type | Description |
|-------|------|-------------|
| `file` | `string` | Relative path to the file |
| `line` | `int?` | Start line number |
| `endLine` | `int?` | End line number |
| `severity` | `string` | `Critical`, `Warning`, `Info`, or `Nitpick` |
| `title` | `string` | Short description of the issue |
| `description` | `string` | Detailed explanation |
| `suggestion` | `string?` | How to fix the issue |
| `ruleId` | `string?` | ID of the matched custom rule |

---

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Review passed — no critical findings |
| `1` | Review failed — one or more critical findings |
| `2` | Error — missing API key, LLM failure, git error, etc. |

Use exit code `1` as a CI/CD gate:

```bash
owlsight review --base main --api-key $KEY || echo "Review failed"
```

---

## How It Works

1. **Diff** — runs `git diff <base-branch>` to get changed files
2. **Filter** — excludes files matching patterns in `review.excludePatterns`
3. **Batch** — groups files into batches of `maxFilesPerBatch`
4. **Review** — for each batch, runs the [agentic loop](../guide/agentic-loop):
   - Sends diff + rules to the LLM
   - LLM uses [tools](../guide/llm-tools) to investigate code context
   - LLM returns structured findings
5. **Aggregate** — collects findings from all batches
6. **Filter** — removes findings below `minSeverity`
7. **Output** — writes console and/or JSON output
