using HarmonyLib;
using FilterSignals.Runtime;
using RimWorld;
using Verse;

namespace FilterSignals.Patches
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

#if FILTER_SIGNALS_HAS_MAP_DISPOSE
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
#endif

}
