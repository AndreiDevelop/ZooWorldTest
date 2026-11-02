# Presentation

Status: assignment requirements agreed; presentation tuning TBD. Priority: Essential.
Owner: Andrii Maksymov. Updated: 2026-11-02.
Main GDD: [ZooWorld GDD](GDD.md). Implementation: pending.

## Camera and visual language

Use a top-down 3D view. An orthographic camera is the proposed implementation; the
assignment specifies view direction but not projection type. Frogs and snakes must be
distinguishable using simple geometry, shape or color. Production models, textures,
animation rigs, audio and visual effects are not required.

The assignment's screenshot illustrates camera orientation. Its obstacles and unrelated
HUD elements are not additional ZooWorld requirements. No concept art has been approved
or generated for this project.

## uGUI HUD and eating feedback

- Place separate dead-prey and dead-predator totals in the top-right corner.
- Initialize both totals to zero at the start of a new session and update them once per death.
- Display `Tasty!` beneath a predator each time it eats another animal.
- Render runtime UI in the separate, additively loaded HUD scene using uGUI.
- Proposed implementation: TMP text on uGUI Canvases; pooled feedback labels track the eater.

Feedback duration, offsets, typography and overlap behavior are SO-authored presentation
tuning, currently TBD. When a tracked view dies or is recycled, stop feedback for its old
lifetime. HUD unload releases subscriptions and labels. Displayed totals reflect the shared
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
- Omnivore counter presentation awaits the contact/counter decision in Animals.
