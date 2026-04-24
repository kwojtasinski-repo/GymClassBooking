---
name: create-domain-entity
description: "Use when adding or modifying a domain entity, enum, or value object. Covers entity structure, EF Core model configuration, AppDbContext DbSet registration, and domain layer conventions for this project."
---

# Skill: Create Domain Entity

Use this skill when adding a new entity to `src/GymClassBooking.Domain/Entities/`.

## Checklist before generating

1. Identify required properties — which are required, which are optional.
2. Identify relationships — does this entity have a collection of another? Is it owned by another?
3. Decide primary key type — use `int Id` (auto-incremented by EF Core InMemory).
4. Decide if any enum belongs in `src/GymClassBooking.Domain/Enums/`.

## File locations

```
src/GymClassBooking.Domain/Entities/{Name}.cs
src/GymClassBooking.Domain/Enums/{EnumName}.cs   (if needed)
```

## Entity template

```csharp
namespace GymClassBooking.Domain.Entities;

public class {Name}
{
    public int Id { get; set; }

    // Required scalar properties — use required keyword for strings
    public required string Name { get; set; }

    // Optional scalar
    public string? Description { get; set; }

    // Value type with default
    public int Count { get; set; }

    // Navigation: owns many
    public ICollection<{Child}> {Children} { get; set; } = [];

    // Navigation: owned by (FK + nav)
    public int {Parent}Id { get; set; }
    public {Parent} {Parent} { get; set; } = null!;
}
```

## Enum template

```csharp
namespace GymClassBooking.Domain.Enums;

public enum {Name}
{
    {Value1} = 0,
    {Value2} = 1,
    {Value3} = 2
}
```

## EF Core model configuration (in `AppDbContext.OnModelCreating`)

Add inside `OnModelCreating`:

```csharp
modelBuilder.Entity<{Name}>(e =>
{
    e.HasKey(x => x.Id);
    e.Property(x => x.Name).IsRequired().HasMaxLength(200);

    // Unique index (if needed)
    e.HasIndex(x => x.UniqueField).IsUnique();

    // Relationship: many-to-one
    e.HasOne(x => x.{Parent})
     .WithMany(p => p.{Children})
     .HasForeignKey(x => x.{Parent}Id)
     .OnDelete(DeleteBehavior.Cascade);
});
```

Also add `DbSet<{Name}> {Name}s => Set<{Name}>();` to `AppDbContext` and `IAppDbContext`.

## Rules

- **Domain has zero dependencies** — no EF Core, no Application, no API references in Domain.
- Use `= null!` for required navigations that EF Core will always populate.
- Use `ICollection<T>` initialized to `[]` for collection navigations.
- Use `int Id` (not Guid) — consistent with existing entities.
- Add the new `DbSet` to `IAppDbContext` interface and `AppDbContext` implementation.

## Do NOT

- Add business logic to entities — that belongs in Application services.
- Add data annotations (`[Required]`, `[MaxLength]`) — use Fluent API in `AppDbContext`.
- Add constructors that EF Core can't use — EF Core needs a parameterless constructor (implicit or explicit).
