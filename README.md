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

| ID | Severity | Description |
|----|----------|-------------|
| DOC001 | Warning | `<summary>` tag is empty or whitespace-only |
| DOC002 | Warning | `<param>` tag is empty or whitespace-only |
| DOC003 | Warning | Parameter has no corresponding `<param>` tag |
| DOC004 | Warning | Non-void member is missing a `<returns>` tag, or the tag is empty |
| DOC005 | Warning | `<exception>` tag is empty or whitespace-only |

All diagnostics target **public members only** and are enabled by default.

---

## Installation

```shell
dotnet add package DocQuality.Analyzers
```

Or via the NuGet Package Manager in Visual Studio.

---

## Usage

Once installed, the analyzer runs automatically as part of your build. Violations appear as warnings in your IDE and build output.

**Example — DOC001:**
```csharp
/// <summary></summary>  // ⚠️ DOC001: Summary tag is empty
public void DoSomething() { }
```

**Example — DOC003:**
```csharp
/// <summary>Does something important.</summary>
// ⚠️ DOC003: Parameter 'value' has no corresponding <param> tag
public void DoSomething(int value) { }
```

**Example — DOC004:**
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

Severity levels can be adjusted in your `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.DOC001.severity = error
dotnet_diagnostic.DOC003.severity = none
```

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
