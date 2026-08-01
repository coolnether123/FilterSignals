# TechSense Filters compatibility investigation — 2026-08-01

## Scope and confidence

This is an evidence report, not a blanket compatibility promise. All valid runtime
results below used `H:\Games\RimWorld1-6-4871Win64\RimWorldWin64.exe`, RimWorld
`1.6.4871 rev573`, developer mode enabled, Core only, and an isolated harness
profile. Workshop folders on `D:` were inputs only. The investigated TechSense
revision was `f0bdc59166c082a33095fd1d221b6e0f5fe990f0`; its DLL SHA-256 was
`87747350840B362CFFDADDE48861CC943D819D083A8D147CDD9DAD91870C7C1D`.
Spine's DLL SHA-256 was
`F0773EC3E03DE4B35F5AA10AFFFAB42484BDB12BB38AFD7A061D97322F6D0C54`.

The Workshop date below is the local Workshop folder timestamp, used as the
download-snapshot proxy because Steam does not preserve a separate download
receipt in these folders. Every pair included Harmony, RimWorld Agent, Spine,
and TechSense in that order before the listed external mod unless the reversed
order is explicitly shown.

## Results

### TechSense alone — compatible

- Mod: TechSense Filters, `CoolNether123.TechSenseFilters`, local source at the
  revision above; Core only; load order `Core > Harmony > RimWorld Agent > Spine
  > TechSense Filters`.
- Scenario: fresh game, filter fixture, classification/cache probes, settings,
  save/reload, and removal-safety work from the project's preceding verification
  lane.
- Evidence root:
  `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\TechSenseFilters-c16476171867471a97186bbf5a507909`.

### Better Workbench Management — compatible in the exercised surface

- External: Better Workbench Management, `falconne.BWM`, Workshop `935982361`,
  version `1.6.1.3`, snapshot `2025-07-13T22:48:10.8621144Z`. Its tested DLL
  SHA-256 was
  `0913320230337B8482C3903454A3E1417BAFB6A7AA80A3703D936736CBA827D4`.
- Scenarios: BWM alone; TechSense before BWM; BWM before Spine/TechSense; fresh
  map; real fueled stove; real bill; bill-details ingredient filter; optional
  TechSense toolbar; transient Locked view; saved filter-state non-mutation;
  settings persistence; save/reload.
- Both load orders reached play and reloaded without matching exceptions. The
  real narrow bill window showed both mods' controls without overlap. TechSense
  reported `filterUnchanged:true`, `allowedCount:467`.
- Harmony ownership was separated: TechSense owned the ThingFilter drawing
  hooks; BWM owned its bill and `Dialog_BillConfig` hooks. No shared method had
  competing patches in the inspected inventory.
- Evidence:
  `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\TSCompatTsBwm-23cfafca522746809b35a16a7da35c04`,
  `...\TSCompatBwmTs-0b89533f046d4e12a71e8516b4e2c097`, and
  `...\TSCompatBwmOnly-9ba05bbc2fa044119a6893f331d91a1d`. The strongest UI
  capture is `ipc\captures\ts-bwm-bill-details-filter-20260801-005915-726.png`.
- Limitation: BWM removal from the copied pair save and long simulation were not
  run. A blank intermediate capture was reproduced as harness command
  cross-contamination and disappeared when commands were serialized; it is a
  false alarm, not a game defect.

### Adaptive Storage Framework — compatible with a documented limitation

- External: Adaptive Storage Framework, `adaptive.storage.framework`, Workshop
  `3033901359`, no declared mod version, snapshot
  `2025-09-28T18:16:41.2679169Z`. Tested DLL SHA-256:
  `28DAA37ADE4144CAD2B7669EDFDA2201F9C0E8E99D4639853131E066026935B9`.
- Scenario: fresh map, shared ThingFilter fixture, non-mutation, optional toolbar,
  capability probes, setting persistence, save/reload, Harmony inventory.
- Result: no matching exception. TechSense exclusively owned shared ThingFilter
  rendering; ASF patched `ThingFilter.ExposeData` and a special filter worker.
  The fixture remained unchanged with 467 allowed definitions.
