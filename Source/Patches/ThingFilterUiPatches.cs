using System;
using System.Collections.Generic;
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
            ThingDef tDef,
            ref List<ThingDef> ___suppressSmallVolumeTags,
            out RowPatchState __state)
        {
            __state = new RowPatchState
            {
                RowY = __instance.CurHeight
            };
            if (tDef == null || !tDef.IsStuff || !tDef.smallVolume)
            {
                return;
            }

            if (___suppressSmallVolumeTags == null)
            {
                ___suppressSmallVolumeTags = new List<ThingDef>();
                __state.CreatedSuppressionList = true;
            }

            __state.SuppressionList = ___suppressSmallVolumeTags;
            if (!___suppressSmallVolumeTags.Contains(tDef))
            {
                ___suppressSmallVolumeTags.Add(tDef);
                __state.AddedSuppression = true;
            }
        }

        private static void Postfix(
            Listing_TreeThingFilter __instance,
            ThingDef tDef,
            Map map,
            ref List<ThingDef> ___suppressSmallVolumeTags,
            RowPatchState __state)
        {
            FilterUiController.DrawIndicator(
                __instance,
                tDef,
                map,
                __state.RowY);
            RestoreSmallVolumeSuppression(
                tDef,
                ref ___suppressSmallVolumeTags,
                __state);
        }

        private static Exception Finalizer(
            Exception __exception,
            ThingDef tDef,
            ref List<ThingDef> ___suppressSmallVolumeTags,
            RowPatchState __state)
        {
            RestoreSmallVolumeSuppression(
                tDef,
                ref ___suppressSmallVolumeTags,
                __state);
            return __exception;
        }

        private static void RestoreSmallVolumeSuppression(
            ThingDef tDef,
            ref List<ThingDef> currentSuppressions,
            RowPatchState state)
        {
            if (state.AddedSuppression)
            {
                state.SuppressionList?.Remove(tDef);
            }

            if (state.CreatedSuppressionList &&
                ReferenceEquals(currentSuppressions, state.SuppressionList))
            {
                currentSuppressions = null;
            }
        }

        private struct RowPatchState
        {
            internal float RowY;
            internal List<ThingDef> SuppressionList;
            internal bool CreatedSuppressionList;
            internal bool AddedSuppression;
        }
    }
}
