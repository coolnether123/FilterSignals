# Verification

Verified on 2026-07-30 against RimWorld 1.6 using an isolated Agent Harness lane.

## Automated

`dotnet run --project .\Tests\Mod.Tests.csproj -c Release`

Result: 8/8 domain tests passed. Coverage includes no-path N/A, any-viable-path
wins, unlocked missing source, locked research, unusable source, pawn
capability, optional materials, and final override precedence.

The central RimWorld build pipeline compiled the release assembly with
RimWorld 1.6, Unity modules, Harmony, and standalone Spine references. The
package validator returned `RWT-BUILD-PACKAGE-VALID` with package ID
`CoolNether123.TechSenseFilters`, supported version 1.6, and declared Harmony
and Spine dependencies.

## Isolated in-game lane

Session:
`TechSenseFilters-1fd2b7ac69374cbc97deb85c0ed7b605`

- Quickstart reached a ready map with three free colonists.
- The harness enumerated all 14 TechSense Harmony targets, including
  `Listing_TreeThingFilter.Visible` and `DoThingDef`.
- The deterministic verification fixture used the same vanilla
  `ThingFilterUI.DoThingFilterConfigWindow` path as normal dialogs.
- Can make, Unlocked, Locked, and N/A toolbar controls rendered with compact
  per-item markers and hover explanations.
- Disabling Locked removed the locked Pemmican row while the fixture continued
  to report `filterUnchanged:true` and `allowedCount:467`.
- A save named `TechSenseFinal` completed, loaded back as generation 1, and
  paused at tick 14513. Reopening the fixture after load preserved the UI and
  again reported the permanent filter unchanged.
- With the filter dialog closed, a forced 600-tick sample completed in
  0.257124 seconds (2333.50 ticks/second), demonstrating that the mod adds no
  per-tick polling path while the UI is closed.
- The lane log contained no TechSense exception, error, or repeated runtime
  warning.

Primary screenshots:

- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\TechSenseFilters-1fd2b7ac69374cbc97deb85c0ed7b605\ipc\captures\techsense-final-all-settled-20260730-194431-404.png`
- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\TechSenseFilters-1fd2b7ac69374cbc97deb85c0ed7b605\ipc\captures\techsense-final-locked-hidden-settled-20260730-194649-979.png`
- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\TechSenseFilters-1fd2b7ac69374cbc97deb85c0ed7b605\ipc\captures\techsense-after-save-load-20260730-194752-156.png`

The profile scanner also reports metadata warnings belonging to unpublished
Spine and unrelated inactive work-in-progress mods present in the development
catalog. Those are outside this repository; TechSense does not fabricate a
Spine download URL.
