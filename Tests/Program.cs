using System;
using FilterSignals.Domain;
using FilterSignals.Presentation;
using static RimWorld.ModTestSupport.Test;

namespace FilterSignals.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            Start("Filter Signals domain tests");
            Run("no paths are not applicable", NoPathsAreNotApplicable);
            Run("any viable path wins", AnyViablePathWins);
            Run(
                "unlocked missing source is distinct",
                UnlockedMissingSourceIsDistinct);
            Run(
                "locked research is unavailable",
                LockedResearchIsUnavailable);
            Run(
                "unusable source explains itself",
                UnusableSourceExplainsItself);
            Run(
                "pawn capability is required",
                PawnCapabilityIsRequired);
            Run(
                "material shortage is optional input",
                MaterialShortageIsOptionalInput);
            Run(
                "explicit override wins",
                ExplicitOverrideWins);
            Run(
                "conditional recipe availability uses actual instances",
                ConditionalRecipeAvailabilityUsesActualInstances);
            Run(
                "conditional recipe rejection blocks same-def source",
                ConditionalRecipeRejectionBlocksSameDefSource);
            Run(
                "bill giver usability remains required",
                BillGiverUsabilityRemainsRequired);
            Run(
                "narrow toolbar uses readable rows",
                NarrowToolbarUsesReadableRows);
            Run(
                "wide toolbar remains inline",
                WideToolbarRemainsInline);
            Run(
                "navigation chooses a stable usable source",
                NavigationChoosesStableUsableSource);
            Run(
                "navigation opens missing research",
                NavigationOpensMissingResearch);
            Run(
                "navigation selects missing workstation build option",
                NavigationSelectsMissingWorkstationBuildOption);
            Run(
                "navigation fails safely without a target",
                NavigationFailsSafelyWithoutTarget);

            return Finish();
        }

        private static void NoPathsAreNotApplicable()
        {
            ClassificationResult result =
                ProductionClassifier.Classify(
                    Array.Empty<ProductionPathAssessment>());
            Equal(
                ProductionClassification.NotApplicable,
                result.Classification);
            Contains(
                "This colony is unable to make this item",
                result.Explanation);
            NotContains("player", result.Explanation);
        }

        private static void AnyViablePathWins()
        {
            ClassificationResult result = ProductionClassifier.Classify(
                new[]
                {
                    Path(
                        "fabrication bench",
                        research: false,
                        present: false,
                        usable: false,
                        pawn: true,
                        materials: true,
                        locked: "Requires fabrication research."),
                    Path(
                        "machining table",
                        research: true,
                        present: true,
                        usable: true,
                        pawn: true,
                        materials: true)
                });
            Equal(
                ProductionClassification.CanMakeNow,
                result.Classification);
            Equal("machining table", result.PathLabel);
        }

        private static void UnlockedMissingSourceIsDistinct()
        {
            ClassificationResult result = ProductionClassifier.Classify(
                new[]
                {
                    Path(
                        "fabrication bench",
                        research: true,
                        present: false,
                        usable: false,
                        pawn: true,
                        materials: true)
                });
            Equal(
                ProductionClassification.ResearchUnlocked,
                result.Classification);
            Contains("no fabrication bench exists", result.Explanation);
        }

        private static void LockedResearchIsUnavailable()
        {
            ClassificationResult result = ProductionClassifier.Classify(
                new[]
                {
                    Path(
                        "fabrication bench",
                        research: false,
                        present: true,
                        usable: true,
                        pawn: true,
                        materials: true,
                        locked: "Requires fabrication research.")
                });
            Equal(
                ProductionClassification.CannotMakeYet,
                result.Classification);
            Equal(
                "Requires fabrication research.",
                result.Explanation);
        }

        private static void UnusableSourceExplainsItself()
        {
            ClassificationResult result = ProductionClassifier.Classify(
                new[]
                {
                    Path(
                        "machining table",
                        research: true,
                        present: true,
                        usable: false,
                        pawn: true,
                        materials: true)
                });
            Equal(
                ProductionClassification.ResearchUnlocked,
                result.Classification);
            Contains("no usable machining table", result.Explanation);
        }

        private static void PawnCapabilityIsRequired()
        {
            ClassificationResult result = ProductionClassifier.Classify(
                new[]
                {
                    Path(
                        "crafting spot",
                        research: true,
                        present: true,
                        usable: true,
                        pawn: false,
                        materials: true)
                });
            Equal(
                ProductionClassification.ResearchUnlocked,
                result.Classification);
            Contains("no colonist", result.Explanation);
        }

        private static void MaterialShortageIsOptionalInput()
        {
            ClassificationResult result = ProductionClassifier.Classify(
                new[]
                {
                    Path(
                        "electric stove",
                        research: true,
                        present: true,
                        usable: true,
                        pawn: true,
                        materials: false)
                });
            Equal(
                ProductionClassification.ResearchUnlocked,
                result.Classification);
            Contains("materials", result.Explanation);
        }

        private static void ExplicitOverrideWins()
        {
            var classificationOverride = new ClassificationResult(
                ProductionClassification.NotApplicable,
                "Quest reward only.");
            ClassificationResult result = ProductionClassifier.Classify(
                new[]
                {
                    Path(
                        "fabrication bench",
                        research: true,
                        present: true,
                        usable: true,
                        pawn: true,
                        materials: true)
                },
                classificationOverride);
            if (!ReferenceEquals(classificationOverride, result))
            {
                throw new InvalidOperationException(
                    "The exact override result must be preserved.");
            }
        }

        private static void
            ConditionalRecipeAvailabilityUsesActualInstances()
        {
            ProductionSourceSelection selection =
                ProductionSourceSelector.Select(
                    "conditional bench",
                    new[]
                    {
                        new ProductionSourceCandidate(
                            "conditional bench",
                            currentlyUsableForBills: true,
                            recipeAvailableOnInstance: false),
                        new ProductionSourceCandidate(
                            "conditional bench",
                            currentlyUsableForBills: true,
                            recipeAvailableOnInstance: true)
                    });

            Equal(true, selection.SourcePresent);
            Equal(true, selection.BillGiverUsable);
            Equal(true, selection.SourceUsable);
        }

        private static void
            ConditionalRecipeRejectionBlocksSameDefSource()
        {
            ProductionSourceSelection selection =
                ProductionSourceSelector.Select(
                    "conditional bench",
                    new[]
                    {
                        new ProductionSourceCandidate(
                            "conditional bench",
                            currentlyUsableForBills: true,
                            recipeAvailableOnInstance: false)
                    });

            Equal(true, selection.SourcePresent);
            Equal(true, selection.BillGiverUsable);
            Equal(false, selection.SourceUsable);
        }

        private static void BillGiverUsabilityRemainsRequired()
        {
            ProductionSourceSelection selection =
                ProductionSourceSelector.Select(
                    "powered bench",
                    new[]
                    {
                        new ProductionSourceCandidate(
                            "powered bench",
                            currentlyUsableForBills: false,
                            recipeAvailableOnInstance: true)
                    });

            Equal(true, selection.SourcePresent);
            Equal(false, selection.BillGiverUsable);
            Equal(false, selection.SourceUsable);
        }

        private static void NarrowToolbarUsesReadableRows()
        {
            ToolbarLayoutPlan layout =
                ToolbarLayout.Calculate(264f);
            Equal(ToolbarLayoutMode.TwoColumn, layout.Mode);
            Equal(4, layout.Buttons.Length);
            if (layout.Height <= 30f)
            {
                throw new InvalidOperationException(
                    "A narrow toolbar must reserve a second button row.");
            }

            for (int index = 0;
                index < layout.Buttons.Length;
                index++)
            {
                LayoutRect button = layout.Buttons[index];
                if (button.Width < 100f ||
                    button.X < 0f ||
                    button.XMax > 264f ||
                    button.Y < 0f ||
                    button.YMax > layout.Height)
                {
                    throw new InvalidOperationException(
                        "Narrow toolbar button " + index +
                        " is clipped or unreadably small.");
                }

                for (int other = index + 1;
                    other < layout.Buttons.Length;
                    other++)
                {
                    if (button.Overlaps(layout.Buttons[other]))
                    {
                        throw new InvalidOperationException(
                            "Narrow toolbar buttons overlap.");
                    }
                }
            }
        }

        private static void WideToolbarRemainsInline()
        {
            ToolbarLayoutPlan layout =
                ToolbarLayout.Calculate(500f);
            Equal(ToolbarLayoutMode.Inline, layout.Mode);
            Equal(30f, layout.Height);
            for (int index = 0;
                index < layout.Buttons.Length;
                index++)
            {
                if (layout.Buttons[index].Width < 72f)
                {
                    throw new InvalidOperationException(
                        "Wide toolbar button is below its readable width.");
                }
            }
        }

        private static void NavigationChoosesStableUsableSource()
        {
            ProductionNavigationDecision decision =
                ProductionNavigationPolicy.Decide(
                    ProductionClassification.CanMakeNow,
                    new[]
                    {
                        NavigationCandidate(
                            "Recipe_Beta",
                            canMake: true,
                            sourceTarget: "source:20"),
                        NavigationCandidate(
                            "Recipe_Alpha",
                            canMake: true,
                            sourceTarget: "source:10")
                    });

            Equal(
                ProductionNavigationKind.SelectProductionSource,
                decision.Kind);
            Equal("source:10", decision.TargetId);
            Equal("Recipe_Alpha", decision.PathId);
            Equal(2, decision.AlternativeCount);
        }

        private static void NavigationOpensMissingResearch()
        {
            ProductionNavigationDecision decision =
                ProductionNavigationPolicy.Decide(
                    ProductionClassification.CannotMakeYet,
                    new[]
                    {
                        NavigationCandidate(
                            "Recipe_Locked",
                            research: false,
                            researchTarget: "research:Fabrication")
                    });

            Equal(
                ProductionNavigationKind.OpenResearch,
                decision.Kind);
            Equal("research:Fabrication", decision.TargetId);
        }

        private static void
            NavigationSelectsMissingWorkstationBuildOption()
        {
            ProductionNavigationDecision decision =
                ProductionNavigationPolicy.Decide(
                    ProductionClassification.ResearchUnlocked,
                    new[]
                    {
                        NavigationCandidate(
                            "Recipe_ZLater",
                            research: true,
                            sourcePresent: false,
                            researchTarget:
                                "research:AdvancedFabrication"),
                        NavigationCandidate(
                            "Recipe_Ready",
                            research: true,
                            sourcePresent: false,
                            buildTarget: "build:FabricationBench")
                    });

            Equal(
                ProductionNavigationKind.SelectBuildOption,
                decision.Kind);
            Equal("build:FabricationBench", decision.TargetId);
        }

        private static void NavigationFailsSafelyWithoutTarget()
        {
            ProductionNavigationDecision decision =
                ProductionNavigationPolicy.Decide(
                    ProductionClassification.ResearchUnlocked,
                    new[]
                    {
                        NavigationCandidate(
                            "Recipe_NoPawn",
                            research: true,
                            sourcePresent: true)
                    });

            Equal(ProductionNavigationKind.None, decision.Kind);
            Equal(false, decision.IsActionable);
        }

        private static ProductionNavigationCandidate NavigationCandidate(
            string pathId,
            bool canMake = false,
            bool research = true,
            bool sourcePresent = true,
            string sourceTarget = null,
            string researchTarget = null,
            string buildTarget = null)
        {
            return new ProductionNavigationCandidate(
                pathId,
                pathId,
                canMake,
                research,
                sourcePresent,
                sourceTarget,
                researchTarget,
                buildTarget);
        }

        private static ProductionPathAssessment Path(
            string label,
            bool research,
            bool present,
            bool usable,
            bool pawn,
            bool materials,
            string locked = null)
        {
            return new ProductionPathAssessment(
                label,
                research,
                present,
                usable,
                pawn,
                materials,
                locked);
        }

    }
}
