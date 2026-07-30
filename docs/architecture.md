# Architecture

## Data flow

`DefinitionProductionIndex` builds a definition-only recipe/product/source index
after RimWorld has loaded definitions. `MapCapabilitySnapshot` then captures the
small amount of colony state needed to evaluate those paths: present and usable
production buildings plus eligible player colonists and colony mechs.

`ClassificationService` combines that data with registered compatibility
providers, reduces all paths through the pure `ProductionClassifier`, and
caches final results. A viable path wins when an item has multiple recipes.
Each map owns a 256 KiB bounded LRU result cache. Snapshots expire after 120
game ticks as a safety net, while research, relevant building state, pawn
presence, power, fuel, breakdowns, and map disposal trigger targeted
invalidation.

Presentation is separate from classification. `FilterUiController` stores
per-dialog presentation state in a `ConditionalWeakTable<ThingFilterUI.UIState,
FilterPresentationState>`. Its toolbar only affects the return value of
`Listing_TreeThingFilter.Visible`; no `ThingFilter` setter or allowance-copy
method is called. Closing the dialog releases that transient relationship
naturally.

Settings and patch installation use the standalone Spine dependency:

- `Spine.UI.SettingsFramework` provides settings scribing, hierarchy, and list
  drawing.
- `Spine.Harmony.HarmonyUtil` installs the assembly patches and reports skipped
  or failed patches.

The Harmony option `AllowStructReturns` is enabled because the guarded patch
set contains the value-returning `Visible(ThingDef)` method. That postfix
amends a Boolean result only; it does not copy or rewrite a struct payload.

## Classification boundary

The pure domain layer has no RimWorld dependency and accepts a list of
`ProductionPathAssessment` values. RimWorld-specific recipe, research,
workstation, pawn, and inventory inspection remains in the runtime layer.
Public compatibility interfaces expose the same domain objects rather than
internal cache or UI types.

Provider order is deterministic by ordinal ID. Duplicate IDs fail fast.
Provider exceptions are isolated, logged once, and do not prevent vanilla or
other provider paths from being classified.

## Single-caller helper audit

Several private methods currently have one direct caller:
`EvaluateOverrides`, `AddCustomPaths`, `MaterialsAvailable`,
`BuildLockedReason`, `BuildUnavailableReason`, and `StableHash`. They are kept
private because each isolates one policy or failure boundary inside
`ClassificationService`; inlining them would mix compatibility execution,
RimWorld queries, explanation composition, and log-key generation into the hot
classification method. The recommended architectural action is to keep them
local and not create a shared utility until a second real consumer appears.

`CapabilityInvalidation.InvalidateIfProductionSource` has multiple patch
callers and centralizes the rule that unrelated buildings must not flush the
cache.
