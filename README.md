# ZooWorld

Unity developer test project. Frogs and snakes spawn every 1-2 seconds, move with distinct
behaviors and collide physically. Animal pooling and session lifecycle are implemented.
Food-chain eating and the visible HUD are pending iterations 3-4.

## Open

Use **Unity 6000.3.26f1 (Unity 6.3 LTS)** and open this repository root.
The project uses the **Universal 3D / URP** template (`com.unity.template.urp-blank`).
Open `Assets/Scenes/Intro.unity` and enter Play Mode. Intro loads Main and HUD additively;
the session becomes Ready after both scene roots receive their shared models.
Main has a top-down orthographic camera and primitive animals on a ground plane.
Frogs jump visually while their colliders remain grounded; snakes move continuously.

## Project status

- Zenject/Extenject is installed from the minimal Source package at pinned revision
  `8495d61cebe4175b358370a3599b32ae976d6de6` (package metadata version 9.2.0).
- Intro owns the session scope; Main and HUD have separate child scopes and explicit readiness/cleanup.
- Animal models, species validation and per-lifetime model injection are implemented.
- Frog/Snake prefab variants, SO assets, independent movement motors and per-prefab pools are implemented.
- Predation and visual HUD remain pending. At this stage all contacts are resolved by physics.
- Authored gameplay configuration must use typed ScriptableObject folders under `Assets/SO/`.
- Repeated runtime objects must use prefabs, with variants and nested prefabs where appropriate.
- CrazyGames integration, monetization and CI are outside this setup.
- Player builds require an explicit request.

The original assignment is preserved in `Zoo world_2026.pdf`.

## Documentation

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

Verification: 20 EditMode and 16 PlayMode cases passed, including movement distance/speed,
physical rebound, repeated pool reuse, stale callback rejection, spawn readiness/cadence and scene cleanup.
