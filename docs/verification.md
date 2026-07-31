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

The authoritative release run completed against clean commit
`451d9706e0d4791dd37ed24f289a0cd102d6d320` and packaged DLL SHA-256
`43F67AD68C279AEE6CFE38F3049CCB05EFD16DFE495DEAC38B43BFA150096F4C`.
The generated record is
`Engineering/artifacts/final-runtime-evidence.json` (ignored build evidence,
not release input), with SHA-256
`E752635AC85524748FC1A86907CD66F66DD103BAC956CBB92B16C4B87C2AE298`.
It binds the exact source, DLL, lane manifests, active mods, game and
version-manifest hashes, logs, saves, screenshots, and outcomes.

Final lane `TechSenseFilters-c16476171867471a97186bbf5a507909`
verified:

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
- final log review with zero TechSense exceptions, errors, repeated warnings,
  or Harmony failures.

The clean central build recorded `SourceDirty=false`, used tooling commit
`e27edbb89f998870ea4e1383171ad45e05d115fa`, and reproduced the packaged DLL
exactly. The removal lane
`Spine-d3449e33c04b4e3f882b997dd2e1fc8d` loaded and re-saved the final save
without TechSense and without error or exception matches. All lanes exited
cleanly with exit code 0.

Historical pre-release screenshots or sessions are not release evidence for a
later source commit or DLL.

## Responsive-toolbar and navigation follow-up

Automated coverage now reports `PASS: 17 TechSense domain tests`. The added
tests verify a 264-pixel two-row layout with four non-overlapping buttons,
wide inline layout, colony-specific no-path wording, deterministic usable-source
selection across multiple paths, missing-research navigation, missing-workbench
build navigation, and safe no-target behavior.

The final central Release build completed with exit code 0 against clean
RimWorld-Tooling commit
`b639ebf6ddedd1d26064903c2391837b5c8c58f9`. Package validation returned
`RWT-BUILD-PACKAGE-VALID`; the packaged `TechSenseFilters.dll` is 65,536 bytes
with SHA-256
`52014D7DAE3F29EF7281422F9386C6C7C430448618278B00929184F1ACA5A996`.

Isolated lane `TechSenseFilters-e971922cf9c74a2f9d8b3fa945c2b082`
used exactly Core, Harmony, RimWorld Agent, Spine, and TechSense Filters:

- `narrow-toolbar-drag2-20260731-013504-787.png` shows the real resizable
  `ThingFilterUI` fixture at approximately 300 pixels of filter width. The
  title remains separate and the four full labels render in a readable 2x2
  grid above untouched vanilla controls.
- Clicking Pemmican's locked square changed the active main tab to Research
  and selected the Pemmican project.
  `navigation-click-locked-20260731-013542-863.png` shows the selected research
  detail and the full deterministic-navigation tooltip.
- Clicking Kibble's unlocked square changed the active main tab to Architect
  and activated the missing butcher-spot build path.
  `navigation-click-unlocked-20260731-013628-104.png` preserves that state.
- The fixture continued to display `Permanent filter state: unchanged`.
  Log review found no exception matches or TechSense navigation errors.
- The lane stopped with exit code 0 and `ForcedTermination=false`.
