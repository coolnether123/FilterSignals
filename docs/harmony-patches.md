# Harmony patch inventory

Harmony owner: `CoolNether123.FilterSignals`

| Target | Patch | Purpose |
| --- | --- | --- |
| `ThingFilterUI.DoThingFilterConfigWindow` | prefix/finalizer | Draw and scope transient toolbar state |
| `Listing_TreeThingFilter.Visible` | postfix | Hide rows for disabled classifications without mutating the filter |
| `Listing_TreeThingFilter.DoThingDef` | prefix/postfix | Capture row position and draw status marker |
| `ResearchManager.FinishProject` | postfix | Invalidate all research-dependent results |
| `Building.SpawnSetup` / `Building.DeSpawn` | postfix or prefix/postfix | Invalidate only when a recipe-source building changes |
| `Pawn.SpawnSetup` / `Pawn.DeSpawn` | postfix or prefix/postfix | Refresh colony capability for player pawns |
| `CompPowerTrader.PowerOn` setter | postfix | Refresh production-source usability |
| `CompBreakdownable.DoBreakdown` / `Notify_Repaired` | postfix | Refresh production-source usability |
| `CompRefuelable.ConsumeFuel` / `Refuel(float)` | prefix/postfix | Refresh only on a has-fuel transition |
| `Map.Dispose` | postfix | Release the map-owned cache |

All patches are installed through Spine's `HarmonyUtil.PatchAll`. Errors and
safety-gate skips always produce a FilterSignals warning; successful patch details
are emitted only in RimWorld developer mode.

The isolated verification lane enumerated 14 patched target methods, including
both private item-filter methods. There are no transpilers.
