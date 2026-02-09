---
title: Agentic Loop
---

# Agentic Loop

OwlSight uses a manual agentic loop to give the LLM full code investigation capabilities during review. Instead of just analyzing the diff in isolation, the LLM can actively explore the codebase.

---

## How It Works

```
ReviewEngine.RunAsync()
  ├── Load config, rules, git diff
  ├── Filter and batch changed files
  └── For each batch → AgenticLoop.RunAsync():
      ├── Build messages: system prompt + rules + diffs
      └── LOOP (max N iterations):
          ├── Call LLM with messages + available tools
          ├── If tool calls → execute each, append results, continue
          ├── If text response → parse JSON findings, break
          └── If max iterations → break with warning
```

---

## Why Manual Loop?

OwlSight implements the tool-calling loop manually rather than using framework-level auto-invocation. This gives full control over:

- **Logging** — every tool call and result is logged for debugging
- **Iteration limits** — configurable max roundtrips prevent runaway costs
- **Error recovery** — tool failures return error messages to the LLM instead of crashing
- **Result truncation** — large tool outputs are truncated to manage context window size
- **JSON parsing** — the final response is parsed from flexible formats (raw JSON, markdown code blocks)

---

## Flow Detail

### 1. System Prompt

The system prompt includes:

- **Instructions** — how to analyze code, what to look for, output format
- **Custom rules** — all rules from `.owlsight/rules/*.md` with their full content
- **Output schema** — exact JSON format for findings

### 2. User Prompt

The user prompt contains the diff for the current batch of files, formatted as:

```
## File: src/Example.cs (Modified)
```diff
@@ -10,6 +10,8 @@ namespace Example
     public void DoSomething()
     {
         var x = 1;
+        var y = 2;
+        Console.WriteLine(x + y);
         return;
     }
```
```

### 3. Tool Calls

The LLM can request tool calls at any point. Common patterns:

- **Reading surrounding code** — `read_file_lines` to see context around a changed line
- **Checking related files** — `read_file` to understand an interface or base class
- **Searching for patterns** — `search_text` to find similar patterns elsewhere
- **Understanding structure** — `get_file_structure` to see the project layout
- **Checking authorship** — `get_git_blame` to understand when code was changed
- **Reviewing history** — `get_git_log` to understand recent changes

### 4. Final Output

When the LLM is done investigating, it returns a JSON response:

```json
{
  "findings": [
    {
      "file": "src/Example.cs",
      "line": 13,
      "severity": "Warning",
      "title": "Console.WriteLine in production code",
      "description": "Console.WriteLine should not be used in production.",
      "suggestion": "Use ILogger instead.",
      "ruleId": "no-console-writeline"
    }
  ]
}
```

---

## Configuration

| Setting | Default | Description |
|---------|---------|-------------|
| `llm.maxToolRoundtrips` | `15` | Maximum tool-calling iterations per batch |
| `llm.maxTokens` | `4096` | Maximum output tokens per LLM call |
| `llm.temperature` | `0.2` | Lower = more deterministic, more consistent findings |
| `review.maxFilesPerBatch` | `10` | Files per batch (affects context window usage) |

---

## Safety

- **Path traversal prevention** — every tool validates that resolved paths stay within the working directory
- **Output truncation** — tool results over 30,000 characters are truncated
- **File size limits** — `read_file` truncates files over 50,000 characters
- **Search result limits** — `search_text` returns at most 100 matches
- **Regex timeout** — search patterns have a 5-second execution timeout
