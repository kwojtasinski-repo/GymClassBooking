---
description: "Use when adding or modifying a Blazor WebAssembly page, razor component, UI feature, navigation, or frontend layout. Covers page structure, GymApiClient usage, MudBlazor components, route parameters, and Blazor WASM conventions for this project."
applyTo: "src/GymClassBooking.Web/**"
---

# Blazor Page Rules

Read `.github/skills/create-blazor-page/SKILL.md` before generating any Blazor/Razor code.

## Key rules (summary)

- Pages live in `src/GymClassBooking.Web/Pages/`. Use `@page "/route"`.
- All API calls go through `GymApiClient` — never raw `HttpClient`, never EF Core.
- Use MudBlazor components (`MudCard`, `MudTable`, `MudChip`, `MudSnackbar`) — no raw HTML Bootstrap.
- Inject `GymApiClient` and `ISnackbar` via `@inject`. Load data in `OnInitializedAsync()`.
- Models mirror API DTOs — live in `src/GymClassBooking.Web/Models/ApiModels.cs`.
