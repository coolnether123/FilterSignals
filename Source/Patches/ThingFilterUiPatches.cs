using System;
using System.Reflection;
using HarmonyLib;
using TechSenseFilters.Presentation;
using UnityEngine;
using Verse;

namespace TechSenseFilters.Patches
{
    [HarmonyPatch(
        typeof(ThingFilterUI),
        nameof(ThingFilterUI.DoThingFilterConfigWindow))]
    internal static class ThingFilterWindowPatch
    {
        private static void Prefix(
            ref Rect rect,
            ThingFilterUI.UIState state,
            Map map)
        {
            FilterUiController.BeginAndDraw(ref rect, state, map);
        }

        private static Exception Finalizer(Exception __exception)
        {
            FilterUiController.End();
            return __exception;
        }
    }

    [HarmonyPatch]
    internal static class ThingFilterVisibilityPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(Listing_TreeThingFilter),
                "Visible",
                new[] { typeof(ThingDef) });
        }

        private static void Postfix(
            ThingDef td,
            ref bool __result)
        {
            if (__result)
            {
                __result = FilterUiController.ShouldShow(td);
            }
        }
    }

    [HarmonyPatch]
    internal static class ThingFilterRowPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(Listing_TreeThingFilter),
                "DoThingDef",
                new[]
                {
                    typeof(ThingDef),
                    typeof(int),
                    typeof(Map)
                });
        }

        private static void Prefix(
            Listing_TreeThingFilter __instance,
            out float __state)
        {
            __state = __instance.CurHeight;
        }

        private static void Postfix(
            Listing_TreeThingFilter __instance,
            ThingDef tDef,
            Map map,
            float __state)
        {
            FilterUiController.DrawIndicator(
                __instance,
                tDef,
                map,
                __state);
        }
    }
}
