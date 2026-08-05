# Filter Signals

Filter Signals adds colony-aware manufacturing information to RimWorld 1.6
item-filter dialogs. It answers four different questions without changing the
filter itself:

- **Can make** — at least one production path is unlocked and currently usable.
- **Unlocked** — research is complete, but a workstation, capable pawn, or
  optional material requirement is missing.
- **Locked** — production exists, but its research or another colony
  prerequisite is not complete.
- **N/A** — no known production path exists.

The colored square beside each item shows one short status explanation on
hover, without stacking the item's normal description underneath it.
Small-volume materials explain "10 units = 1" there instead of displaying
RimWorld's inline `/10` marker in the item row.

Clicking a square navigates to a usable workstation, the relevant research
project, or an available Architect build option when a safe vanilla target
exists. Architect navigation preserves the current camera position. If no
action is available, clicking the square leaves the storage panel open and
unchanged.

The colored squares are the default interface. An optional four-button toolbar
can temporarily hide or show classifications only in the open dialog; it is
disabled by default and can be enabled in mod settings. Saved allow/deny
choices are never rewritten.

Alt-click a status square or optional toolbar control to open and highlight its
setting. Alt-click never navigates to research, a workstation, or the Architect
menu, and never toggles the temporary view filter.

## Requirements

- RimWorld 1.6
- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
- [Spine](https://github.com/coolnether123/Spine) — the shared runtime used by
  CoolNether123 mods

## Installation

Install Harmony and Spine, copy `FilterSignals` into RimWorld's `Mods` folder,
then enable Harmony, Spine, and Filter Signals in that order.

Filter Signals stores only global display preferences and never rewrites saved
allow/deny choices, so it is safe to add to or remove from an existing save.

## Settings

The normal RimWorld mod settings page can enable the optional filter toolbar,
control status indicators, and choose whether current material shortages count
against **Can make**. The toolbar and material-shortage check are disabled by
default so the stockpile panel stays close to vanilla.

## Compatibility

Vanilla and modded `RecipeDef` products and recipe users are indexed
automatically. Mods with nonstandard production systems can register a provider
or final classification override through the public API documented in
[`docs/compatibility.md`](docs/compatibility.md).

## Documentation

- [Architecture](docs/architecture.md)
- [Harmony patches](docs/harmony-patches.md)
- [Technical investigation](docs/technical-investigation.md)
- [Duplicate research](docs/research/duplicate-check.md)
- [Verification record](docs/verification.md)

## Developer fixture

Live debug actions are isolated in `Developer/FilterSignals.TestFixture`, a
separately loadable developer mod. Build and load that folder only for harness
verification; it is never part of the Filter Signals shipping package.

## License

Released under the [MIT License](LICENSE). Harmony and Spine are used under
their own licenses.
