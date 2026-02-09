---
title: LLM Tools
---

# LLM Tools

During review, the LLM has access to seven tools for investigating the codebase. These tools let the AI read files, search code, explore directory structure, and check git history — all while staying safely within the repository boundary.

---

## Quick Reference

| Tool | Description |
|------|-------------|
| [`read_file`](#read-file) | Read entire file contents |
| [`read_file_lines`](#read-file-lines) | Read specific line range |
| [`list_files`](#list-files) | List files with optional glob pattern |
| [`search_text`](#search-text) | Regex search across files |
| [`get_file_structure`](#get-file-structure) | Directory tree view |
| [`get_git_blame`](#get-git-blame) | Git blame for authorship info |
| [`get_git_log`](#get-git-log) | Recent commit history |

---

## `read_file`

Read the entire contents of a file.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `path` | `string` | Yes | Relative path to the file |

Files over 50,000 characters are truncated. Returns an error if the file doesn't exist.

**Use case:** The LLM reads a file to understand an interface, base class, or configuration that the changed code depends on.

---

## `read_file_lines`

Read specific lines from a file (1-indexed, inclusive).

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `path` | `string` | Yes | Relative path to the file |
| `startLine` | `int` | Yes | Start line number (1-indexed) |
| `endLine` | `int` | Yes | End line number (inclusive) |

Returns lines prefixed with line numbers (e.g., `42: var x = 1;`).

**Use case:** The LLM reads context around a changed line to understand if a change is safe, or reads a specific method body.

---

## `list_files`

List files in a directory, optionally filtered by a glob pattern.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `directory` | `string` | No | Relative directory path (defaults to root) |
| `pattern` | `string` | No | Glob pattern (e.g., `*.cs`, `**/*.json`) |

Returns at most 200 entries, sorted alphabetically.

**Use case:** The LLM explores the project structure to find related files, test files, or configuration.

---

## `search_text`

Search for a text or regex pattern in files.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `pattern` | `string` | Yes | Regex pattern to search for |
| `path` | `string` | No | Directory to search in (defaults to root) |
| `filePattern` | `string` | No | Glob pattern to filter files (e.g., `*.cs`) |

Returns at most 100 matches in `file:line: content` format. Regex patterns have a 5-second timeout.

**Use case:** The LLM searches for usages of a method, checks if a pattern exists elsewhere, or finds related code.

---

## `get_file_structure`

Get the directory tree structure.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `path` | `string` | No | Directory to list (defaults to root) |

Returns a tree view up to 3 levels deep, excluding hidden files/directories.

```
├── src/
│   ├── Controllers/
│   │   ├── UserController.cs
│   │   └── OrderController.cs
│   ├── Services/
│   │   ├── UserService.cs
│   │   └── OrderService.cs
│   └── Models/
│       └── User.cs
└── tests/
    └── UserServiceTests.cs
```

**Use case:** The LLM understands the project layout to find related files or understand naming conventions.

---

## `get_git_blame`

Get git blame information for a file.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `path` | `string` | Yes | Relative path to the file |
| `startLine` | `int` | No | Start line number |
| `endLine` | `int` | No | End line number |

Returns standard `git blame` output showing who last modified each line and when.

**Use case:** The LLM checks if a problematic pattern was recently introduced or has existed for a long time, or identifies who to ask about unclear code.

---

## `get_git_log`

Get recent git commit log entries.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `path` | `string` | No | File path to filter log |
| `count` | `int` | No | Number of entries (default 10) |

Returns `git log --oneline` output.

**Use case:** The LLM reviews recent changes to understand the context of modifications, or checks if a file has been actively worked on.

---

## Security

All tools enforce **path traversal prevention**. Every path is resolved to an absolute path and validated to stay within the repository working directory. Attempts to access files outside the repository (e.g., `../../etc/passwd`) are rejected with an error message.

```
read_file("../../etc/passwd")
→ Error: Access denied: path '../../etc/passwd' resolves outside the working directory.
```

This applies to all tools that accept path parameters.
