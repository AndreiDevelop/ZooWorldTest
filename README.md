# ZooWorld

Unity developer test project. Current scope: an empty project and engineering documentation.

## Open

Use **Unity 6000.3.26f1 (Unity 6.3 LTS)** and open this repository root.
The project uses the **Universal 3D / URP** template (`com.unity.template.urp-blank`).
Open `Assets/Scenes/SampleScene.unity` for the standard camera/light scene.

## Project status

- Gameplay implementation has not started in this bootstrap commit.
- MCV and Zenject are required implementation targets; Zenject is not installed in this baseline.
- Separate Intro, Main and additively loaded HUD scenes are required and pending implementation.
- Authored gameplay configuration must use typed ScriptableObject folders under `Assets/SO/`.
- Repeated runtime objects must use prefabs, with variants and nested prefabs where appropriate.
- CrazyGames integration, monetization and CI are outside this setup.
- Player builds require an explicit request.

The original assignment is preserved in `Zoo world_2026.pdf`.

## Documentation

- [Agent instructions](AGENTS.md)
- [Code style and architecture](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md)
- [Documentation guidelines](Documentation/DOCUMENTATION_GUIDELINES.md)
- [GDD best practices](Documentation/GDD/GDD_BEST_PRACTICES.md)
- [Art direction guidelines](Documentation/GDD/ART_DIRECTION_GUIDELINES.md)
- [Bootstrap validation](Documentation/Engineering/Reports/2026-11-02-bootstrap-validation.md)

Unity Pipeline is included for Editor automation through Unity CLI. Template packages remain
at the versions resolved by the selected Editor. Unity Cloud is not linked.

Previously prepared gameplay drafts are preserved locally under the ignored
`LocalDrafts/Gameplay/` folder. They are outside `Assets/`, are not imported, and are not part
of the committed project.
