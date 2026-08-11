# Verification

Release-gate verification targets RimWorld 1.6 and uses the generalized
RimWorld Agent harness.

## Current Filter Signals identity

- Player-facing name: `Filter Signals`
- Package ID: `CoolNether123.FilterSignals`
- Assembly: `FilterSignals.dll`
- Source namespace: `Filter Signals`
- Harmony owner and Spine consumer ID: `CoolNether123.FilterSignals`
- Language-key prefix: `FilterSignals_`

The rename intentionally provides no package-ID, assembly, namespace, API,
settings, or language-key alias for the superseded identity. The current
repository contains only the new identity.

## Automated contracts

The domain suite is run with:

`dotnet run --project .\Tests\FilterSignals.Tests.csproj -c Release`

Result: 25/25 tests pass (56 assertions). Coverage includes classification
precedence, research and workstation states, material-shortage policy,
actual-instance recipe acceptance, bill-giver usability, responsive toolbar
layout, stable navigation selection, safe no-target behavior, legacy
`ProductionPathAssessment` constructor call shapes and exact public CLR
constructor reflection, plus ambiguous navigation rejection.

The following unchanged structural/UI contracts also pass:

- `Tests\Test-ToolbarDefault.ps1`
- `Tests\Test-NavigationUiBoundary.ps1`
- `Tests\Test-IndicatorTooltipOwnership.ps1`
- `Tests\Test-SmallVolumeTooltipFixture.ps1`
- `Tests\Test-ClassificationExceptionRecovery.ps1`
- `Tests\Test-CompatibilityIsolation.ps1`

These protect the hidden-by-default toolbar, camera-preserving navigation,
inert no-action clicks, sole-tooltip ownership, settings-gated temporary
`/10` suppression, the focused small-volume runtime fixture, transient
evaluation-exception fallbacks, and provider/override ordering and isolation.
The first four are the required UI fixtures; the final two are additional
source contracts.

## Historical Aug. 7 build and runtime evidence

The historical central build wrapper compiled commit
`793ad06f1ca09d5708be72666f2bd3491847655e` from the canonical
`<repo-root>` root against RimWorld 1.6, Harmony
2.4.2, and the refreshed Spine assembly. The build completed with zero
warnings and zero errors.

The 2026-08-07 release-gate shipping `FilterSignals.dll` was 47,616 bytes with SHA-256
`C76D01E03D22E22C8B6FFCBA072CDE57B3FDA24D2FB22A320BB71F2DA7B51E2F`.
`Test-RwtPackage` reports `RWT-BUILD-PACKAGE-VALID` for package
`CoolNether123.FilterSignals`, RimWorld 1.6, Harmony, and Spine.

The build result is stored outside the source repository at
`A:\Dev\RimWorld\Temp\FilterSignals-1.6-build-20260807-r1\build-result.json`.
Its source was clean; the shared tooling checkout was concurrently dirty and
is therefore recorded as such rather than described as a clean tooling build.
This provenance and runtime evidence are historical Aug. 7 evidence only and
do not describe the current dirty worktree.

## Second-review invalidation checkpoint

Building spawn/despawn and power-state Harmony hooks were removed. Capability
snapshots already refresh on their bounded 120-tick cadence, so those broad
hooks duplicated normal cache expiry. Immediate research completion
invalidation remains because it changes every map's classification at once;
`Map.Dispose` remains for lifecycle cleanup.

The historical domain suite passed 20 contracts and 46 assertions, along with the
toolbar, navigation, tooltip-ownership, and small-volume fixtures. The final
centralized RC rebuild produced a 47,616-byte `FilterSignals.dll` with SHA-256
`C76D01E03D22E22C8B6FFCBA072CDE57B3FDA24D2FB22A320BB71F2DA7B51E2F`.
In the combined live lane, its Harmony owner count fell from 12 to 8 and no
Filter Signals error was present in either harness or Player.log output.

## Runtime boundary

Earlier focused gameplay sessions remain the behavioral evidence for the real
filter fixture, tooltip/navigation behavior, settings persistence, and
save/reload. The final combined lane
`coolnether-suite-355cca1875a740909cbc91d9c1a59c57` proves the renamed package,
assembly, and Harmony identity coexist in the complete suite: it reached a map,
reported eight Filter Signals-owned patches, and produced no target-mod Error.
## Final release-candidate gate — 2026-08-03

