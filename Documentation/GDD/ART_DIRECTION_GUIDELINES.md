# Art Direction Guidelines

This is a portable production baseline. Preserve the destination project's approved art direction and references; do not copy another game's palette, characters or art assets as universal defaults. Keep actual palette values and reference measurements in the target's authoritative art guide.

## Icon families and outlines

- Group comparable icons by their visual role and intended displayed size: equipment tiers, resources, HUD symbols and portraits may have distinct line hierarchies. Within one family, match contour color, visible thickness, cap/join character, highlight treatment and the relationship between outer contour and internal detail.
- Compare at equal intended screen size in Unity, after import and atlas packing. Source-pixel widths and PNG dimensions alone do not establish equal line weight. Record the reference display size and chosen thickness/tolerance per family; inspect representative small/mobile sizes as well.
- Keep tier/variant silhouettes recognizable while standardizing outlines. Do not add a second halo, change framing, grow transparent silhouettes or change sprite pivots/slicing merely to match line weight. Preserve intentional smaller internal lines. Use a shared master/reference and preserve its ink when adding color.
- Color-only reskins retain silhouettes, transparent alpha, composition, component dimensions, text, navigation, raycasts, data bindings and behavior. Match palette roles and state colors to approved references. Simple glyphs and text stay crisp; texture must not obscure them.
- Inspect icons together on light and dark backgrounds, in selected, locked and disabled states and at actual UI scale. Check alpha fringes, clipping, apparent size and outline consistency after packing. Record actual visual checks rather than claiming uniformity from a generation prompt.

## Import invariants

Preserve existing source dimensions, GUIDs, sprite IDs/rectangles, PPU, pivots, borders, filtering, mipmaps, compression format/quality/Crunch and platform overrides during a color-only reskin unless the user explicitly authorizes changing them. Preserve atlas packing and compression settings as well. New assets match the closest existing role. A palette change does not authorize a compression or resolution optimization.

## Scope and evidence

Use original or authorized artwork. Distinguish concept previews from verified runtime captures. Document project-specific style decisions in the existing art/UI guide and link the relevant evidence. Installing these rules does not generate art, rebuild UI or implement a rendering effect.
