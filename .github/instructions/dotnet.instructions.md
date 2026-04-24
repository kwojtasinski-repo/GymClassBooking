---
applyTo: "**/*.cs,**/*.csproj"
description: "Use when writing, reviewing, or modifying any C# code or project file. Covers .NET 10 conventions: nullable reference types, implicit usings, async/await rules, DI registration, record DTOs, Clean Architecture layer dependencies, and package versioning for this project."
---

# .NET 10 Coding Instructions

## Target framework

All projects target `net10.0`. Never downgrade to an earlier TFM.

## Nullable reference types

All projects have `<Nullable>enable</Nullable>`. Every reference type must be non-nullable unless explicitly opted out with `?`. Never suppress the nullable warning with `!` unless you can prove the value is non-null at that point.

## Implicit usings

All projects have `<ImplicitUsings>enable</ImplicitUsings>`. Do not add redundant `using System;`, `using System.Collections.Generic;`, or `using System.Threading.Tasks;`.

## Async/await

- All database and I/O methods must be `async` and return `Task` or `Task<T>`.
- Use `ConfigureAwait(false)` only in library code, not in ASP.NET Core or Blazor.
- Never use `.Result` or `.Wait()` — always `await`.

## Dependency injection

- Register services in the project's `DependencyInjection.cs` extension method.
- Never call `new` on a service that has dependencies — always resolve via DI.
- Prefer `AddScoped` for services that touch the database; `AddSingleton` for stateless services.

## Record types for DTOs

Use `record` types for request/response DTOs. They are immutable by default and support structural equality.

```csharp
public record MyRequest(string Name, int Count);
```

## Clean Architecture layer rules

| Layer          | May depend on               | May NOT depend on                |
| -------------- | --------------------------- | -------------------------------- |
| Domain         | nothing                     | Application, Infrastructure, API |
| Application    | Domain                      | Infrastructure, API              |
| Infrastructure | Domain, Application         | API                              |
| API            | Application, Infrastructure | —                                |

Violating these rules must be justified with a new ADR.

## Package versioning

Use `10.0.x` versions for all `Microsoft.*` packages (EF Core, Extensions, ASP.NET Core). Keep non-Microsoft packages (xunit, Moq, MudBlazor) at their latest stable version compatible with net10.0.
