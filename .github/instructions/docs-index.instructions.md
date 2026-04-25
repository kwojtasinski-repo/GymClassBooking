---
applyTo: "**"
---

# Docs & Context Index

> AI routing table — tells the agent WHAT to read FIRST for each type of task.
> When starting a task, match it to a row below and read the listed files before generating code.

---

## Task → Context Map

| Task type                            | Read FIRST                                           | Then read                           |
| ------------------------------------ | ---------------------------------------------------- | ----------------------------------- |
| **Any non-trivial task**             | `.github/context/anti-patterns-critical.context.md`  | —                                   |
| **Add a controller**                 | `.github/skills/create-api-controller/SKILL.md`      | `anti-patterns-critical.context.md` |
| **Add an Application service**       | `.github/skills/create-application-service/SKILL.md` | —                                   |
| **Add a domain entity / enum**       | `.github/skills/create-domain-entity/SKILL.md`       | —                                   |
| **Add a DTO**                        | `.github/skills/create-dto/SKILL.md`                 | —                                   |
| **Add a Blazor page**                | `.github/skills/create-blazor-page/SKILL.md`         | —                                   |
| **Add a unit test**                  | `.github/skills/create-unit-test/SKILL.md`           | —                                   |
| **Add an integration test**          | `.github/skills/create-integration-test/SKILL.md`    | —                                   |
| **Change waitlist promotion rule**   | `.github/skills/create-waitlist-promotion/SKILL.md`  | —                                   |
| **Change cancellation penalty rule** | `.github/skills/create-cancellation-policy/SKILL.md` | —                                   |
| **Add a booking guard / rule**       | `.github/skills/create-booking-rule/SKILL.md`        | —                                   |
| **Understand project structure**     | `.github/context/repo-index.md`                      | —                                   |
| **What not to do / anti-patterns**   | `.github/context/anti-patterns-critical.context.md`  | —                                   |

---

## Context Files

| File                                                | Purpose                                               | When to update                                       |
| --------------------------------------------------- | ----------------------------------------------------- | ---------------------------------------------------- |
| `.github/copilot-instructions.md`                   | Global coding conventions, stack rules, skills table  | When a new convention is established                 |
| `.github/context/anti-patterns-critical.context.md` | Blocking architectural violations                     | When a pattern causes a second-occurrence correction |
| `.github/context/repo-index.md`                     | Machine-readable codebase map                         | When projects/files are added or removed             |
| `.github/instructions/docs-index.instructions.md`   | This file — AI routing table                          | When new docs or context files are added             |
| `.github/instructions/api.instructions.md`          | Auto-loads for controller/endpoint/route tasks        | When API conventions change                          |
| `.github/instructions/blazor.instructions.md`       | Auto-loads for Blazor/Razor/UI tasks                  | When Blazor conventions change                       |
| `.github/instructions/application.instructions.md`  | Auto-loads for service/interface/business logic tasks | When Application layer conventions change            |
| `.github/instructions/domain.instructions.md`       | Auto-loads for entity/enum/domain tasks               | When Domain layer conventions change                 |
| `.github/instructions/testing.instructions.md`      | Auto-loads for any test-related tasks                 | When testing conventions change                      |

---

## Skills

| Skill                      | File                                                 | Invoked when                            |
| -------------------------- | ---------------------------------------------------- | --------------------------------------- |
| create-api-controller      | `.github/skills/create-api-controller/SKILL.md`      | Adding a new controller                 |
| create-application-service | `.github/skills/create-application-service/SKILL.md` | Adding a service + interface            |
| create-domain-entity       | `.github/skills/create-domain-entity/SKILL.md`       | Adding an entity or enum                |
| create-dto                 | `.github/skills/create-dto/SKILL.md`                 | Adding request/response/summary records |
| create-blazor-page         | `.github/skills/create-blazor-page/SKILL.md`         | Adding a Blazor WASM page               |
| create-unit-test           | `.github/skills/create-unit-test/SKILL.md`           | Adding xUnit unit tests                 |
| create-integration-test    | `.github/skills/create-integration-test/SKILL.md`    | Adding WebApplicationFactory tests      |
| create-waitlist-promotion  | `.github/skills/create-waitlist-promotion/SKILL.md`  | Changing waitlist promotion priority    |
| create-cancellation-policy | `.github/skills/create-cancellation-policy/SKILL.md` | Changing late cancellation penalty rule |
| create-booking-rule        | `.github/skills/create-booking-rule/SKILL.md`        | Adding a guard to the booking flow      |
