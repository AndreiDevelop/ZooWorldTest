# Documentation Guidelines

These rules apply when creating or updating ZooWorld documentation. Explicit user instructions take precedence. Keep documentation useful for the next person who must design, implement, verify or maintain the project.

## Language and format

- Write all new and updated documentation in English, including headings, tables, diagram labels and image captions. Preserve official reference titles and literal identifiers when needed.
- Use Markdown, descriptive filenames and relative links for repository documents. Use full URLs for external references.
- Write concise, concrete statements. Define unfamiliar terms and use consistent names for systems, currencies, content IDs and states.
- Mark unknowns as `TBD` with the decision needed. Distinguish proposals, approved requirements and verified implementation; do not present an intention as existing behavior.

## Location and authority

Keep maintained project documentation under `Documentation/`:

| Location | Purpose |
| --- | --- |
| `DOCUMENTATION_GUIDELINES.md` | Documentation maintenance rules |
| `Engineering/` | Architecture, coding conventions, integration and build instructions |
| `Engineering/Reports/` | Dated findings and validation records |
| `GDD/` | Main GDD, feature specifications and GDD writing rules |

Read [Code Style and Architecture](Engineering/CODE_STYLE_AND_ARCHITECTURE.md) for engineering rules and [GDD Best Practices](GDD/GDD_BEST_PRACTICES.md) for design specifications. Maintain one authoritative description of each rule or system; link to it instead of copying it. Reports describe a point in time and do not override maintained specifications.

## Preserving shared conventions across projects

The [shared architecture requirements](Engineering/CODE_STYLE_AND_ARCHITECTURE.md#shared-architecture-requirements) are user-established rules for every project using this baseline. When adapting documents, preserve their mandatory meaning. A source project's architecture, a legacy implementation, missing packages or an older template must not turn these requirements into optional recommendations or remove them.

Keep project-specific observations and implementation gaps in a separate section. If the user explicitly revises a shared rule, update both the current project's authoritative documents and both the `project-docs-integration` and `unity-project-bootstrap` skills and their templates; record any project-only exception as explicitly scoped. Reapplying the skill merges into the same authorities and retains the shared rules without duplicate sections. Documentation updates alone do not implement the architecture.

## Document naming

Use English ASCII filenames without spaces. Choose the document's role first, then use the corresponding convention:

| Document role | Convention | Example |
| --- | --- | --- |
| Repository agent instructions | Exact standard name at the project root | `AGENTS.md` |
| Shared rules and standing project-wide guides | `UPPER_SNAKE_CASE.md` | `DOCUMENTATION_GUIDELINES.md`, `CODE_STYLE_AND_ARCHITECTURE.md`, `GDD_BEST_PRACTICES.md` |
| Main game design document | Exact name under `Documentation/GDD/` | `GDD.md` |
| Feature or subsystem design specification | `PascalCase.md` under `Documentation/GDD/` | `Renovation.md`, `PuzzleRewards.md` |
| Focused engineering guide or implementation plan | Descriptive `kebab-case.md` under `Documentation/Engineering/` | `save-migration.md`, `startup-preload-plan.md` |
| Dated audit, investigation or validation report | `YYYY-MM-DD-topic.md` under `Documentation/Engineering/Reports/` | `2026-10-16-startup-preload-validation.md` |
| Folder index, when needed | Exact standard name in the indexed folder | `README.md` |

- Keep names short and specific to the subject. Use the same feature/system terminology as the main GDD and code; avoid vague names such as `notes.md`, `new-document.md` or `misc.md` for maintained specifications.
- Keep dates, author names, language suffixes and revision numbers out of maintained guide/specification filenames. Record status, owner, updated date and significant decisions inside the document; use Git for revisions. Do not create `final`, `final2`, `latest` or `v2` copies of the same authority.
- Use the report date as the filename prefix. If several reports share a date, distinguish their scopes with meaningful topic suffixes. Update the same report for the same investigation rather than creating numbered copies.
- Preserve standard acronyms in standing guide names (`GDD`, `CI`, `SDK`); use lowercase words/acronyms in kebab-case names. File links must match the actual case, including on case-sensitive filesystems.
- Apply these conventions to new documents. Keep established filenames and folder names unless a rename is part of the task; do not perform an incidental filename migration. When renaming a document, update all repository links and agent instructions in the same change and preserve one authoritative copy.
- Mirror the filename topic in a readable English H1. Put drafts and approval status inside the document, not in its filename. A naming example does not imply that the feature or system exists.

## Writing and updating a document

1. Read the relevant existing documents and implementation before making factual claims.
2. Choose the existing authoritative document, or create a focused document if the subject has no suitable home.
3. State its purpose, scope and status. For evolving specifications, include an owner or `TBD`, and an updated date in `YYYY-MM-DD` format.
4. Describe the actual rules, dependencies and limitations. Use examples, tables or diagrams where they resolve ambiguity.
5. Link to related specifications, relevant source files and references. Label illustrative examples so they cannot be mistaken for approved project values.
6. When behavior changes, update affected documentation and cross-links in the same change. If design and implementation differ, record the gap explicitly.
7. Keep a brief dated change history for significant design decisions, including the reason. Git remains the detailed edit history.

## Evidence and verification

- Record how implementation claims were checked. Use repository paths, configuration names, test results or dated observations as appropriate.
- For validation reports, include scope, environment, checks performed, results and remaining issues. Never report an unrun check as passing.
- Keep generated dumps and large logs outside maintained prose; link to a relevant report when useful.
- Do not place credentials, tokens or private payloads in documentation or screenshots.

## Review checklist

- The text is in English and its status is clear.
- Rules and examples agree with the current authoritative specifications.
- Filenames follow the role-based naming conventions above; relative links and heading anchors resolve.
- New documents are discoverable from a relevant index or parent document. Update `AGENTS.md` when adding mandatory project-wide working rules, rather than listing every feature there.
- Obsolete instructions are updated or clearly marked as superseded with a link to their replacement.
- The document contains enough information to act on, without repeating unrelated background.
