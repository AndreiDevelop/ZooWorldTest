# Bootstrap validation

Status: verified locally. Updated: 2026-11-02.

## Environment and scope

- Project: `/Users/andrii/Projects/TestTasks/ZooWorld`.
- Repository: https://github.com/AndreiDevelop/ZooWorldTest, existing `main` history preserved.
- Editor: Unity 6000.3.26f1, arm64 (Unity 6.3 LTS).
- Template: `com.unity.template.urp-blank` (Universal 3D / URP).
- Scope: empty project, shared engineering/documentation baseline, original assignment.
- CrazyGames, gameplay, monetization and CI are excluded.

## Checks performed

- Opened and imported the final project path through Unity CLI; Editor reported ready.
- Compilation completed with no compiler or Console errors.
- Inspected `Assets/Scenes/SampleScene.unity` through Editor APIs: Main Camera,
  Directional Light and Global Volume; one camera and an assigned URP asset.
- Verified product name ZooWorld, company Andrii Maksymov, run in background,
  Force Text serialization and Visible Meta Files.
- Checked all Assets entries have their `.meta` files and maintained documentation links resolve.
- Removed template readme/tutorial assets through AssetDatabase.
- Removed the previously installed Extenject package through Package Manager API.
  Two historical warnings came from its test prefabs; that package is absent from the baseline.
- Preserved earlier gameplay source drafts in ignored `LocalDrafts/Gameplay/`, outside Assets.
- No project-owned scripts, gameplay assets or Resources folders are included.

## Pending implementation

- Mandatory MCV/Zenject composition and shared/session/scene lifetimes.
- Mandatory separate Intro, Main and additively loaded HUD scenes, readiness and cleanup.
- Gameplay SO configuration in typed folders, reusable prefabs and animal behavior.

The shared requirements remain documented in
[Code Style and Architecture](../CODE_STYLE_AND_ARCHITECTURE.md).
Play Mode gameplay checks and platform checks are pending. No player build was requested or created.
