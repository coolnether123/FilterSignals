# Technical investigation

## RimWorld 1.6 integration points

The vanilla item-filter UI has one shared entry point,
`ThingFilterUI.DoThingFilterConfigWindow`. Its `UIState` is an appropriate
identity for transient dialog presentation, while the actual visible leaf
decision is made by private `Listing_TreeThingFilter.Visible(ThingDef)`.
Individual definition rows are drawn by private
`Listing_TreeThingFilter.DoThingDef(ThingDef, int, Map)`.

FilterSignals therefore uses:

1. a prefix/finalizer around the shared filter window to draw the toolbar and
   scope the current presentation context;
2. a Boolean postfix on `Visible` to apply temporary classification views; and
3. a prefix/postfix around `DoThingDef` to place the compact status marker at
   the row that vanilla just drew.

This covers every vanilla caller of the shared item-filter widget instead of
special-casing stockpiles, bills, outfits, food restrictions, or caravan
dialogs. It also preserves the exact vanilla `ThingFilter` object and its
serialized allowances.

## Production semantics

All non-surgery `RecipeDef` entries with products are indexed. Sources come
from `RecipeDef.AllRecipeUsers`, which includes modded recipe users after
definition resolution. `RecipeDef.AvailableNow` is used as the full current
availability gate, while unfinished explicit research prerequisites are
enumerated to produce a better explanation.

A source is present when a player production building of the matching
definition is spawned. The map snapshot retains every actual source instance;
it does not collapse buildings with the same `ThingDef`. A path is usable only
when the same instance passes both
`IBillGiver.CurrentlyUsableForBills()` and
`RecipeDef.AvailableOnNow(actualBuilding)`, matching the two distinct vanilla
gates. This preserves conditional behavior implemented by vanilla or modded
`RecipeWorker` subclasses, while still allowing any accepted instance or
alternate recipe path to win. Pawn capability checks required work types,
recipe skill requirements, and mechanitor-only recipes against active player
colonists and colony mechs.

Material checks use `RecipeDef.PotentiallyMissingIngredients` only when the
player enables that setting. It is disabled by default because inventory
changes are frequent and “production capability” is normally more useful than
“ingredients happen to be present this instant.”

Resource-producing entities, transformations, or framework-defined production
that does not resolve to a `RecipeDef` is intentionally not guessed. Those
systems can register a deterministic production provider or a narrow final
override through the documented public API.

## Duplicate investigation

The authoritative pre-implementation search and closest adjacent mods are
recorded in [`research/duplicate-check.md`](research/duplicate-check.md). No
original Discord author was publicly identifiable, so the mod does not invent
an attribution.
