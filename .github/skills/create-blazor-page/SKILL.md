---
name: create-blazor-page
description: "Use when adding or modifying a Blazor WebAssembly page, razor component, or UI feature. Covers page structure, GymApiClient usage, MudBlazor components, route parameters, and Blazor WASM conventions for this project."
---

# Skill: Create Blazor Page

Use this skill when adding a new page to `src/GymClassBooking.Web/Pages/`.

## Checklist before generating

1. Identify the route and any route parameters (`{Id:int}` etc.).
2. Identify which `GymApiClient` methods are needed.
3. Identify which MudBlazor components fit the layout (list → `MudTable`/`MudCard`, detail → `MudText`/`MudAlert`, form → `MudTextField`/`MudNumericField`).

## File location

```
src/GymClassBooking.Web/Pages/{PageName}.razor
```

## Template

```razor
@page "/route/{Param:int}"

@inject GymApiClient Api
@inject ISnackbar Snackbar

<PageTitle>Page Title</PageTitle>

@if (_loading)
{
    <MudProgressCircular Indeterminate="true" />
}
else if (_data is null)
{
    <MudAlert Severity="Severity.Error">Could not load data.</MudAlert>
}
else
{
    <MudText Typo="Typo.h4" Class="mb-4">Heading</MudText>

    @* Main content here using MudBlazor components *@
}

@code {
    [Parameter] public int Param { get; set; }

    private bool _loading = true;
    private SomeModel? _data;

    protected override async Task OnInitializedAsync()
    {
        _data = await Api.GetSomethingAsync(Param);
        _loading = false;
    }
}
```

## Rules

- **`@page` directive must be first** (before `@inject`).
- Always show `<MudProgressCircular Indeterminate="true" />` while loading.
- Always show `<MudAlert Severity="Severity.Error">` when data is null after load.
- All API calls go through `GymApiClient` — never `HttpClient` directly.
- Use `@inject GymApiClient Api` and `@inject ISnackbar Snackbar` for notifications.
- Load data in `OnInitializedAsync()`, not `OnParametersSetAsync()` (unless parameter can change within the same page instance).
- Use MudBlazor components exclusively — no raw HTML `<table>`, `<button>`, or Bootstrap classes.
- Models live in `src/GymClassBooking.Web/Models/ApiModels.cs` — reuse existing records.
- State mutations: set `_loading = true` → call API → set `_loading = false` in `finally`.

## MudBlazor component guide

| UI need            | Component                                                  |
| ------------------ | ---------------------------------------------------------- |
| List of cards      | `MudGrid` + `MudCard` + `MudCardContent`                   |
| Tabular data       | `MudTable T="ModelType"` with `MudTh`/`MudTd`              |
| Status badge       | `MudChip T="string" Color="Color.Success/Error/Warning"`   |
| Warning banner     | `MudAlert Severity="Severity.Warning"`                     |
| Action button      | `MudButton Variant="Variant.Filled" Color="Color.Primary"` |
| Text input         | `MudTextField @bind-Value="_field"`                        |
| Number input       | `MudNumericField @bind-Value="_id" Min="1"`                |
| Toast notification | `Snackbar.Add("message", Severity.Success)`                |
| Section divider    | `MudDivider Class="my-4"`                                  |

## Do NOT

- Use Bootstrap CSS classes (`btn`, `card`, `table`).
- Call EF Core or Infrastructure services.
- Add `@using` directives — global usings are in `_Imports.razor`.
