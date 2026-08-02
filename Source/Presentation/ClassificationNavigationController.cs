using System;
using System.Collections.Generic;
using RimWorld;
using FilterSignals.Domain;
using FilterSignals.Runtime;
using Verse;

namespace FilterSignals.Presentation
{
    internal static class ClassificationNavigationController
    {
        internal static bool TryNavigate(
            ClassificationNavigationTarget target)
        {
            try
            {
                if (target == null || !target.IsActionable)
                {
                    return false;
                }

                switch (target.Decision.Kind)
                {
                    case ProductionNavigationKind.SelectProductionSource:
                        return SelectProductionSource(target);
                    case ProductionNavigationKind.OpenResearch:
                        return OpenResearch(target);
                    case ProductionNavigationKind.SelectBuildOption:
                        return SelectBuildOption(target);
                    default:
                        return false;
                }
            }
            catch (Exception exception)
            {
                Log.ErrorOnce(
                    "[Filter Signals] Navigation failed safely without " +
                    "changing the filter: " + exception,
                    207481903);
                return false;
            }
        }

        private static bool SelectProductionSource(
            ClassificationNavigationTarget target)
        {
            Building source = target.ProductionSource;
            if (source == null ||
                !source.Spawned ||
                source.Map == null)
            {
                return false;
            }

            CameraJumper.TryJumpAndSelect(source);
            return true;
        }

        private static bool OpenResearch(
            ClassificationNavigationTarget target)
        {
            if (target.Research == null ||
                Find.MainTabsRoot == null ||
                MainButtonDefOf.Research?.TabWindow
                    is not MainTabWindow_Research researchWindow)
            {
                return false;
            }

            Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
            researchWindow.Select(target.Research);
            return true;
        }

        private static bool SelectBuildOption(
            ClassificationNavigationTarget target)
        {
            if (target.Buildable == null ||
                target.Map == null ||
                Find.MainTabsRoot == null ||
                MainButtonDefOf.Architect == null ||
                Find.CurrentMap != target.Map)
            {
                return false;
            }

            Designator_Build designator =
                FindBuildDesignator(target.Buildable);
            if (designator == null)
            {
                return false;
            }

            Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Architect);
            Find.DesignatorManager.Select(designator);
            return true;
        }

        private static Designator_Build FindBuildDesignator(
            BuildableDef target)
        {
            DesignationCategoryDef category =
                target?.designationCategory;
            if (category == null || !category.Visible)
            {
                return null;
            }

            foreach (Designator designator in
                category.AllResolvedAndIdeoDesignators)
            {
                Designator_Build match =
                    FindBuildDesignator(designator, target);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static Designator_Build FindBuildDesignator(
            Designator designator,
            BuildableDef target)
        {
            if (designator == null || !designator.Visible)
            {
                return null;
            }

            if (designator is Designator_Build build &&
                build.PlacingDef == target)
            {
                return build;
            }

            if (designator is Designator_Dropdown dropdown)
            {
                IReadOnlyList<Designator> elements = dropdown.Elements;
                for (int index = 0; index < elements.Count; index++)
                {
                    Designator_Build match =
                        FindBuildDesignator(elements[index], target);
                    if (match != null)
                    {
                        return match;
                    }
                }
            }

            return null;
        }
    }
}
