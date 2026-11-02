# Animals

Status: agreed core; open decisions below. Priority: Essential.
Owner: Andrii Maksymov. Updated: 2026-11-02.
Main GDD: [ZooWorld GDD](GDD.md). Implementation: data/models and validation exist;
species assets, motion, pooling and contact resolution remain pending.

## Species and instance data

Each species definition contains a stable ID, display name, Sprite icon, food-chain role,
minimum/maximum size, movement definition and prefab reference. Supported roles include
Prey, Predator and Omnivore. Initial required content is Frog (Prey) and Snake (Predator).
Supporting an Omnivore value does not imply adding an unrequested third animal species.

Current size belongs to an individual animal's runtime state. The authored range is
read-only species configuration. Sample size uniformly at spawn and use it as a dimensionless
multiplier of the prefab's authored scale and collider. Size does not grow after eating.
Actual species ranges remain tuning decisions. Sampling and no-growth rules were approved on 2026-11-02.

Names and icons are content metadata; an animal-selection menu or portrait HUD is not required.

## Contact outcomes

| Contact | Result | Authority |
| --- | --- | --- |
| Prey / Prey | Both survive and separate through physics | Assignment |
| Predator / Prey | Predator eats prey, regardless of relative size | Assignment |
| Predator / Predator, unequal size | Larger current size survives and eats the other | Owner decision |
| Predator / Predator, equal size | Lower unique lifetime ID survives | Owner decision |
| Any contact involving Omnivore | TBD: eligible victims, size comparison and counter category | Owner decision required |

Size comparison is symmetric in callback order. A dead animal cannot eat or die again.
The victim disappears; the surviving eater receives `Tasty!` feedback. Each death increments
exactly one counter. Returning an animal to a pool during shutdown is not a death.

No player input, rewards, saved state or manual attack commands are required.

## Acceptance criteria

- New species using an existing role/movement can be configured without editing core controllers.
- Icons, names, roles and size ranges are authored in species SO assets.
- Invalid IDs, references and nonpositive/nonfinite/reversed size ranges are rejected before spawning.
- Predator pairs select the larger survivor in either callback order.
- Duplicate callbacks count one death and produce one eating event.
- A recycled view receives a new lifetime identity and cannot retain an old animal's state.
- Omnivore behavior is not silently treated as Prey or Predator before its rules are agreed.

## Dependencies and open decisions

Depends on [Simulation](Simulation.md) for contact, sizing and reuse and
[Presentation](Presentation.md) for feedback/counters. No persistence is specified.

Decisions owned by Andrii: Omnivore contact matrix and death-counter mapping, and authored
species size ranges. The Frog/Snake path can proceed independently of Omnivore content.
Uniform size initialization, no growth after eating and lower-ID equal-size ties are approved.

## References and history

- [Original assignment](../../Zoo%20world_2026.pdf): Frog, Snake and base food chain.
- Owner decisions in project chat, 2026-11-02: add species metadata, Omnivore and size range;
  replace the proposed ID-based predator winner with comparison of current sizes.
- Owner follow-up, 2026-11-02: approved uniform initial size, no growth after meals and
  lower lifetime ID as the equal-size predator tie-breaker.
