---
name: create-dto
description: "Use when adding or modifying DTOs, request/response records, or summary models. Covers record conventions, naming patterns, mapping rules, and Web-side mirror models for this project."
---

# Skill: Create DTO

Use this skill when adding new Data Transfer Objects to `src/GymClassBooking.Application/DTOs/`.

## Rules

- DTOs are `record` types — immutable, value-based equality, positional constructor.
- DTOs live in the Application layer — they cross the boundary between Application and API/Web.
- Name conventions:
  - `{Entity}Request` — incoming data (POST/PUT body).
  - `{Entity}Response` — outgoing data (GET/POST response body).
  - `{Entity}Summary` — lightweight read model for list endpoints.
- Domain entities are **never** returned directly from services — always map to a DTO.

## File location

All DTOs for a feature live in one file:

```
src/GymClassBooking.Application/DTOs/{Feature}Dtos.cs
```

## Templates

### Request DTO (incoming)

```csharp
public record {Entity}Request(
    int {ForeignKeyId},
    string {RequiredField},
    int {ValueField});
```

### Response DTO (outgoing — full detail)

```csharp
public record {Entity}Response(
    int Id,
    int {ForeignKeyId},
    string {ForeignKeyName},        // denormalized for convenience
    string {Field},
    DateTime CreatedAt,
    string Status,                  // enum rendered as string
    int? {OptionalField});          // nullable for optional values
```

### Summary DTO (outgoing — list item, fewer fields)

```csharp
public record {Entity}Summary(
    int Id,
    string Name,
    DateTime StartsAt,
    int {CountField});
```

## Mapping pattern (inside the service)

Map directly in the service using the record constructor — no AutoMapper:

```csharp
private static {Entity}Response Map({Entity} e) => new(
    e.Id,
    e.{ForeignKeyId},
    e.{NavProp}?.Name ?? string.Empty,
    e.{Field},
    e.CreatedAt,
    e.Status.ToString(),
    e.{OptionalField});
```

## Rules

- Use `string` for enum values in responses (`.ToString()`) — avoids integer deserialization issues in Blazor.
- Use `int?` (nullable) for optional fields like `WaitlistPosition` — do not use sentinel values (-1).
- Mirror the DTO fields in `src/GymClassBooking.Web/Models/ApiModels.cs` so the Blazor client can deserialize them.
- Keep Request DTOs minimal — only include fields that the caller can legitimately set.

## Web mirror (ApiModels.cs)

After adding a DTO to the Application layer, add a matching `record` in:

```
src/GymClassBooking.Web/Models/ApiModels.cs
```

The Web model must have the same property names and types to enable `ReadFromJsonAsync<T>()`.

## Do NOT

- Add validation attributes to DTOs — use FluentValidation or inline service checks.
- Include navigation properties or `ICollection<T>` in DTOs — project only scalar values.
- Reuse the same DTO for request and response — keep them separate.
