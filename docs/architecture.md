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

The colored per-item square is the default stockpile interaction. The
classification toolbar is an opt-in setting, disabled by default; when hidden,
the controller reserves no vertical space and `Visible(ThingDef)` remains
vanilla-authoritative.

`ToolbarLayout` is an engine-free responsive layout policy. It keeps the title
and four buttons on one row only while every button retains at least 72 pixels.
At ordinary narrow filter widths it reserves a title row and a readable
two-column button grid; exceptionally narrow widths fall back to one column.
The calculated height is removed from the vanilla filter content rectangle, so
the toolbar never overlays the search, hit-point, or quality controls.

The status square is an additive navigation affordance and never writes to a
`ThingFilter`. `ClassificationNavigationResolver` considers vanilla recipe
paths only and uses the same map-specific recipe assessment as classification.
It deterministically chooses recipe and source definitions by ordinal
definition name and production-building instances by thing ID. A click selects
a usable existing source, opens and selects missing research, or activates an
available build designator for the workstation or its first missing building
prerequisite. Architect selection is allowed only for the currently displayed
map and never moves the camera. Custom production paths, stale objects, hidden
designators, and other ambiguous states do not advertise a click action; if
clicked, they leave the storage panel open and unchanged.
Hover text contains only the classification, a short reason, and an action
instruction when a real navigation target exists. A fully satisfied path uses
the single status line "Able to be produced in this colony" rather than
repeating its research or workstation state. Because vanilla registers a
tooltip for the entire item row first, the square clears current-frame tooltips
only while the pointer is inside its 18-by-18 interaction rectangle, then
registers the Filter Signals tooltip. This gives the square one unambiguous tooltip
while preserving the vanilla item description everywhere else on the row.
For small-volume materials such as gold and silver, the row patch temporarily
adds the current definition to RimWorld's own suppression list while vanilla
draws that row, then restores the list in both normal and exceptional paths.
This removes the inline `/10` notation without persisting or changing filter
state. The square tooltip carries the concise equivalent, “Small-volume: 10
units = 1.”

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
`EvaluateOverrides`, `AddCustomPaths`, and `StableHash` in
`ClassificationService`, plus `MaterialsAvailable`, `BuildLockedReason`, and
`BuildUnavailableReason` in `RecipeAssessmentFactory`. They are kept private
because each isolates one policy or failure boundary; inlining them would mix
compatibility execution, RimWorld queries, explanation composition, and
log-key generation into the hot classification method. The recommended
architectural action is to keep them local and not create a shared utility
until a second real consumer appears.

`CapabilityInvalidation.InvalidateIfProductionSource` has multiple patch
callers and centralizes the rule that unrelated buildings must not flush the
cache.

`ProductionSourceSelector.Select` has one production caller,
`MapCapabilitySnapshot.SelectSource`, plus focused unit-test callers. It remains
an internal domain policy because separating the pure "any actual instance may
win" reduction from RimWorld objects is what makes the conditional
`RecipeWorker` regression testable. The recommended action is to keep it local
to Filter Signals; do not promote it to Spine or shared tooling unless another
production consumer needs the same policy.

`MapCapabilitySnapshot.RecipeAvailableOnInstance` and its local `StableHash`
helper each have one direct caller. They isolate the modded-code exception
boundary and stable `ErrorOnce` key from source enumeration. They should remain
private; moving a one-consumer fault boundary into shared infrastructure would
not improve reuse.

The follow-up adds several intentional one-caller seams:

- `LayoutRect.Overlaps` is pure layout-test geometry, while
  `ToolbarLayout.IsFinite` protects its single public calculation boundary.
- `ClassificationService.ProductionIndex` and
  `MapCapabilitySnapshot.FindUsableSource` expose the minimum read-only state
  needed by the click-only navigation resolver.
- The collection-level `ResolveBuildRequirement` overload and
  `MissingBuildResearch` isolate deterministic prerequisite reduction.
- `SelectProductionSource`, `OpenResearch`, `SelectBuildOption`, and the outer
  `FindBuildDesignator` method isolate three distinct RimWorld UI actions and
  their failure checks.
- `ClassificationPresentation.NavigationTooltip` keeps navigation wording out
  of row drawing.

Recommendation: keep these methods local. They each guard an engine boundary
or a testable policy seam; promoting them to Spine or a general helper would
add coupling without a second production consumer.

The four `Run*Probe` diagnostics methods each have one caller in the aggregate
capability debug action. They remain separate because each produces a
standalone runtime assertion for conditional instances, workstation
invalidation, research invalidation, or multi-path definitions. They are
developer-only acceptance probes, not reusable runtime services; the
recommended action is to keep them private and local. `TrySpawnSource` is
shared by the conditional-instance and workstation probes.

The small-volume diagnostic action reuses the ordinary filter fixture and only
seeds its vanilla quick-search text with `Gold`. This gives automated lanes a
deterministic row for tooltip and layout captures without adding a gameplay
code path or a separate imitation of RimWorld's filter UI.
