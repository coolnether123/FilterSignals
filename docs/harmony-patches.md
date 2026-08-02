# Harmony patch inventory

Harmony owner: `CoolNether123.FilterSignals`

| Target | Patch | Purpose |
| --- | --- | --- |
| `ThingFilterUI.DoThingFilterConfigWindow` | prefix/finalizer | Draw and scope transient toolbar state |
| `Listing_TreeThingFilter.Visible` | postfix | Hide rows for disabled classifications without mutating the filter |
| `Listing_TreeThingFilter.DoThingDef` | prefix/postfix | Capture row position and draw status marker |
| `ResearchManager.FinishProject` | postfix | Invalidate all research-dependent results |
| `Map.Dispose` | postfix | Release the map-owned cache |

All patches are installed through `SpineApi.Patching.PatchAll`. Errors and
safety-gate skips always produce a Filter Signals warning; successful patch details
are emitted only in RimWorld developer mode.

Building, power, pawn, fuel, and breakdown transitions deliberately use the
bounded 120-tick safety refresh. They do not justify hooks in hot or
compatibility-sensitive paths for a cosmetic status signal. Research keeps
immediate invalidation because its state is global and player-triggered;
`Map.Dispose` remains solely to release map-owned state. There are no
transpilers.
