---
description: "Use when adding or modifying a domain entity, value object, aggregate, or domain enum. Covers entity structure, EF Core model configuration, AppDbContext DbSet registration, and domain layer conventions for this project."
applyTo: "src/GymClassBooking.Domain/**"
---

# Domain Entity Rules

Read `.github/skills/create-domain-entity/SKILL.md` before generating any entity or enum code.

## Key rules (summary)

- Entities live in `src/GymClassBooking.Domain/Entities/`. Enums in `Enums/`.
- Domain has zero dependencies — no references to Infrastructure, Application, or API.
- After adding an entity: register a `DbSet<T>` in `AppDbContext` and `IAppDbContext`.
- Use `int Id` as primary key. Navigation properties use `virtual` only if lazy loading is needed (it isn't — avoid).
- Enums use `int` backing type. Always add a `None = 0` default value.
