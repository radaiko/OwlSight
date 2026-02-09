---
title: init
---

# `owlsight init`

Scaffold the `.owlsight/` configuration directory in your repository.

```bash
owlsight init [options]
```

---

## Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--working-dir` | `-d` | Current directory | Working directory to initialize |

---

## What It Creates

```
.owlsight/
├── config.json                 # Review configuration
└── rules/
    └── no-console-writeline.md # Example review rule
```

### `config.json`

A default configuration file:

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

### Example Rule

An example rule file demonstrating the markdown rule format with YAML front matter.

See [Custom Rules](../guide/custom-rules) for how to write your own.

---

## Examples

### Initialize Current Directory

```bash
$ owlsight init
Initialized .owlsight/ directory
  Config:  .owlsight/config.json
  Rules:   .owlsight/rules/

Edit .owlsight/config.json to customize your settings.
Add review rules as markdown files in .owlsight/rules/.
```

### Initialize a Different Directory

```bash
$ owlsight init -d /path/to/repo
Initialized .owlsight/ directory
  Config:  .owlsight/config.json
  Rules:   .owlsight/rules/
```

### Re-running Init

Running `init` on an already-initialized directory is safe. Existing files are not overwritten:

```bash
$ owlsight init
Warning: .owlsight directory already exists.
Initialized .owlsight/ directory
  Config:  .owlsight/config.json
  Rules:   .owlsight/rules/
```

---

## Next Steps

After initializing:

1. Edit `.owlsight/config.json` to set your LLM provider and model
2. Add custom review rules in `.owlsight/rules/`
3. Run `owlsight review --base main --api-key $KEY` to test
4. Commit `.owlsight/` to your repository so the team shares the same config
