# Compatibility API

Filter Signals automatically understands production represented by resolved
`RecipeDef` data. Framework authors only need this API when their production
path is not represented that way.

## Add production paths

Implement `IFilterSignalsProductionProvider` and register one instance:

```csharp
FilterSignalsApi.RegisterProductionProvider(myProvider);
```

The provider receives the item, current map, and the player's material-shortage
setting. Return zero or more `ProductionPathAssessment` objects. A provider must
be deterministic for the current map revision and must not mutate game state.

## Override a classification

For definitions whose semantics cannot be expressed as production paths,
implement `IFilterSignalsClassificationOverride`:

```csharp
FilterSignalsApi.RegisterClassificationOverride(myOverride);
```

Return `false` to allow the normal recipe/providers pipeline to continue.
Return `true` with a non-null `ClassificationResult` to supply the final result.
Use this sparingly: a production provider composes with other mods and is
usually the better integration.

## Cache invalidation

Filter Signals handles vanilla research, building usability, pawn presence, and map
lifecycle events. A framework whose custom state changes classification should
call:

```csharp
FilterSignalsApi.Invalidate(map);
// Or, only when a global rule changed:
FilterSignalsApi.InvalidateAll();
```

IDs are compared ordinally, must be nonempty, and must be unique within their
provider kind. Execution order is deterministic by ID. A throwing integration
is logged once and isolated from every other classification path.
