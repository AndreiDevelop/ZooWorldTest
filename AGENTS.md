# ZooWorld project instructions

Read and follow [Code Style and Architecture](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md) before changing project-owned code.
Read and follow [Documentation Guidelines](Documentation/DOCUMENTATION_GUIDELINES.md) when creating or updating documentation.
Read and follow [GDD Best Practices](Documentation/GDD/GDD_BEST_PRACTICES.md) when creating or updating game design specifications.
For gameplay changes, read the relevant feature linked from the [GDD](Documentation/GDD/GDD.md)
and the [ZooWorld system architecture](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md#zooworld-system-architecture).
Keep agreed requirements, unresolved decisions and actual implementation status distinct.
For generated GDD concepts, follow [concept-art storage and freshness](Documentation/GDD/GDD_BEST_PRACTICES.md#concept-art-storage-and-freshness): save images locally under the GDD tree, link them from the relevant design specification, and keep images, galleries and links current together.

## Shared architecture baseline

Follow the mandatory [shared architecture requirements](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md#shared-architecture-requirements): MCV with model injection into Views and Controllers through Zenject, separate Intro/Main/HUD scene responsibilities, SO configuration, type-first logical asset ownership and typed SO folders, and recursive subsystem/role grouping only for groups of at least two files. Preserve these rules across project integrations; record legacy implementation gaps separately. Only an explicit user instruction changes a shared requirement.

Read [Art Direction Guidelines](Documentation/GDD/ART_DIRECTION_GUIDELINES.md) for visual work and icon-family outline consistency. Preserve the project's approved style and import/compression settings.

## Existing project guidance

- Preserve existing project-specific instructions and approved visual references when applying this template. Add links to the project's actual art guide when available; do not import another game's art direction.

## Builds

Create a player/platform build only when the user explicitly requests a build; ordinary implementation or verification does not authorize local or CI-triggered builds.

## Unity workflow preferences

- Prefer Unity CLI for every Unity task it can perform, including inspecting the running Editor, importing and configuring assets, editing scenes and Animator controllers, executing editor C#, and validating changes.
- Follow the unity:unity-cli skill when available. Target the intended project explicitly with --project-path and include the caller/skill flags required by the skill.
- Use Unity MCP when a required capability is unavailable through CLI or CLI remains unusable after a focused diagnosis. Use UI automation for necessary visual checks or operations unavailable through CLI/MCP.
- A timeout does not prove that an operation was cancelled. Inspect the resulting state before retrying a mutation to avoid duplicate assets or changes.
- Minimize token consumption: use reusable scripts, batch related work where appropriate, and return short summaries and relevant diagnostics. Save full reports locally instead of printing large scene hierarchies, asset lists, or logs.
- Ordinary file reading, source editing, and version control may use standard filesystem and shell tools when these are more direct. Prefer Unity APIs through CLI for changes to Unity-serialized assets.
- Verify the actual result before reporting completion. Explain briefly when falling back from CLI.