Passed 20 contracts (46 assertions), tooltip-ownership, navigation, compact
volume, toolbar-default, clean build, and package checks. Live verification
with all eight gameplay mods showed compact status squares, one stable tooltip,
no grey artifacts, and a hidden-by-default toolbar. The UI/content stack also
loaded with RimHUD, Better Pawn Control, Achtung, Vanilla Expanded production,
Rimatomics, and Rimefeller without a Filter Signals exception.

## Current 1.6 compatibility rerun — 2026-08-07

The following compatibility observations are historical Aug. 7 evidence and
do not claim a pass-three runtime rerun.

Fresh Workshop payloads were loaded with the verified 1.6 release assembly in
isolated AgenticHarness lanes. The direct collision cases were exercised through
the real bill/filter path where applicable:

- Filter Manager (`Jaxe.FilterManager`, Workshop 2812197851): compatible on
  the exercised surface. Its Invert/Clear/Presets controls and Filter Signals
  were visible together in `Dialog_BillConfig`; no in-game exception occurred.
- Stockpile Stack Limit (Continued) (`Mlie.StockpileStackLimit`, Workshop
  2274678322): compatible on the exercised surface. Both mods patched
  `ThingFilterUI.DoThingFilterConfigWindow`; the live filter window opened and
  rendered without an exception.
- Recipe icons (Continued) (`Mlie.RecipeIcons`, Workshop 2904906618): no direct
  overlap in the current 1.6 payload. Its four Harmony patches do not target
  `Listing_TreeThingFilter.DoThingDef`; the lane produced no exception.
- Dubs Mint Menus, Nice Bill Tab, Better Workbench Management, and Adaptive
  Storage Framework all loaded with Filter Signals, retained the expected
  Filter Signals patch set, and produced no exception in the exercised
  startup/UI surface.
- Research Reinvented's `ResearchManager.FinishProject` postfix coexisted with
  Filter Signals. One combined run reported missing Research Reinvented texture
  files; the same payload run alone did not reproduce that warning, so it is
  recorded as external payload noise rather than a Filter Signals failure.

These are scoped runtime observations, not a blanket compatibility promise.
The unrequested research-tree replacements, Search Agency's legacy 1.1
payload, and large production-mod stacks remain outside this rerun.

## Final correction passes — 2026-08-09

The pass-five visual record below was produced from a dirty
working tree and is not attributed to clean commit
`793ad06f1ca09d5708be72666f2bd3491847655e`. The shared build wrapper was
attempted repeatedly but remained blocked before compilation by the
pre-existing empty `isolated-benchmark.managedDir` manifest entry. Shared
infrastructure was not edited.

The isolated pass-five MSBuild compile used the resolved RimWorld 1.6 managed
assemblies, Harmony 2.4.2, Spine 1.6, deterministic compilation, and the shared
`RimWorld.Mod.props`, writing to
`A:\Dev\Temp\FilterSignals-Pass5-Diagnostic-20260809`. It completed with zero
warnings and zero errors. Its 54,272-byte assembly has SHA-256
`204C2EF335D9EAE60FF9D45043C6B224FD9C4282BF20B26F3AAEDCE41DE06372`.
The staged assembly matched that output, and `Test-RwtPackage` returned
`RWT-BUILD-PACKAGE-VALID`.

The exact pass-five hash ran in isolated RimWorld 1.6 session
`FilterSignals-2c597784d7814346bc6ec064cbe1e5ea` with Harmony, Spine, the
developer fixture, Filter Manager, Stockpile Stack Limit, Recipe Icons, and
Nice Bill Tab. Captures verified that the measured toolbar title remains on one
readable line without overlapping the first button; disabling indicators
restored vanilla `/10` and its tooltip, while enabling indicators suppressed
`/10` and showed the Filter Signals small-volume explanation. Ordinary and
Alt-modified IMGUI clicks preserved the filter and camera. Save/load,
classification, research, multi-path, cache, Harmony, and log probes completed
without a Filter Signals exception.

The preceding pass-four hash also completed a process-restart/save-load lane
with Dubs Mint Menus, Research Reinvented, Adaptive Storage Framework, Vanilla
Expanded Framework and Furniture, and Rimefeller. That lane exercises the same
classification/navigation code; pass five changes only the responsive toolbar
layout and text-state restoration. The final domain suite is 25 tests and 56
assertions, with all six PowerShell fixtures passing.

After that visual pass, the final review found that adding optional parameters
to the public `ProductionPathAssessment` constructor had preserved source
compatibility but not its original CLR signature. Pass six restored the exact
eight-parameter public constructor and added reflection-level invocation
coverage for both that legacy ABI and the extended metadata constructor. No
toolbar or other runtime UI source changed after the pass-five captures.

