# GDD Best Practices

These are the ZooWorld rules for writing and maintaining game design documents. All GDD text must be in English, including headings, mechanic descriptions, tables, captions and diagram labels. Follow the shared [Documentation Guidelines](../DOCUMENTATION_GUIDELINES.md).

## Purpose

A GDD specifies the gameplay the team must build and the behavior QA must verify. Explain what the player can do, why a mechanic exists and how its rules work. Keep implementation architecture in [Code Style and Architecture](../Engineering/CODE_STYLE_AND_ARCHITECTURE.md); link to engineering details when necessary.

## Main GDD and separate feature documents

- Use `GDD.md` in this folder as the main GDD when creating the project's design specification. This guide defines the format; it is not the game's approved design.
- Keep the main GDD concise: design pillars, a brief core loop, shared terminology and a linked feature index.
- **If a feature could potentially require more than one page, create a separate Markdown document for it in this same `Documentation/GDD/` folder immediately. Add a link to it in the main GDD.** Do not wait until the main GDD becomes too long.
- Markdown has no fixed pagination. Treat a page as roughly 400–600 words with ordinary formatting. Split earlier if tables, diagrams, multiple states or edge cases make the feature difficult to scan.
- Use descriptive filenames such as `FeatureName.md` or `Progression.md`. Each feature document must link back to the main GDD and to its dependencies.
- The main GDD contains only a short feature summary and its link. Detailed rules live in the feature document; avoid two competing versions of the same specification.
- If a feature has substantial independent subsystems, split them into additional documents in this folder and link them from both the parent feature and the main index.

Recommended feature index columns: feature, summary, priority, design status and specification link. Use priorities `Essential`, `Optional`, `Future` or `Cut`; track design status separately as `Draft`, `Approved` or `Superseded`. Record implementation and validation status separately from design approval.

## Mechanics must be actionable

For each mechanic, define:

- Player intent, available input and conditions for accepting that input.
- State before the action, the transition and observable result.
- Preconditions, costs, rewards, limits, timing and failure conditions.
- Numeric values with units; for formulas, define variables, rounding and boundaries.
- Interactions with other mechanics, progression and saved state.
- UI feedback, input blocking and behavior during interruption or retries.
- Edge cases and measurable acceptance criteria.

Use `TBD` for unresolved values and name the required decision. Never silently invent approved balance numbers. For example, an illustrative reward rule might say: “After a successful objective, grant the configured reward once. Retrying the completion action must not grant it again.” The actual specification must identify the reward source, success condition and persistence behavior.

## References

Every feature must include a References section, or explicitly state that no references are used. For each reference, record the game or source, a URL or repository asset path, and the specific behavior or visual detail it demonstrates. Add a timestamp for videos and captions for screenshots.

Explain what to adopt, what to adapt and what differs in ZooWorld. A reference supports a written requirement; it cannot replace the rules. Distinguish inspiration from assets approved for production, and record provenance or usage restrictions for assets the team intends to reuse.

## Concept art storage and freshness

- Whenever concept art is generated for the game design, save the actual image files locally under the GDD documentation tree, for example `Documentation/GDD/Images/<Feature>/`. Use descriptive filenames and view suffixes. Add relative Markdown links or embedded images in the main GDD or the relevant feature specification; the main GDD must link to that specification. A temporary generator URL or chat attachment alone is not a saved GDD reference.
- Keep only the current concept set in the maintained GDD folders and galleries. Multiple views or alternatives may remain when they are still relevant; label their purpose and draft/approved status without implying approval merely because an image was generated.
- When a concept changes, save its replacement and update all affected GDD links, embeds, galleries and captions in the same change. Remove superseded concept files after checking their references and replacing or removing stale links. Do not leave obsolete alternatives or duplicate versions as active design references; use version control for history. Retain historical copies separately only when explicitly requested.
- Keep related prompts or manifests alongside the current concepts when needed for reproducibility, and update them with the images they describe. These documentation images are separate from production assets imported into Unity.
- Before completion, resolve every added or changed local image/gallery link from its containing document, confirm the target exists, and check that it depicts the current intended design. Check references before deleting superseded files so no maintained GDD document or gallery points to a removed or outdated concept.

## Feature document template

```markdown
# Feature Name

Status: Draft
Priority: Essential / Optional / Future / Cut
Owner: TBD
Updated: YYYY-MM-DD
Main GDD: [ZooWorld GDD](GDD.md)

## Objective and scope
Player benefit, design rationale, included behavior and exclusions.

## Mechanics and flow
Inputs, preconditions, states, transitions, outcomes and failure paths.

## Values and balance
Parameters, units, defaults, limits, formulas and unresolved decisions.

## Presentation and content
UI states, feedback, art, animation, VFX and audio requirements.

## Dependencies and persistence
Related feature links, unlock conditions, saved state and retry behavior.

## Acceptance criteria
Observable checks for normal, boundary, failure and interruption cases.

## References
Sources with links and an explanation of the relevant details.

## Open questions
Decisions needed, owners and impact on implementation.

## Change history
Date, decision and reason.
```

This template is illustrative: its `GDD.md` link becomes valid when the main GDD is created. Fill relevant sections, and mark genuinely inapplicable sections with a reason. Add analytics requirements only when needed, defining event triggers and duplicate handling.

## Maintenance and review

Update the feature specification when an approved rule changes, together with affected dependencies and the main index. Keep proposals visible as drafts until a decision is made. Preserve the reason for major changes and record any implementation gap explicitly.

Before handing a feature to implementation, verify that another team member can determine its behavior and test it using the document. Check that every potentially multi-page feature has its own file and a working main-GDD link, references explain their relevance, and unresolved questions are visible.

## Writing reference

[Strudo: How to Make a Professional Game Design Document (GDD)](https://strudoapp.com/articles/how-to-make-a-professional-game-design-document-gdd) is a reference for system-based organization, explicit mechanics, production requirements and ongoing revision. The one-page split threshold and English-language requirement above are ZooWorld project rules and take precedence over the article's suggested document lengths.
