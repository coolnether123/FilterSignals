# TechSense Filters

TechSense Filters adds colony-aware manufacturing information to RimWorld 1.6
item-filter dialogs. It answers four different questions without changing the
filter itself:

- **Can make**: at least one production path is unlocked and currently usable.
- **Unlocked**: research is complete, but a workstation, capable pawn, or
  optional material requirement is missing.
- **Locked**: production exists, but its research or another colony prerequisite
  is not complete.
- **N/A**: no known production path exists.

The colored square beside each item opens a precise explanation on hover.
The four toolbar buttons temporarily hide or show classifications only in the
open dialog. Saved allow/deny choices are never rewritten.

## Requirements

- RimWorld 1.6
- Harmony
- Spine (`CoolNether123.Spine`)

## Settings

The normal RimWorld mod settings page controls the toolbar, status indicators,
and whether current material shortages count against **Can make**. Material
shortages are ignored by default so normal inventory churn does not make the UI
noisy.

## Compatibility

Vanilla and modded `RecipeDef` products and recipe users are indexed
automatically. Mods with nonstandard production systems can register a provider
or final classification override through the public API documented in
[`docs/compatibility.md`](docs/compatibility.md).

Implementation and verification details are in [`docs/architecture.md`](docs/architecture.md)
and [`docs/verification.md`](docs/verification.md).
