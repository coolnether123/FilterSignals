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

## Camera-preserving navigation polish

Final lane:
`TechSenseFilters-06b03f88bbc14606b4ea060880b57cbc`

- The clean central build used source commit
  `6df93beb64c0f531bcc82073baec5139f2853ffb`.
- The 17 existing domain tests passed unchanged. The toolbar-default gate and
  the new navigation UI boundary gate also passed.
- Before clicking Kibble's colored square, structured camera state was
  `camera.cell=124,120` and `camera.view=80,95..168,145`.
- The click selected the available Architect build designator. Camera state
  afterward was exactly `camera.cell=124,120` and
  `camera.view=80,95..168,145`; no recentering occurred.
- Hover text contains only a short status, short reason, and action line when
  an action exists. No-target squares advertise no unavailable-action prose.
  Their earlier close-on-click behavior was superseded by the later inert-click
  verification below.
- Capture `techsense-architect-camera-preserved-final-20260731-055936-651.png`
  records the open Architect tab, active Kibble row, and short three-line
  TechSense text without changing the visible map position.
- The shipping DLL SHA-256 is
  `0C971A0420F98516FBFB3C99F317D8958853107C9FD10F6AAB00B205B89A8472`.
  Package validation returned `RWT-BUILD-PACKAGE-VALID`.
- The live log contains zero UI-root, illegal-OnGUI, TechSense navigation, or
  Harmony failures. Development Mode is enabled and the proof lane remains
  open.

## Status-square tooltip ownership

Final lane:
`TechSenseFilters-f5b1df97e54e40689884662915ae0541`

- The clean central build used source commit
  `3924f0cd91ecaaad8828dbb211425a5f680d895c` and completed with exit code 0.
- The 17 existing domain tests passed unchanged. The toolbar-default,
  navigation UI boundary, and new indicator-tooltip ownership gates all
  passed.
- Capture `techsense-square-single-tooltip-20260731-214617-318.png` shows that
  hovering Kibble's colored square displays only the short three-line
  TechSense tooltip. RimWorld's item description is no longer stacked with it.
- Capture `techsense-row-vanilla-tooltip-preserved-20260731-214637-649.png`
  shows that hovering the Kibble name still displays the normal vanilla item
  description by itself.
- The shipping DLL is 66,048 bytes with SHA-256
  `5095C207492AA0D10F0A645BF4B1444867349AAE16A9A874BF7D19E8D7F34724`.
  Package validation returned `RWT-BUILD-PACKAGE-VALID`.
- The live log contains zero UI-root, illegal-OnGUI, TechSense error, root
  exception, or Harmony failure matches. Development Mode is enabled and the
  proof lane remains open for manual inspection.

## Small-volume marker layout

Final lane:
`TechSenseFilters-6bf2ce502b794c12b075783c8b419fa5`

- The clean central build used source commit
  `eada34f9cd21d3c6b9abdd9de8b7fc0b656e240d` and completed with exit code 0.
- The 17 domain tests and all three UI gates passed. The indicator gate now
  protects the spacing reserved for RimWorld's small-volume marker.
- The old square position covered the `/1` in gold and silver's vanilla `/10`
  notation, leaving an isolated gray `0`. The status-square column now ends
  before that reserved area.
- Capture `techsense-small-volume-expanded-20260731-233706-808.png` shows gold
  and silver with the complete `/10` marker, no gray oval artifact, and one
  consistently aligned status-square column.
- The shipping DLL is 65,536 bytes with SHA-256
  `5D4D797941C83F0F3CBABC21A23BF9847C092D52F5077B93FC6EC6A6AEA2AC28`.
  Package validation returned `RWT-BUILD-PACKAGE-VALID`.
- The live log contains zero UI-root, illegal-OnGUI, TechSense error, root
  exception, or Harmony failure matches. Development Mode is enabled and the
  proof lane remains open for manual inspection.

## Inert no-action clicks and concise producible wording

Final lane:
`TechSenseFilters-3f82afc247724eaa9df4b932ce3c444c`

- The clean central build used source commit
  `4eb5380d89f40d55146904380a0a35f65aa2bd2c` and completed with exit code 0.
- The 17 domain tests passed unchanged. All three UI gates passed, including
  the revised requirement that a no-action square never clears selection.
- Clicking Chocolate's non-actionable square left the filter fixture visibly
  open. Capture `techsense-inert-after-click-storage-open-20260731-232240-024.png`
  records the result, and the structured fixture probe reported
  `open:true filterUnchanged:true allowedCount:467` afterward.
- A fully satisfied production path now displays "Able to be produced in this
  colony" without a redundant research or production-detail line. The UI gate
  protects both the exact English text and the empty extra-explanation branch.
- The shipping DLL is 65,536 bytes with SHA-256
  `FB8103CC73EC6F02DA93C9EBCB069A2FA3DCB93B3396835139E541A00ECD77BC`.
  Package validation returned `RWT-BUILD-PACKAGE-VALID`.
- The live log contains zero UI-root, illegal-OnGUI, TechSense error, root
  exception, or Harmony failure matches. Development Mode is enabled and the
  proof lane remains open for manual inspection.
