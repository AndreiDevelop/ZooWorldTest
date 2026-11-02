# Reserve code, architecture and organization review

Date: 2026-11-02. Scope: project-owned C#, assembly boundaries, composition/lifetimes and
asset organization against [Code Style and Architecture](../CODE_STYLE_AND_ARCHITECTURE.md).
Unity 6000.3.26f1; Unity CLI 1.0.0-beta.15. Vendor/package code is outside the style cleanup.

## Findings and corrections

| Finding | Correction |
| --- | --- |
| HudController depended on HudSceneRoot to obtain presentation references | Constructor-inject settings, label prefab and feedback RectTransform directly; HudSceneInstaller binds them. Presentation no longer depends on the composition root |
| Spawn-clearance scan could dereference a view destroyed before the next pool tick | Skip destroyed views, dead models and retired lifetimes; add a regression exercising destruction, spawning and subsequent pool cleanup |
| Repeated unnecessary fully qualified Unity types and unused imports | Use normal imports or Object/Random aliases, sort imports and remove two semantically unused directives |
| Environment materials mixed at the Materials root | Move Ground and Wall to Materials/World and update setup-tool paths/folder creation |
| Input configuration at Assets root | Move InputSystem_Actions.inputactions into Settings, preserving its GUID |
| One-time setup scripts mixed beside their Editor assembly definition | Group the five setup tools under Scripts/Editor/Setup; keep the Editor assembly boundary at its existing root |

All eight moves used AssetDatabase.MoveAsset. Each destination GUID was compared with the
source GUID immediately after the move. Source namespaces, serialized class identities and
scene/prefab GUID references remain intact. No gameplay tuning, assets or vendor/template
content were deleted. In particular, SampleScene and TMP vendor resources remain preserved.

## Code-style checks

- Parsed all 55 project C# files using Roslyn from the locally installed .NET SDK.
- Checked parse errors, nested types, type/file names, braces on control-flow statements,
  expression-bodied methods/constructors and multiple field declarators: no remaining findings.
- Ran the Roslyn formatter over project-owned C#: final pass requires no formatting changes.
- Constructed semantic compilations for all five project assemblies using the running Editor's
  actual source files, defines and references: zero compilation errors and no unused imports.
- Removed unused UnityEngine from SceneScope and ZooWorld.Domain.Session from FoodChainPlayModeTests.
- Reviewed the functional diff for readable block bodies and explicit dependency/lifetime ownership.
  These checks cover the listed rules; they are not a permanent analyzer enforcing every prose convention.

## Architecture and organization checks

- Domain remains a separate assembly with no engine references and no dependency on views or DI.
- Zenject container access remains confined to infrastructure installers/factory/scopes.
- Intro/Main/HUD responsibilities and shared authoritative models remain separate.
- Opened all three scenes through Editor APIs: zero missing MonoBehaviour scripts and zero
  serialized references to objects in another scene. Main/HUD configuration validation passed.
- Frog and Snake are still prefab variants; their AnimalView reference validation passed.
- Project-owned assets use explicit references, typed SO categories and logical prefab/material groups.
  Single-file SO categories and necessary assembly roots are intentional grouping exceptions.
- No global runtime object searches, project Resources.Load calls, ECS or static mutable gameplay
  state were introduced. TMP Resources remain required vendor support, not a project loading path.

## Verification and remaining scope

After the fixes and moves, **39 EditMode + 32 PlayMode = 71 tests passed**, with zero
failures/skips. Final Console has zero errors/warnings and compilation is complete. Editor is
restored to Intro outside Play Mode, with no unsaved scene changes. Asset metadata,
documentation links and `git diff --check` pass. Raw syntax/semantic reports, move records, scene checks and test envelopes are retained
locally under ignored `LocalDrafts/Reports/reserve-*` files.

Platform builds, 1,000 simultaneous animal performance and the undefined Omnivore interaction
matrix remain outside this pass. No commit/push or player build is part of this review.
