# DocQuality.Analyzers

[![NuGet](https://img.shields.io/nuget/v/DocQuality.Analyzers.svg)](https://www.nuget.org/packages/DocQuality.Analyzers)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
> ⚠️ **Early stage:** This project is not fully tested. Bug reports, feedback, and contributions of any kind are very welcome — see [Contributing](#contributing).

Roslyn analyzers that enforce XML documentation comment quality on public C# members. Catch empty tags, missing parameters, undocumented return values, and more — at compile time.

---

## Motivation

C# already ships two compiler warnings that encourage XML documentation:

| Warning | What it catches |
|---------|-----------------|
| [CS1591](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs1591) | A publicly visible type or member has **no** XML comment at all |
| [CS1573](https://learn.microsoft.com/en-us/dotnet/csharp/misc/cs1573) | A method has XML comments for **some** parameters but not all of them |

These warnings are useful, but they only check whether a comment **exists** — not whether it actually says anything meaningful. In practice, many tools (IDE snippets, AI assistants, "generate XML doc" refactorings) auto-generate documentation stubs like:

```csharp
/// <summary></summary>
/// <param name="value"></param>
/// <returns></returns>
public int Process(int value) { … }
```

This satisfies CS1591 and CS1573 — no compiler warnings — yet the documentation is completely empty. Consumers see blank tooltips and generated API docs with no descriptions.

**DocQuality.Analyzers bridges that gap.** It works alongside CS1591 and CS1573 by verifying that the documentation content is actually filled in: summaries are not empty, every `<param>` tag contains a description, `<returns>` is present and non-blank, and so on. Together, the built-in warnings ensure documentation *exists*, and DocQuality ensures it is *meaningful*.

---

## Diagnostics

| ID | Default Severity | Description |
|----|------------------|-------------|
| DOC001 | Warning | `<summary>` tag is empty or whitespace-only |
| DOC002 | Warning | `<param>` tag is empty or whitespace-only |
| DOC003 | Warning | Parameter has no corresponding `<param>` tag |
| DOC004 | Warning | Non-void member is missing a `<returns>` tag, or the tag is empty |
| DOC005 | Warning | `<exception>` tag is empty or whitespace-only |

### Which diagnostics apply where?

| Declaration kind | DOC001 | DOC002 | DOC003 | DOC004 | DOC005 |
|---|:---:|:---:|:---:|:---:|:---:|
| Class | ✓ | ✓¹ | ✓¹ | | ✓ |
| Struct | ✓ | ✓¹ | ✓¹ | | ✓ |
| Record / Record struct | ✓ | ✓ | ✓ | | ✓ |
| Interface | ✓ | | | | ✓ |
| Enum | ✓ | | | | |
| Delegate | ✓ | ✓ | ✓ | ✓ | ✓ |
| Method | ✓ | ✓ | ✓ | ✓ | ✓ |
| Constructor | ✓ | ✓ | ✓ | | ✓ |
| Property | ✓ | | | | ✓ |
| Indexer | ✓ | ✓ | ✓ | | ✓ |
| Field | ✓ | | | | |
| Event / Event field | ✓ | | | | ✓ |
| Operator | ✓ | ✓ | ✓ | ✓ | ✓ |
| Conversion operator | ✓ | ✓ | ✓ | ✓ | ✓ |

¹ Only when the type has a primary constructor (C# 12+).

All diagnostics target **public members only** and are enabled by default. Members marked with `override` or `<inheritdoc />` are skipped.

---

## Installation

```shell
dotnet add package DocQuality.Analyzers
```

Or via the NuGet Package Manager in Visual Studio.

---

## Usage

Once installed, the analyzer runs automatically as part of your build. Violations appear as warnings (or suggestions) in your IDE and build output.

**Example — DOC001 (empty summary on a class):**
```csharp
/// <summary></summary>  // ⚠️ DOC001: Summary tag is empty
public class MyService { }
```

**Example — DOC001 (empty summary on a method):**
```csharp
/// <summary></summary>  // ⚠️ DOC001: Summary tag is empty
public void DoSomething() { }
```

**Example — DOC003 (missing param on a record):**
```csharp
/// <summary>Represents a person.</summary>
// ⚠️ DOC003: Parameter 'Name' has no corresponding <param> tag
// ⚠️ DOC003: Parameter 'Age' has no corresponding <param> tag
public record Person(string Name, int Age);
```

**Example — DOC004 (missing returns on a method):**
```csharp
/// <summary>Gets the current count.</summary>
// ⚠️ DOC004: Returns a value but has no <returns> tag
public int GetCount() => _count;
```

### Suppressing a diagnostic

You can suppress individual diagnostics with `#pragma` or `[SuppressMessage]`:

```csharp
#pragma warning disable DOC001
/// <summary></summary>
public void DoSomething() { }
#pragma warning restore DOC001
```

---

## Configuration

### Severity

Severity levels can be adjusted in your `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.DOC001.severity = error
dotnet_diagnostic.DOC003.severity = none
```

### Behavior options

The following `.editorconfig` options control which checks are active:

| Option | Default | Description |
|--------|---------|-------------|
| `dotnet_diagnostic.DOC001.require_summary` | `true` | Set to `false` to disable DOC001 entirely (empty `<summary>` tags are allowed). |
| `dotnet_diagnostic.DOC002.require_param` | `true` | Set to `false` to disable DOC003 (missing `<param>` tags are allowed). |
| `dotnet_diagnostic.DOC002.require_param_description` | `true` | Set to `false` to disable DOC002 (empty `<param>` tags are allowed). |
| `dotnet_diagnostic.DOC004.require_returns` | `true` | Set to `false` to disable DOC004 (missing or empty `<returns>` tags are allowed). |

### Generated code exclusion

By default, all diagnostics run on generated files too. You can exclude generated code (files ending in `.g.cs`, `.designer.cs`, `.generated.cs`, or containing `[GeneratedCode]`) per-diagnostic:

| Option | Default |
|--------|---------|
| `dotnet_diagnostic.DOC001.exclude_generated_code` | `true`* |
| `dotnet_diagnostic.DOC002.exclude_generated_code` | `true`* |
| `dotnet_diagnostic.DOC003.exclude_generated_code` | `false` |
| `dotnet_diagnostic.DOC004.exclude_generated_code` | `false` |
| `dotnet_diagnostic.DOC005.exclude_generated_code` | `false` |

*\* These defaults are set in the `.editorconfig` shipped with the package. The analyzer's built-in default is `false`; the packaged `.editorconfig` overrides DOC001 and DOC002 to `true`.*

### Example `.editorconfig`

```ini
[*.cs]
# Promote empty-summary to an error
dotnet_diagnostic.DOC001.severity = error

# Disable the missing-param-tag check
dotnet_diagnostic.DOC003.severity = none

# Don't require <returns> tags
dotnet_diagnostic.DOC004.require_returns = false

# Exclude generated code from DOC005
dotnet_diagnostic.DOC005.exclude_generated_code = true
```

---

## Code fixes

The analyzer ships with code fixes for all five diagnostics:

| Diagnostic | Code fix |
|------------|----------|
| DOC001 | Fills the empty `<summary>` tag with placeholder text |
| DOC002 | Fills the empty `<param>` tag with placeholder text |
| DOC003 | Adds a new `<param>` tag for the undocumented parameter |
| DOC004 | Fills the empty `<returns>` tag, or adds a new one if missing |
| DOC005 | Fills the empty `<exception>` tag with placeholder text |

All code fixes insert `TODO: document this` as placeholder text.

---

## Contributing

Contributions are very welcome! The project is in early development and there's plenty of room to help:

- 🐛 **Bug reports** — open an issue if something doesn't behave as expected
- ✅ **Tests** — test coverage is limited; adding tests for edge cases is a great place to start
- 💡 **New diagnostics** — have an idea for another doc quality rule? Open an issue to discuss it
- 📝 **Documentation** — improvements to this README or inline docs are always appreciated

To get started:

```shell
git clone https://github.com/rubenseggers/DocQuality.Analyzers.git
cd DocQuality.Analyzers
dotnet build
dotnet test
```

Please open an issue before starting significant work, so we can discuss the approach.

---

## License

MIT
