using System;
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
                source.Map == null ||
                target.Map == null ||
                Find.CurrentMap != source.Map ||
                Find.CurrentMap != target.Map)
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
                target.Research.IsFinished ||
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
                target.BuildDesignator == null ||
                target.Map == null ||
                Find.MainTabsRoot == null ||
                MainButtonDefOf.Architect == null ||
                Find.CurrentMap != target.Map)
            {
                return false;
            }

            if (!target.BuildDesignator.Visible ||
                target.BuildDesignator.PlacingDef != target.Buildable)
            {
                return false;
            }

            Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Architect);
            Find.DesignatorManager.Select(target.BuildDesignator);
            return true;
        }
    }
}
