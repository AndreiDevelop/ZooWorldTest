# Simulation

Status: agreed core; numeric tuning TBD. Priority: Essential.
Owner: Andrii Maksymov. Updated: 2026-11-02.
Main GDD: [ZooWorld GDD](GDD.md). Implementation: movement/pooling verified in iteration 2;
automatic food-chain contacts and death statistics verified in iteration 3.

## Spawning and direction

During a ready session, one animal appears every 1-2 seconds and starts moving randomly.
Initial population, first-spawn timing, species distribution, spawn clearance and turn
cadence are not specified by the assignment. Current implementation begins with an empty world,
spawns the first animal after the first interval and samples both species uniformly. All tuning is in SO assets.
Do not silently add an active-population limit or spawn a large initial batch.

The scheduler emits at most one animal per Update and schedules the next 1-2 second interval
after that spawn; a long frame does not trigger a catch-up burst. Spawn placement tries eight
positions with size-aware clearance, then allows physics to resolve overlap rather than dropping a spawn.

The camera's visible ground area determines return behavior. Leaving it changes the
animal's movement direction back toward the screen, rather than teleporting it or killing it.
Account for instance size and viewport/aspect changes. Keep enough ground outside the
camera area for an animal to return. Spawn inside a valid visible location.

## Movement

| Species | Required behavior | Configuration |
| --- | --- | --- |
| Frog | Periodic jumps covering a fixed ground distance | Interval (s), distance (world units), duration (s), visual height (world units) |
| Snake | Continuous linear movement at a fixed speed | Speed (world units/s) |

The animal Rigidbody/collider stays on the ground plane. A child visual follows a jump arc;
the arc does not remove the frog from contact with predators. Physics uses FixedUpdate timing.
Collision-free movement meets the configured distance/speed; collisions may change actual
displacement. Movement preserves visible collision recoil rather than immediately
overwriting every impulse. A frog finishes its current jump before adopting a changed direction.
Current tuning is jump interval 1.2 s, duration 0.45 s, distance 1.8 units, height 0.7 units;
snake speed 2 units/s, turn interval 1.5-3.5 s and recoil recovery 4 units/s squared.

## Animal reuse

Animal GameObjects are pooled by prefab. A new spawn acquires an inactive view, initializes
a new lifetime model/size/movement state and only then enables simulation and contacts.
Death disables the victim's participation immediately and returns its view once.

Before reuse, reset linear/angular velocity, visual jump offset, timers, subscriptions,
references and feedback ownership. Previous-lifetime collision or animation callbacks cannot
affect the next occupant. Pools grow lazily, retain at most 16 inactive views per prefab and
destroy inactive overflow. There is no prewarming or active-animal cap. Runtime return is deferred
until the next physics tick; contact participation stops immediately. Reuse resets the Rigidbody
after activation while contacts remain disabled, because inactive Unity bodies can retain velocity.

## Acceptance criteria

- Exactly one spawn occurs per configured 1-2 second interval while the session is running.
- Frogs visibly jump while their physical contact body remains grounded.
- Both movements remain correct across fixed-step settings and share no mutable SO state.
- Prey contacts visibly separate; animals outside the viewport return without teleportation.
- Repeated acquire/death/release cycles retain no previous-lifetime state or subscriptions.
- Scene unload stops spawning, releases active objects and leaves no pending callbacks.

## References and dependencies

- [Original assignment](../../Zoo%20world_2026.pdf): cadence, random movement, return behavior and physics.
- Owner decision, project chat, 2026-11-02: visual-only jump arc and animal pooling.
- Depends on [Animals](Animals.md) and [Presentation](Presentation.md). No persistence is required.
