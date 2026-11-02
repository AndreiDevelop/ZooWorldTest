# Iteration 1 validation

Status: verified locally. Owner: Andrii Maksymov. Updated: 2026-11-02.
Scope: composition and animal data from the [iteration plan](../implementation-plan.md).

## Implemented

- Pinned minimal Zenject/Extenject Source package at revision
  `8495d61cebe4175b358370a3599b32ae976d6de6`; package metadata reports 9.2.0.
- Separate Domain, Runtime, Editor, EditMode-test and PlayMode-test assemblies.
- Intro/Main/HUD scenes, explicit SceneFlowSO, shared session bindings and child scene scopes.
- Readiness gating, startup cancellation, shutdown and unexpected required-scene unload handling.
- AnimalDefinitionSO with ID, display name, Sprite icon, role, size range, movement and prefab references.
- Per-lifetime AnimalModel/identity, fixed sampled size, death state and shared death statistics.
- AnimalLifetimeFactory injects the same model into Controller and inactive View; replacement
  detaches the View from its previous model. Actual pooling remains iteration 2 work.
- Omnivore definitions can be authored but are explicitly rejected at the simulation boundary.

## Automated checks

Executed through the connected Unity 6000.3.26f1 Editor using Unity CLI:

| Suite | Result | Coverage |
| --- | --- | --- |
| ZooWorld.EditMode.Tests | 20 passed, 0 failed | Size/reference validation, configuration immutability, fresh identities, DI ownership, readiness, one-shot death statistics and Omnivore rejection |
| ZooWorld.PlayMode.Tests | 3 passed, 0 failed | Shared session across scenes, startup cancellation, complete shutdown and unexpected HUD unload |

PlayMode Test Runner requires clearing the Editor start-scene override and starting from an
empty Editor scene. Intro was restored as the normal start scene after the run.
Raw results are retained locally under ignored `LocalDrafts/Reports/iteration-one-editmode.json`
and `iteration-one-playmode.json`; these local artifacts are not repository deliverables.

## Normal Editor startup

- Entered Play Mode from Intro with the restored startup configuration.
- Verified exactly three loaded scenes, Main/HUD readiness and SessionState.Ready.
- Confirmed real frame progression: frameCount advanced from 7264 to 9944 across observations.
- Inspected a camera capture: the expected template sky/ground background is visible;
  no animal or HUD presentation is claimed at this stage.
- Gracefully stopped the session, exited Play Mode and left Intro open in Edit Mode.
- Compiler and Console error counts were zero. Unity emitted an FMOD audio-device
  reinitialization warning during Play Mode transitions; audio is not part of this iteration.

The first startup check exposed an unsaved Intro configuration reference. It was corrected
through Editor APIs; setup now saves configuration before scene transitions, reloads the reference
and rejects missing reference assignments. The successful tests/startup above ran after the fix.

## Limits and next iteration

No animal content assets/prefabs, spawn loop, concrete movement, pools, food-chain resolver,
visible counters or eating feedback are implemented. The current AnimalController is a
readiness/model coordination foundation, not a motion implementation. Equal-size predator
tie policy is documented; its resolver implementation belongs to iteration 3.

Uniform initial size, no growth after eating and lower lifetime ID on equal predator sizes
were explicitly approved by the owner. Omnivore contacts/counter mapping and authored species
size ranges remain unresolved. No player build was created. No commit/push was made for this iteration.
