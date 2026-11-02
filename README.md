# ZooWorld

Unity developer test project. Frogs and snakes spawn every 1-2 seconds, move with distinct
behaviors and interact through the food chain. Animal pooling, eating, session lifecycle
and the additive uGUI HUD are implemented. Death totals appear at the upper right; pooled
`Tasty!` labels follow the eater.

## Open

Use **Unity 6000.3.26f1 (Unity 6.3 LTS)** and open this repository root.
The project uses the **Universal 3D / URP** template (`com.unity.template.urp-blank`).
Open `Assets/Scenes/Intro.unity` and enter Play Mode. Intro loads Main and HUD additively;
the session becomes Ready after both scene roots receive their shared models.
Main has a top-down orthographic camera, primitive animals and gray straight/L-shaped walls.
Frogs jump visually while their colliders remain grounded; snakes move continuously.

## Project status

- Zenject/Extenject is installed from the minimal Source package at pinned revision
  `8495d61cebe4175b358370a3599b32ae976d6de6` (package metadata version 9.2.0).
- Intro owns the session scope; Main and HUD have separate child scopes and explicit readiness/cleanup.
- Animal models, species validation and per-lifetime model injection are implemented.
- Frog/Snake prefab variants, SO assets, independent movement motors and per-prefab pools are implemented.
- Static wall prefabs block both species; spawning excludes wall volumes.
- Prey pairs rebound through physics. Predators eat prey and smaller predators; equal predator
  sizes resolve by lower lifetime ID. Each death updates shared statistics once and emits one meal event.
- The victim disappears immediately and returns to the pool on the next physics tick.
- HUD counters observe shared statistics; pooled eating labels track lifetime identity and clear on reuse/unload.
- Authored gameplay configuration must use typed ScriptableObject folders under `Assets/SO/`.
- Repeated runtime objects must use prefabs, with variants and nested prefabs where appropriate.
- CrazyGames integration, monetization and CI are outside this setup.
- Player builds require an explicit request.

The original assignment is preserved in `Zoo world_2026.pdf`.

## Documentation