- Evidence:
  `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\TSCompatAsf-9b1ce74bc13044b3a7b79f710cc33e49`.
- Limitation: ASF is a framework and this lane did not include a concrete ASF
  storage content mod, so presentation inside an ASF storage building remains
  pending.

### Dubs Mint Menus — compatible with a documented limitation

- External: Dubs Mint Menus, `Dubwise.DubsMintMenus`, Workshop `1446523594`,
  version `1.3.1247`, snapshot `2025-07-13T22:48:10.4353841Z`.
- Load order: `Core > Harmony > Agent > Spine > TechSense > Dubs Mint Menus`.
- Scenario: fresh map, shared filter fixture, non-mutation, capability and cache
  probes, toolbar, settings persistence, save/reload, Harmony summary.
- Result: no matching exception; 467 definitions; the captured filter was
  aligned and usable. Harmony reported 21 TechSense and 18 Mint Menus-owned
  patches without a demonstrated collision. Cold classification was 1 ms and
  warm classification 0 ms in this diagnostic run.
- Evidence:
  `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\TSCompatMint-64367224e73c449a8035941f7a7987b1`,
  especially `ipc\captures\ts-mint-filter-20260801-010709-393.png`.
- Limitation: the Mint Menus research and Architect replacements and TechSense's
  colored-square navigation into them were not exercised. A capability probe's
  `noSpawnCells` result is an inconclusive fixture precondition, not a mod error.

### Research Reinvented — compatible with a documented limitation

- External: Research Reinvented, `PeteTimesSix.ResearchReinvented`, Workshop
  `2868392160`, no declared mod version, snapshot
  `2025-09-28T18:16:41.7290161Z`.
- Load order: `Core > Harmony > Agent > Spine > TechSense > Research Reinvented`.
- Scenario: fresh map, shared filter fixture, real classification invalidation
  probes, toolbar, settings persistence, save/reload, Harmony summary.
- Result: no matching exception; filter unchanged with 471 definitions. The
  research probe moved packaged survival meals from `CannotMakeYet` to
  `ResearchUnlocked`; a spawned/removed workstation moved tribalwear
  `ResearchUnlocked > CanMakeNow > ResearchUnlocked`. Cold/warm classification
  was 1/0 ms. Harmony reported 37 Research Reinvented and 21 TechSense-owned
  patches without a demonstrated collision.
- Evidence:
  `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\TSCompatResearch-20e1ee8296c34234bf392c8d2f49d144`.
- Limitation: clicking a TechSense status square into Research Reinvented's
  actual research interface was not exercised.

### Vanilla Expanded Framework + Vanilla Furniture Expanded — compatible with a documented limitation

- External: Vanilla Expanded Framework,
  `OskarPotocki.VanillaFactionsExpanded.Core`, Workshop `2023507013`, snapshot
  `2026-05-21T07:02:36.3857086Z`; Vanilla Furniture Expanded,
  `VanillaExpanded.VFECore`, Workshop `1718190143`, snapshot
  `2025-07-13T22:48:10.2967212Z`.
- Load order: `Core > Harmony > Agent > Spine > TechSense > VEF > VFE`.
- Scenario: content-expanded filter, classification invalidation, multi-recipe
  path selection, toolbar, setting persistence, save/reload, Harmony summary.
- Result: no matching exception; filter unchanged with 510 definitions. Instance,
  workstation, research, and multi-path probes completed. The diagnostic cache
  measured 0 ms cold and warm. Despite VEF's large patch inventory, no runtime
  collision was observed on the exercised surface.
- Evidence:
  `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\TSCompatVFE-e71478107e744da48077927a732595eb`.
- Limitation: an actual VFE production building and each VFE recipe family were
  not exercised, and this was not the requested full Vanilla Expanded stack.

### Rimefeller — inconclusive for custom production; ordinary surface stable

- External: Rimefeller, `Dubwise.Rimefeller`, Workshop `1321849735`, version
  `1.2.1634`, snapshot `2025-07-13T22:48:10.4412432Z`.
