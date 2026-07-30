using HarmonyLib;
using RimWorld;
using TechSenseFilters.Runtime;
using Verse;

namespace TechSenseFilters.Patches
{
    [HarmonyPatch(
        typeof(ResearchManager),
        nameof(ResearchManager.FinishProject))]
    internal static class ResearchFinishedPatch
    {
        private static void Postfix()
        {
            ClassificationService.InvalidateAll();
        }
    }

    [HarmonyPatch(
        typeof(Map),
        nameof(Map.Dispose))]
    internal static class MapDisposedPatch
    {
        private static void Postfix(Map __instance)
        {
            ClassificationService.Release(__instance);
        }
    }

    [HarmonyPatch(
        typeof(Building),
        nameof(Building.SpawnSetup))]
    internal static class ProductionBuildingSpawnedPatch
    {
        private static void Postfix(Building __instance, Map map)
        {
            CapabilityInvalidation.InvalidateIfProductionSource(
                __instance,
                map);
        }
    }

    [HarmonyPatch(
        typeof(Building),
        nameof(Building.DeSpawn))]
    internal static class ProductionBuildingDespawnedPatch
    {
        private static void Prefix(
            Building __instance,
            out Map __state)
        {
            __state = __instance.Map;
        }

        private static void Postfix(
            Building __instance,
            Map __state)
        {
            CapabilityInvalidation.InvalidateIfProductionSource(
                __instance,
                __state);
        }
    }

    [HarmonyPatch(
        typeof(Pawn),
        nameof(Pawn.SpawnSetup))]
    internal static class ColonyPawnSpawnedPatch
    {
        private static void Postfix(Pawn __instance, Map map)
        {
            if (__instance.Faction == Faction.OfPlayer)
            {
                ClassificationService.Invalidate(map);
            }
        }
    }

    [HarmonyPatch(
        typeof(Pawn),
        nameof(Pawn.DeSpawn))]
    internal static class ColonyPawnDespawnedPatch
    {
        private static void Prefix(Pawn __instance, out Map __state)
        {
            __state = __instance.Map;
        }

        private static void Postfix(Pawn __instance, Map __state)
        {
            if (__instance.Faction == Faction.OfPlayer)
            {
                ClassificationService.Invalidate(__state);
            }
        }
    }

    [HarmonyPatch(
        typeof(CompPowerTrader),
        nameof(CompPowerTrader.PowerOn),
        MethodType.Setter)]
    internal static class ProductionSourcePowerChangedPatch
    {
        private static void Postfix(CompPowerTrader __instance)
        {
            CapabilityInvalidation.InvalidateIfProductionSource(
                __instance.parent);
        }
    }

    [HarmonyPatch(
        typeof(CompBreakdownable),
        nameof(CompBreakdownable.DoBreakdown))]
    internal static class ProductionSourceBrokeDownPatch
    {
        private static void Postfix(CompBreakdownable __instance)
        {
            CapabilityInvalidation.InvalidateIfProductionSource(
                __instance.parent);
        }
    }

    [HarmonyPatch(
        typeof(CompBreakdownable),
        nameof(CompBreakdownable.Notify_Repaired))]
    internal static class ProductionSourceRepairedPatch
    {
        private static void Postfix(CompBreakdownable __instance)
        {
            CapabilityInvalidation.InvalidateIfProductionSource(
                __instance.parent);
        }
    }

    [HarmonyPatch(
        typeof(CompRefuelable),
        nameof(CompRefuelable.ConsumeFuel))]
    internal static class ProductionSourceConsumedFuelPatch
    {
        private static void Prefix(
            CompRefuelable __instance,
            out bool __state)
        {
            __state = __instance.HasFuel;
        }

        private static void Postfix(
            CompRefuelable __instance,
            bool __state)
        {
            if (__state != __instance.HasFuel)
            {
                CapabilityInvalidation.InvalidateIfProductionSource(
                    __instance.parent);
            }
        }
    }

    [HarmonyPatch(
        typeof(CompRefuelable),
        nameof(CompRefuelable.Refuel),
        new[] { typeof(float) })]
    internal static class ProductionSourceRefueledPatch
    {
        private static void Prefix(
            CompRefuelable __instance,
            out bool __state)
        {
            __state = __instance.HasFuel;
        }

        private static void Postfix(
            CompRefuelable __instance,
            bool __state)
        {
            if (__state != __instance.HasFuel)
            {
                CapabilityInvalidation.InvalidateIfProductionSource(
                    __instance.parent);
            }
        }
    }

    internal static class CapabilityInvalidation
    {
        internal static void InvalidateIfProductionSource(
            Thing thing,
            Map map = null)
        {
            if (thing != null &&
                ClassificationService.IsProductionSource(thing.def))
            {
                ClassificationService.Invalidate(map ?? thing.Map);
            }
        }
    }
}