- [Reserve code, architecture and organization review](Documentation/Engineering/Reports/2026-11-02-reserve-review.md)
- [Final requirements verification and handoff](Documentation/Engineering/Reports/2026-11-02-handoff-validation.md)
- [Game design document](Documentation/GDD/GDD.md)
- [System architecture and current implementation](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md#zooworld-system-architecture)
- [Implementation iterations](Documentation/Engineering/implementation-plan.md)
- [Agent instructions](AGENTS.md)
- [Code style and architecture](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md)
- [Documentation guidelines](Documentation/DOCUMENTATION_GUIDELINES.md)
- [GDD best practices](Documentation/GDD/GDD_BEST_PRACTICES.md)
- [Art direction guidelines](Documentation/GDD/ART_DIRECTION_GUIDELINES.md)
- [Bootstrap validation](Documentation/Engineering/Reports/2026-11-02-bootstrap-validation.md)
- [Iteration 1 validation](Documentation/Engineering/Reports/2026-11-02-iteration-one-validation.md)
- [Iteration 2 validation and capture](Documentation/Engineering/Reports/2026-11-02-iteration-two-validation.md)
- [Iteration 3 validation](Documentation/Engineering/Reports/2026-11-02-iteration-three-validation.md)
- [Iteration 4 validation and capture](Documentation/Engineering/Reports/2026-11-02-iteration-four-validation.md)
- [Arena wall validation and capture](Documentation/Engineering/Reports/2026-11-02-arena-walls-validation.md)

Unity Pipeline is included for Editor automation through Unity CLI. Template packages remain
at the versions resolved by the selected Editor. Unity Cloud is not linked.

For PlayMode Test Runner execution, temporarily clear `EditorSceneManager.playModeStartScene`
and run from an empty Editor scene so the runner owns its test scene. Restore Intro as the
start scene afterwards. Test fixtures load Intro additively and verify actual readiness and cleanup.

Previously prepared gameplay drafts are preserved locally under the ignored
`LocalDrafts/Gameplay/` folder. They are outside `Assets/`, are not imported, and are not part
of the committed project.

Current tuning is in `Assets/SO/Simulation/ZooSimulation.asset`, `Assets/SO/Movement/` and
`Assets/SO/Animals/`. Pools grow on demand with no active population cap and keep at most
16 inactive views per prefab. Each acquire creates a fresh model/ID; returning a view is not a death.

Verification: 39 EditMode and 32 PlayMode cases passed, including movement, physical food-chain
contacts, larger/equal predator outcomes, single death/event accounting, repeated pool reuse,
stale callbacks, HUD counters/feedback/lifecycle, wall collisions, wall-free spawns, spawn readiness/cadence and scene cleanup. Omnivore contacts remain undefined and blocked.

## Extend the species catalogue

1. Create an `AnimalDefinitionSO` through **Assets > Create > ZooWorld > Animal Definition**
   and save it under `Assets/SO/Animals/`. Give it a unique stable ID, display name, icon,
   supported role and positive ordered size range.
2. Reuse `FrogJump.asset` or `SnakeLinear.asset` from `Assets/SO/Movement/`. Use an existing
   animal prefab for the same visuals, or create a variant of `Assets/Prefabs/Animals/Animal.prefab`.
   Keep the prefab root inactive, with its root Rigidbody/SphereCollider and child VisualRoot.
3. Add the definition to the Animals list of `Assets/SO/Simulation/ZooSimulation.asset`.
   The scheduler samples the configured catalogue uniformly; no controller changes are needed.
4. For a new locomotion type, derive a `MovementSO` that validates its settings and returns
   an independent `IMovement` instance from `Create`. Keep mutable movement state in that instance.
   Preserve grounded physics when adding visual motion.

The handoff test creates a temporary third definition with a new ID and fixed size, uses the
production scheduler, controller, linear movement and pool, then destroys the temporary SO.
No demonstration species is shipped. A new food-chain role requires explicit interaction and
statistics rules; authoring Omnivore is supported, but spawning it is blocked until those rules exist.

## Verification and limits

Use the Unity Test Runner for both EditMode and PlayMode assemblies. Before PlayMode tests,
clear `EditorSceneManager.playModeStartScene` and open an empty scene; afterwards restore
`Assets/Scenes/Intro.unity` as the play-mode start scene. Unity CLI async test runs use
`test run --mode editor|playmode --filter ZooWorld.EditMode.Tests|ZooWorld.PlayMode.Tests
--filter_type assembly --async_tests true`, followed by `test status`. Pass this project's
`--project-path`, `--caller plugin --skill unity-cli` and a `--` fence before Editor parameters.

The final report maps each assignment requirement to its evidence and records a normal run
with 100 spawns, 79 animal-pool reuses, 90 meals and matching HUD totals. These are observations,
not a performance benchmark. The architecture supports content-defined species; 1,000 concurrent
animals, zero-GC operation and platform performance have not been measured.

Prototype limits: no player input/reset menu, persistence or pathfinding; animals use physical
recoil and random turns around walls. Crowded spawns may overlap other animals after bounded
placement attempts, but remain outside walls. Omnivore rules and device safe-area/platform
build validation are open. No player build or CI deployment is included in this handoff.


## Project organization

| Location | Responsibility |
| --- | --- |
| `Assets/Scripts/Domain/` | Engine-independent animal/session models and food-chain decisions |
| `Assets/Scripts/Animals/` | Animal view/controller/pool; movement strategies and species definitions |
| `Assets/Scripts/Simulation/` | Spawn schedule, camera bounds, obstacle map and simulation configuration |
| `Assets/Scripts/UI/` | Event-driven counters, feedback controller/pool and presentation settings |
| `Assets/Scripts/Infrastructure/` | Zenject installers, lifetime factory and Intro/Main/HUD composition |
| `Assets/Scripts/Editor/Setup/` | Explicit one-time content setup tools; do not rerun on existing content |
| `Assets/Scripts/Tests/` | Separate EditMode and PlayMode test assemblies |
| `Assets/Prefabs/Animals`, `World`, `UI` | Reusable variants, nested walls and HUD components |
| `Assets/SO/` | Configuration assets grouped by type |
| `Assets/Materials/Animals`, `World` | Animal and environment material groups |
| `Assets/Settings/` | Render pipeline, physics and input assets |
| `Documentation/` | GDD, engineering rules, plans and dated validation reports |

The template SampleScene remains preserved; gameplay starts from Intro. The reserve review
moves existing assets through AssetDatabase without changing GUIDs or deleting template/vendor content.
