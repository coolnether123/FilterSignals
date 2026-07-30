# Architecture

## Data flow

`DefinitionProductionIndex` builds a definition-only recipe/product/source index
after RimWorld has loaded definitions. `MapCapabilitySnapshot` then captures the
small amount of colony state needed to evaluate those paths: every actual
production-building instance plus eligible player colonists and colony mechs.
Per-recipe evaluation requires one instance to pass both bill-giver usability
and `RecipeDef.AvailableOnNow(instance)`. Buildings are grouped for lookup but
never reduced to one definition-level usability Boolean.

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

`ProductionSourceSelector.Select` has one production caller,
`MapCapabilitySnapshot.SelectSource`, plus focused unit-test callers. It remains
an internal domain policy because separating the pure "any actual instance may
win" reduction from RimWorld objects is what makes the conditional
`RecipeWorker` regression testable. The recommended action is to keep it local
to TechSense; do not promote it to Spine or shared tooling unless another
production consumer needs the same policy.

`MapCapabilitySnapshot.RecipeAvailableOnInstance` and its local `StableHash`
helper each have one direct caller. They isolate the modded-code exception
boundary and stable `ErrorOnce` key from source enumeration. They should remain
private; moving a one-consumer fault boundary into shared infrastructure would
not improve reuse.

The four `Run*Probe` diagnostics methods each have one caller in the aggregate
capability debug action. They remain separate because each produces a
standalone runtime assertion for conditional instances, workstation
invalidation, research invalidation, or multi-path definitions. They are
developer-only acceptance probes, not reusable runtime services; the
recommended action is to keep them private and local. `TrySpawnSource` is
shared by the conditional-instance and workstation probes.
