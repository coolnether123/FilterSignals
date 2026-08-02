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

Result: 17/17 tests pass. Coverage includes classification precedence,
research and workstation states, material-shortage policy, actual-instance
recipe acceptance, bill-giver usability, responsive toolbar layout, stable
navigation selection, and safe no-target behavior.

The following unchanged structural/UI contracts also pass:

- `Tests\Test-ToolbarDefault.ps1`
- `Tests\Test-NavigationUiBoundary.ps1`
- `Tests\Test-IndicatorTooltipOwnership.ps1`
- `Tests\Test-SmallVolumeTooltipFixture.ps1`

These protect the hidden-by-default toolbar, camera-preserving navigation,
inert no-action clicks, sole-tooltip ownership, temporary `/10` suppression,
and the focused small-volume runtime fixture.

## Build and package

The central build wrapper compiled commit
`d540a4e7895bc610ca602643382154c85d6541a7` from the canonical
`<repo-root>` root against RimWorld 1.6, Harmony
2.4.2, and the refreshed Spine assembly. The build completed with zero
warnings and zero errors.

The shipping `FilterSignals.dll` is 68,096 bytes with SHA-256
`F3AE39FD4ED9F1D655E206A6C0C4764D1AD2EF9E7F04A1519E97186C32BCAFDB`.
`Test-RwtPackage` reports `RWT-BUILD-PACKAGE-VALID` for package
`CoolNether123.FilterSignals`, RimWorld 1.6, Harmony, and Spine.

The build result is stored outside the source repository at
`<local-temp>\FilterSignalsRenameBuild\build-result.json`.
Its source was clean; the shared tooling checkout was concurrently dirty and
is therefore recorded as such rather than described as a clean tooling build.

## Second-review invalidation checkpoint

Building spawn/despawn and power-state Harmony hooks were removed. Capability
snapshots already refresh on their bounded 120-tick cadence, so those broad
hooks duplicated normal cache expiry. Immediate research completion
invalidation remains because it changes every map's classification at once;
`Map.Dispose` remains for lifecycle cleanup.

The domain suite still passes 17 contracts and 38 assertions, along with the
toolbar, navigation, tooltip-ownership, and small-volume fixtures. The final
centralized RC rebuild produced a 47,616-byte `FilterSignals.dll` with SHA-256
`682501205C52E7CA40F5F90F263C2621F3C13C0201C250550B2755DC2A2A655A`.
In the combined live lane, its Harmony owner count fell from 12 to 8 and no
Filter Signals error was present in either harness or Player.log output.

## Runtime boundary

Earlier focused gameplay sessions remain the behavioral evidence for the real
filter fixture, tooltip/navigation behavior, settings persistence, and
save/reload. The final combined lane
`coolnether-suite-355cca1875a740909cbc91d9c1a59c57` proves the renamed package,
assembly, and Harmony identity coexist in the complete suite: it reached a map,
reported eight Filter Signals-owned patches, and produced no target-mod Error.
