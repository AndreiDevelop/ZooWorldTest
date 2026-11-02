# Presentation

Status: assignment requirements agreed; initial presentation tuning implemented. Priority: Essential.
Owner: Andrii Maksymov. Updated: 2026-11-02.
Main GDD: [ZooWorld GDD](GDD.md). Implementation: primitive animals, arena walls and HUD implemented.

## Camera and visual language

Use a top-down 3D view. An orthographic camera is the proposed implementation; the
assignment specifies view direction but not projection type. Frogs and snakes must be
distinguishable using simple geometry, shape or color. Production models, textures,
animation rigs, audio and visual effects are not required.

Owner decision, 2026-11-02: add the raised gray walls from the supplied
[arena reference](References/arena-walls.png). Main contains two opposing L-shaped walls
and disconnected straight segments around the arena, using reusable prefabs and static
solid colliders. Preserve open passages; this is not a closed perimeter. The screenshot's
other HUD elements are outside the requested scope. Wall geometry is authored in the
level/prefabs; the existing top-down camera and animal visuals remain in use.

No generated concept art is used. The supplied screenshot is a visual reference only.

## uGUI HUD and eating feedback

- Place separate dead-prey and dead-predator totals in the top-right corner.
- Initialize both totals to zero at the start of a new session and update them once per death.
- Display `Tasty!` beneath a predator each time it eats another animal.
- Render runtime UI in the separate, additively loaded HUD scene using uGUI.
- TMP text on two uGUI overlay Canvases separates event-driven counters from moving pooled feedback.

Current initial tuning lives in `Assets/SO/UI/HudPresentation.asset`: labels last 1 second,
start 18 reference pixels beneath the animal's projected collision edge, rise 10 pixels and
fade out. Multiple meals by one eater use 25-pixel vertical slots instead of replacing a label.
Counter text is 22 reference pixels; feedback is 24. Both canvases scale from 1280x720.
The pool grows lazily, retains up to 16 inactive labels and destroys inactive overflow.
These are implementation defaults available for tuning, not values specified by the assignment. When a tracked view dies or is recycled, stop feedback for its old
lifetime. HUD disable clears labels and detaches listeners; re-enable renders current counters without replaying old meals. HUD unload releases subscriptions and labels. Displayed totals reflect the shared
authoritative session model; they are not independently counted by UI.

No interactive controls, pause menu or animal catalogue are currently required. Exactly one
EventSystem is owned by HUD if interactive UI is introduced. Respect safe visible margins
and readable contrast across tested aspect ratios; no specific mobile device matrix is approved.

## Acceptance criteria

- Both death categories remain readable at the top right and match resolved deaths.
- A predator eating either a prey or another predator produces one `Tasty!` event.
- Labels follow the correct lifetime and disappear without leaking or attaching to a reused animal.
- The frog's visual jump does not alter its physical collision layer/height.
- HUD loads additively and uses no serialized cross-scene camera or model references.

## References, dependencies and decisions

- [Original assignment](../../Zoo%20world_2026.pdf): top-down view, primitive graphics,
  `Tasty!`, upper-right death counters and mandatory uGUI.
- [Art direction guidelines](ART_DIRECTION_GUIDELINES.md): portable consistency rules;
  no inherited palette or animal art is implied.
- Depends on [Animals](Animals.md) and [Simulation](Simulation.md).
- [Iteration 4 validation and runtime capture](../Engineering/Reports/2026-11-02-iteration-four-validation.md).
- Omnivore counter presentation awaits the contact/counter decision in Animals.
