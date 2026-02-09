---
title: Custom Rules
---

# Custom Rules

OwlSight loads review rules from markdown files in `.owlsight/rules/`. Each file defines one rule that the LLM uses during code review.

---

## Rule File Format

Rules are markdown files with optional YAML front matter:

```markdown
---
title: No Hardcoded Secrets
severity: Critical
category: Security
description: Do not hardcode API keys, passwords, or other secrets in source code.
---
# No Hardcoded Secrets

Check for hardcoded secrets such as API keys, passwords, connection strings,
and tokens in the codebase.

## What to look for
- String literals that look like API keys (long alphanumeric strings)
- Variables named `password`, `secret`, `apiKey` with string literal values
- Connection strings with embedded credentials

## Exceptions
- Test files using obviously fake values (e.g., `test-key-123`)
- Configuration templates with placeholder values
```

---

## Front Matter Fields

| Field | Required | Default | Description |
|-------|----------|---------|-------------|
| `title` | No | First `#` heading or filename | Human-readable rule name |
| `severity` | No | `Warning` | Default severity: `Critical`, `Warning`, `Info`, `Nitpick` |
| `category` | No | `General` | Category for grouping (e.g., Security, Performance, Style) |
| `description` | No | — | Short description of the rule |

If no front matter is provided, the rule ID comes from the filename and the title from the first heading.

---

## How Rules Are Used

Rules are injected into the LLM's system prompt. The LLM sees:

1. **General review instructions** — analyze diffs, check for bugs, security, performance
2. **Your custom rules** — each rule's title, severity, category, and full content
3. **Output format** — JSON schema for findings

The LLM then reviews the diff with your rules in mind. When it finds a violation, it includes the `ruleId` in the finding:

```json
{
  "file": "src/Config.cs",
  "line": 12,
  "severity": "Critical",
  "title": "Hardcoded API key",
  "description": "API key is assigned as a string literal.",
  "suggestion": "Use environment variables or a secrets manager.",
  "ruleId": "no-hardcoded-secrets"
}
```

---

## Example Rules

### No Console.WriteLine

```markdown
---
title: No Console.WriteLine in Production Code
severity: Warning
category: Code Quality
description: Use structured logging instead of Console.WriteLine.
---
# No Console.WriteLine in Production Code

Production code should use `ILogger` instead of `Console.WriteLine`.

## Why
- Console output cannot be filtered or routed to different sinks.
- It bypasses the application's logging configuration.
- No structured data for log aggregation tools.

## Exceptions
- CLI tools where console output is the intended interface.
- Test projects.
```

### Error Handling

```markdown
---
title: Proper Exception Handling
severity: Warning
category: Reliability
description: Catch specific exceptions and include context in error messages.
---
# Proper Exception Handling

## What to check
- Don't catch generic `Exception` without re-throwing
- Don't swallow exceptions with empty catch blocks
- Include context in error messages
- Use `when` filters for conditional catch blocks

## Bad
```csharp
try { DoSomething(); }
catch (Exception) { } // swallowed
```

## Good
```csharp
try { DoSomething(); }
catch (InvalidOperationException ex)
{
    logger.LogError(ex, "Failed to process order {OrderId}", orderId);
    throw;
}
```
```

### API Design

```markdown
---
title: RESTful API Conventions
severity: Info
category: API Design
description: API endpoints should follow RESTful naming conventions.
---
# RESTful API Conventions

- Use plural nouns for resource endpoints (`/users` not `/user`)
- Use HTTP methods correctly (GET for reads, POST for creates, etc.)
- Return appropriate status codes (201 for created, 404 for not found)
- Use consistent naming: kebab-case for URLs, camelCase for JSON properties
```

---

## File Organization

```
.owlsight/
└── rules/
    ├── no-hardcoded-secrets.md
    ├── no-console-writeline.md
    ├── proper-exception-handling.md
    ├── api-conventions.md
    └── sql-injection.md
```

The filename (without `.md`) becomes the rule ID. Use kebab-case for consistency.

---

## Tips

- **Be specific.** The more concrete your rule, the better the LLM can apply it. Include code examples of good and bad patterns.
- **Set appropriate severity.** `Critical` rules trigger a non-zero exit code in CI. Use it for things that must block a merge.
- **Keep rules focused.** One rule per file, one concern per rule. A broad "write good code" rule won't produce useful findings.
- **Include exceptions.** Tell the LLM when it's acceptable to break the rule (e.g., test files, CLI tools).