The current pass-six assembly is 54,784 bytes with SHA-256
`544044E3D282F55E82C8FBF7E620A743FB56C509F1301DCE734C7621DBD20A4C`.
That exact hash loaded Core, Harmony, RimWorld Agent, Spine, Filter Signals,
and the tracked test fixture in isolated session
`FilterSignals-491c8133b39e49a086c691e2ed124767`. It reached a ready map,
registered all six fixture actions, produced no Filter Signals exception or
error, and stopped with exit code 0 without forced termination. The supervising
CLI exceeded its wrapper timeout before a fresh pass-six UI capture, so the
wrapping proof remains the pass-five visual evidence; the exact pass-six lane
is a load/map/log smoke test, not a claimed fresh UI click.

At that checkpoint the assembly remained a runtime-verified diagnostic
candidate rather than a sanctioned wrapper artifact.

The resolver manifest identifies the RimWorld 1.6 compile/runtime depot as
`1.6.4871 rev573`, while the captured `Player.log` reports
`1.6.4871 rev574`. This environment discrepancy remains part of the evidence
and is not treated as resolved or silently normalized.

## Sanctioned release package gate — 2026-08-10

After the shared depot resolver was corrected to ignore the unavailable
benchmark reference for compile resolution, the repository-owned
`Tools\Build-Version.ps1 -Configuration 1.6` entrypoint completed through the
sanctioned `Invoke-RimWorldBuild.ps1` workflow. It compiled source commit
`95215cc2ed9a7cf98a3bc5c7c41a7a244bd43a81` with zero errors, recorded
`SourceDirty=false`, and produced a deterministic 57,856-byte
`FilterSignals.dll` with SHA-256
`A3D65C70B9AF8C2281DCF28886D9472AEA1816DD867EBF0E532CC6D3B8D2CA1D`.
The intentionally untracked repository-local `AGENTS.md` was excluded from the
status query without staging or modifying it.

The build result is
`A:\Dev\RimWorld\Temp\FilterSignals-release-build-20260810-r4\build-result.json`.
It records tooling commit `59a07ec53affcf93041727abbbe83c14fd6370c3`,
manifest SHA-256
`2B85365E144D8D4F5FCCBEF1BC4029928866D23208733CF2FDAFF526F0033638`,
and common-props SHA-256
`42EA70E0E56A001BF4B185F2DA9E60100AB07B076552827E8359C668F8F0739B`.
The shared tooling worktree was dirty with separately owned infrastructure
changes, so the exact inputs are recorded instead of claiming clean tooling
provenance.

`New-RwtReleasePackage` staged the allowlisted package at
`A:\Dev\RimWorld\Releases\FilterSignals\1.0.0-1.6-20260810-final` with only
`About`, `1.6\Assemblies\FilterSignals.dll`, `Languages`, and `LICENSE`.
It excluded the PDB, source, tests, and developer fixture and returned
`RWT-BUILD-RELEASE-PACKAGE-VALID`; an independent `Test-RwtPackage` call also
returned `RWT-BUILD-PACKAGE-VALID`.

The matching distribution archive is
`A:\Dev\RimWorld\Releases\FilterSignals\FilterSignals-1.0.0-RimWorld-1.6-final.zip`.
It is 254,416 bytes with SHA-256
`E2FDA4B0F37450EB3AF0145A24A56483AD49BC5B7CD35AFA818AA262109373DD`.

The exact packaged hash ran in isolated session
`1.0.0-1.6-20260810-final-377a9ec6f1884ac08600a9eb86c00cd7` with Core, Harmony,
RimWorld Agent, Spine, and the separately staged developer fixture. It reached
a ready map, rendered classification squares, kept all 467 filter definitions
unchanged, invoked the focused filter fixture, registered eight Filter
Signals-owned Harmony patches, and produced no `error`, `exception`, or
`fatal` match in `Player.log`.
The staged snapshot DLL matched the package hash. Shutdown completed with exit
code 0 and `ForcedTermination=false`.

The preceding sanctioned build from the patch-equivalent local cascade
lineage separately exercised all six fixture actions, representative
explanations, and the cold/warm cache probe. Those results are supporting
behavioral evidence only; the session above is the exact final-hash runtime
claim.

The exact package captured the default-hidden toolbar and status-square UI.
Windows focus arbitration prevented a reliable fresh toolbar-on click, so the
toolbar wrapping proof remains the pass-five capture from the same unchanged
toolbar source. No fresh sanctioned-hash toolbar-on interaction is claimed.
