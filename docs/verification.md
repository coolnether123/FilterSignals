# Verification

Release-gate verification targets RimWorld 1.6 and uses an isolated Agent
Harness lane.

## Automated

`dotnet run --project .\Tests\Mod.Tests.csproj -c Release`

Result: 11/11 domain tests passed. Coverage includes no-path N/A, any-viable-path
wins, unlocked missing source, locked research, unusable source, pawn
capability, optional materials, final override precedence, conditional
per-instance recipe acceptance, same-definition conditional rejection, and
bill-giver usability.

The central RimWorld build pipeline compiled the release assembly with
RimWorld 1.6, Unity modules, Harmony, and standalone Spine references. The
package validator returned `RWT-BUILD-PACKAGE-VALID` with package ID
`CoolNether123.TechSenseFilters`, supported version 1.6, and declared Harmony
and Spine dependencies.

## Final isolated in-game lane

The authoritative release run is performed only after the clean release commit
and packaged DLL exist. Its generated record is
`Engineering/artifacts/final-runtime-evidence.json` (ignored build evidence,
not release input). That record must bind the exact source commit and packaged
DLL SHA-256 to the lane manifest, active mods, game and version-manifest hashes,
log hash, screenshots, and verification outcomes.

The final lane covers:

- quickstart and existing-save addition;
- exact active-mod and Harmony-patch ownership;
- research completion plus workstation spawn/removal invalidation in the same
  game tick;
- two same-definition source instances evaluated by a conditional
  `RecipeWorker`, multi-path definition handling, and representative tooltip
  explanations;
- opening and rendering the real shared `ThingFilterUI` fixture, transient
  toolbar filtering without `ThingFilter` mutation, warm/cold cache timing, and
  a closed-dialog tick sample;
- save/load, post-load UI and filter-state checks, and loading the resulting
  save without TechSense to confirm safe removal;
- final log review for TechSense exceptions, errors, repeated warnings, and
  Harmony failures.

Historical pre-release screenshots or sessions are not release evidence for a
later source commit or DLL.