- Load order: `Core > Harmony > Agent > Spine > TechSense > Rimefeller`.
- Scenario: content-expanded filter, classification invalidation and multi-path
  recipe probes, toolbar, setting persistence, save/reload, Harmony summary.
- Result: no matching exception; filter unchanged with 489 definitions. The
  ordinary recipe probes completed and the diagnostic cache measured 1 ms cold,
  0 ms warm. Harmony reported only five Rimefeller-owned patches and no observed
  conflict with TechSense's rendering hooks.
- Evidence:
  `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\TSCompatRimefeller-5f09c48900bf4e1f918cbfcd8e1b5501`.
- Required follow-up: build and operate the oil chain, then compare the status
  for custom outputs against real production. Only after that reproduction can
  the team decide whether a provider belongs in Rimefeller-facing optional
  compatibility code. No speculative provider is justified by this lane.

## Findings and smallest defensible responses

1. **Confirmed compatible:** TechSense alone and the exercised Better Workbench
   Management surfaces, including both reasonable load orders.
2. **Compatible with documented limitation:** ASF framework, Dubs Mint Menus,
   Research Reinvented, and VEF/VFE on the exact surfaces above. Keep the
   limitation in release notes until their named interactive surfaces are run.
3. **Inconclusive/integration candidate:** Rimefeller custom oil production.
   Gather an operational-chain reproduction before writing a provider. Such a
   provider is domain-specific and should not go in Spine.
4. **Packaging limitation:** TechSense logs a metadata warning because the Spine
   dependency has no download or Workshop URL. This is not a runtime conflict,
   but the release package should give users a resolvable Spine location.
5. **Performance:** cache probes over 467–510 definitions were 0–1 ms cold and
   0 ms warm. Concurrent whole-game TPS samples varied too widely to support a
   comparative claim, so performance compatibility remains unproven rather than
   inferred from noisy data.
6. **No patch is justified from these findings.** No compatibility code or test
   was changed.

## Requested targets not tested in this bounded pass

Filter Manager; LWM's Deep Storage; Stockpile Stack Limit/current continuation;
Stockpile and Ingredient Filters; More Filters; RimFridge/current maintained
fridge; Better Pawn Control; Project RimFactory Revived; Dubs Rimatomics;
Medieval Overhaul; Combat Extended; Vanilla Furniture Expanded - Factory;
Vanilla Furniture Expanded - Production as a distinct package; the requested
large Vanilla Expanded stack; ResearchPowl; Semi Random Research; a currently
maintained research-tree replacement; two ordinary colonies; a temporary or
pocket map; Multiplayer/policy contexts; concrete ASF storage content; both
load orders for every pair; removal from each copied pair save; alternate UI
scales/resolutions; and thirty-minute accelerated simulation.

## Grouped full-DLC coverage checklist

The shared full-DLC lane must use the isolated Steam/full-DLC runtime only for
these DLC-dependent assertions and record its exact build and DLC set. Ordinary
Core pairwise conclusions remain tied to the H-drive runtime. The grouped lane
must verify: (1) two ordinary colonies with different capabilities;
(2) capability on one map never changes the other map's indicator; (3) two
instances of the same workstation with only one powered, fueled, connected, or
otherwise usable; (4) Odyssey gravship transition and destination map; (5) a
temporary/pocket map if a maintained provider is available; (6) an SOS2 ship map
if practical; (7) colored-square selection only on the currently displayed map;
(8) Architect navigation preserves camera position; (9) research navigation is
game-global without leaking map capability; (10) save/reload and return through
each transition. These are pending grouped coverage, not permanent unsupported
cases.

## Evidence hygiene

Four earlier `TSCompatCurrent*` launches targeted the live D-drive game root,
where a duplicate pre-existing RimWorld Agent invalidated isolation. They are
invalid-environment evidence only. They were terminated and are excluded from
every compatibility conclusion above. The later clean staged full-DLC root is
a separate grouped-test environment and does not rehabilitate those launches.
