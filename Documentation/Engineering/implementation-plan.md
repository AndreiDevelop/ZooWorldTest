# ZooWorld implementation plan

Status: iterations 0-2 complete; iteration 3 pending. Owner: Andrii Maksymov. Updated: 2026-11-02.
Scope: [GDD](../GDD/GDD.md) and the agreed
[system architecture](CODE_STYLE_AND_ARCHITECTURE.md#zooworld-system-architecture).

## Working budget and sequencing

The owner requested a couple of hours for implementation and delivery. The allocations below
are a proposed 120-minute work budget from gameplay start, not a guaranteed estimate or a
new deadline. Confirm remaining wall-clock time before starting. Package/import problems and
failed acceptance checks consume the reserve and require replanning, rather than unverified completion.

| Iteration | Budget | Status | Reviewable result |
| --- | --- | --- | --- |
| 0. Empty project | Already completed | Verified | Unity 6.3 URP baseline published at `4b213e8` |
| 1. Composition and animal data | 20 min | Verified locally | Scene readiness, injected state and validated species definitions |
| 2. Pool, spawn and movement | 30 min | Verified locally | Reused frogs/snakes moving and returning to view |
| 3. Food-chain resolution | 20 min | Pending | Size-based predator outcome, one death/event per lifetime |
| 4. HUD and feedback | 15 min | Pending | Additive uGUI counters and pooled `Tasty!` |
| 5. Verification and handoff | 20 min | Pending | Checked requirements, extension example and accurate documentation |
| Reserve | 15 min | Unallocated | Integration fixes and delivery |

Iterations are sequential acceptance gates. Do not report an iteration complete because its
time allocation expired. Iterations 1-2 were requested by the owner; later iterations require their own start request.

## Iteration 0 - verified baseline

- Existing remote history is preserved; `main` contains `4b213e8`.
- Unity 6000.3.26f1 URP imports with no current compiler/Console errors or warnings.
- The scene contains Main Camera, Directional Light and Global Volume.
- Gameplay, Zenject and the Intro/Main/HUD split remain pending.
- See [bootstrap validation](Reports/2026-11-02-bootstrap-validation.md).

## Iteration 1 - composition and animal data

- Approved: uniform size initialization, no growth after eating, lower lifetime ID on equal-size predator ties.
- Keep Omnivore contact/counter decisions visible; reject simulation of that role until defined.
- Install/pin Zenject through Package Manager API and introduce explicit assembly boundaries.
- Create Intro/Main/HUD scenes and readiness/cleanup flow. Begin simulation only after Main and HUD are ready.
- Implement species configuration with ID, name, icon, role, size range, movement and prefab.
- Introduce per-lifetime AnimalModel and session statistics; inject shared models into Views/Controllers.

Exit gate: compilation succeeds; scene lifetimes are explicit; definitions validate; invalid
size/reference data produces actionable errors. No omnivore actor is spawned with unspecified rules.

Result: 20 EditMode and 3 PlayMode tests passed. Intro/Main/HUD, shared child-scope injection,
startup cancellation and required-scene unload were checked. Concrete species/movement assets
and prefabs remain iteration 2 work; the definition contracts were exercised with isolated test content.
See [iteration 1 validation](Reports/2026-11-02-iteration-one-validation.md).

## Iteration 2 - pool, spawn and movement

- Implement per-prefab animal pools, fresh lifetime identity, acquire/release reset and stale-callback protection.
- Create a reusable animal base prefab and Frog/Snake variants; author SO assets in typed folders.
- Spawn one animal every 1-2 seconds; compose independent movement instances from movement SOs.
- Keep collision bodies grounded, animate frog visuals, preserve physics recoil and implement camera return.

Exit gate: Play Mode frames advance; both species move correctly, leave/return at varied
aspect ratios and reuse views without stale velocity, scale, timers, subscriptions or visual offsets.
Use focused movement and pool-lifetime tests; do not merely compare configuration getters.

Result: 20 EditMode and 16 PlayMode cases passed, including real physics rebound, movement
distance/speed, 12 reuse cycles, stale release requests, spawn cadence and camera aspect changes.
Both species were observed moving in the normal Intro/Main/HUD startup, with no current
Console errors or warnings during the capture. See [iteration 2 validation](Reports/2026-11-02-iteration-two-validation.md).

## Iteration 3 - food chain

- Implement the agreed contact matrix and compare predator current sizes.
- Commit death before publishing events; disable victim contacts immediately and defer recycling
  until the current physics-contact dispatch is complete.
- Reject dead, self, reciprocal, repeated and stale-lifetime contacts.
- Increment session statistics and issue one eating event per committed death.

Exit gate: EditMode tests cover prey/prey, predator/prey in either order, larger-predator
survival, agreed equal-size outcome, duplicate callbacks and multi-animal contact chains.
Play Mode confirms separation and one release/death per victim. Omnivore behavior requires an agreed matrix.

## Iteration 4 - HUD and feedback

- Add uGUI/TMP dead-prey/dead-predator counters to the upper right of HUD.
- Pool `Tasty!` labels; inject the camera and statistics instead of serializing cross-scene references.
- Follow the eater's current lifetime; clean up on death, recycle, HUD disable and scene unload.

Exit gate: counters start correctly and match actual deaths; each meal produces readable
feedback beneath the eater; reused views cannot inherit a previous lifetime's label.

## Iteration 5 - verification and handoff

- Run meaningful EditMode/PlayMode tests, inspect Console and capture the running simulation.
- Confirm actual frame progression, cadence, both movements, return behavior, contacts and counters.
- Add a temporary species configuration using an existing movement to demonstrate extension
  without a new species-specific controller; remove demonstration content if it is outside delivery scope.
- Inspect pool behavior after repeated reuse and scene unload. Record measured results and remaining limits.
- Update README with startup, extension steps, actual scenes/packages and known limitations.
- Update implementation status in the GDD/architecture/plan together; retain agreed open decisions.
- Review intended Git changes. Commit/push only within the authorization active at delivery;
  never force-push. Verify remote head when a push is authorized.

Exit gate: every Essential requirement is checked or explicitly identified as incomplete.
Do not claim 1,000 simultaneous animals or platform performance without a corresponding measurement.
No local/CI player build is part of verification without an explicit build request.

## Draft reuse and scope protection

Ignored local drafts are optional references, not integrated code. Before reuse, review them
against the current specifications: they lack icon/Omnivore/size support, select predator ties
by ID alone and have no animal pool. Their test results are not Unity-verified results.
Iteration 2 adapted and verified the movement implementations/tests individually; it did not
import the obsolete draft domain, predation or UI code wholesale.

Keep production artwork, menus, economy, saves, backend, ECS, CrazyGames and CI outside these
iterations. Pooling and the agreed architecture remain required even if the budget becomes tight.

## Change history

- 2026-11-02: recorded the bootstrap and decomposed the agreed simulation into testable iterations;
  distinguished approved behavior, proposals and pending implementation.
