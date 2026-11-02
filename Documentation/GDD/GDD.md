# ZooWorld GDD

Status: agreed core requirements; open decisions are explicitly marked.
Owner: Andrii Maksymov. Updated: 2026-11-02.
Implementation: scene composition, animal data, movement, spawning and pooling are implemented;
food-chain resolution and HUD presentation remain pending.

## Concept and purpose

ZooWorld is an autonomous 3D animal simulation viewed from above. Animals appear,
move randomly, collide and interact through a simple food chain. The viewer observes
the simulation; no gameplay input, victory condition or progression is specified.

The test demonstrates clear animal architecture that can support many different species.
The reference to 1,000 different animals describes extensibility of the species catalogue;
it is not a verified requirement for 1,000 simultaneous physics bodies.

## Design pillars

- Readable behavior: frogs jump, snakes move continuously, collisions have visible outcomes.
- Species compose reusable movement and food-chain roles.
- Physics determines contact and separation; food-chain rules determine deaths.
- Primitive graphics are sufficient. Additional artwork does not improve the assignment's score.

## Core loop

1. Spawn one animal every 1-2 seconds.
2. Move in a randomly selected direction using its species movement.
3. Redirect animals that leave the camera area back into view.
4. Resolve physical contacts: separation or eating.
5. Show eating feedback, increment the appropriate death counter and recycle the victim.
6. Continue until the session ends.

## Feature index

| Feature | Priority | Design status | Specification |
| --- | --- | --- | --- |
| Species, size and food chain | Essential | Agreed core; omnivore rules TBD | [Animals](Animals.md) |
| Spawning, movement and reuse | Essential | Agreed core; tuning TBD | [Simulation](Simulation.md) |
| Camera, graphics and uGUI | Essential | Assignment-defined; tuning TBD | [Presentation](Presentation.md) |

These are design statuses. Both species spawn, move and collide in the simulation.
Eating and visible HUD feedback are not implemented yet. See the iteration plan for validation status.

## Technical and delivery constraints

Use the existing Unity 6000.3.26f1 URP project, MCV with Zenject, separate Intro/Main/HUD
scenes, ScriptableObject balance and pooled animal prefabs. Do not use ECS.
The target repository is [ZooWorldTest](https://github.com/AndreiDevelop/ZooWorldTest).
CrazyGames, monetization, CI, persistence and platform builds are outside current scope.
A build requires an explicit request.

See [system architecture](../Engineering/CODE_STYLE_AND_ARCHITECTURE.md#zooworld-system-architecture)
and the [iteration plan](../Engineering/implementation-plan.md).

## References and decision history

- [Original assignment](../../Zoo%20world_2026.pdf): core simulation, species, uGUI,
  architecture emphasis and Git submission. Adopt these requirements directly.
- Project chat, 2026-11-02: owner requested icon/name/omnivore/size-range configuration,
  larger-predator survival, a grounded collider with visual jumping, and animal pooling.
  These extend the assignment and take precedence over earlier proposals.

Open decisions and measurable acceptance criteria are recorded in the feature specifications.
