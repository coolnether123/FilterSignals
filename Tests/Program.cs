using System;
using System.Reflection;
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
                "structured classification causes are propagated",
                StructuredClassificationCausesArePropagated);
            Run(
                "explicit override wins",
                ExplicitOverrideWins);
            Run(
                "override result is isolated from provider paths",
                OverrideResultIsIsolatedFromProviderPaths);
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
                "long localized title uses a readable stacked layout",
                LongLocalizedTitleUsesReadableStackedLayout);
            Run(
                "inline title reserves measured width",
                InlineTitleReservesMeasuredWidth);
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
            Run(
                "navigation follows the winning path and cause",
                NavigationFollowsWinningPathAndCause);
            Run(
                "production assessment constructors preserve compatibility",
                LegacyProductionAssessmentCallsRemainCompatible);
            Run(
                "ambiguous winning navigation paths are rejected",
                AmbiguousWinningNavigationPathsAreRejected);
            Run(
                "material shortage keeps its public enum value",
                MaterialShortageKeepsPublicEnumValue);

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

        private static void OverrideResultIsIsolatedFromProviderPaths()
        {
            ClassificationResult overrideResult =
                new ClassificationResult(
                    ProductionClassification.NotApplicable,
                    "Handled by an integration override.");
            ClassificationResult result = ProductionClassifier.Classify(
                new[]
                {
                    Path(
                        "provider route",
                        research: true,
                        present: true,
                        usable: true,
                        pawn: true,
                        materials: true,
                        pathId: "Provider.Route")
                },
                overrideResult);

            if (!ReferenceEquals(overrideResult, result))
            {
                throw new InvalidOperationException(
                    "An override must isolate the provider path result.");
            }

            Equal(ProductionClassification.NotApplicable, result.Classification);
            Equal(string.Empty, result.PathId);
        }

        private static void StructuredClassificationCausesArePropagated()
        {
            ClassificationResult materials = ProductionClassifier.Classify(
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
                ClassificationReason.MaterialShortage,
                materials.Reason);

            ClassificationResult pawn = ProductionClassifier.Classify(
                new[]
                {
                    Path(
                        "crafting spot",
                        research: true,
                        present: true,
                        usable: true,
                        pawn: false,
                        materials: true,
                        reason: ClassificationReason.NoCapableColonist)
                });
            Equal(
                ClassificationReason.NoCapableColonist,
                pawn.Reason);

            ClassificationResult source = ProductionClassifier.Classify(
                new[]
                {
                    Path(
                        "fabrication bench",
                        research: true,
                        present: false,
                        usable: false,
                        pawn: true,
                        materials: true,
                        reason: ClassificationReason.MissingProductionSource)
                });
            Equal(
                ClassificationReason.MissingProductionSource,
                source.Reason);
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

        private static void LongLocalizedTitleUsesReadableStackedLayout()
        {
            ToolbarLayoutPlan layout =
                ToolbarLayout.Calculate(500f, 220f);
            Equal(ToolbarLayoutMode.TwoColumn, layout.Mode);
            if (layout.Title.Width < 490f ||
                layout.Title.YMax >= layout.Buttons[0].Y)
            {
                throw new InvalidOperationException(
                    "A long localized title must get a full readable row " +
                    "above the buttons.");
            }

            for (int index = 0;
                index < layout.Buttons.Length;
                index++)
            {
                if (layout.Title.Overlaps(layout.Buttons[index]))
                {
                    throw new InvalidOperationException(
                        "The stacked title overlaps button " + index + ".");
                }
            }
        }

        private static void InlineTitleReservesMeasuredWidth()
        {
            ToolbarLayoutPlan layout =
                ToolbarLayout.Calculate(700f, 120f);
            Equal(ToolbarLayoutMode.Inline, layout.Mode);
            if (layout.Title.Width < 120f ||
                layout.Title.XMax >= layout.Buttons[0].X)
            {
                throw new InvalidOperationException(
                    "An inline title must reserve its measured width and " +
                    "leave a gap before the first button.");
            }

            for (int index = 0;
                index < layout.Buttons.Length;
                index++)
            {
                if (layout.Title.Overlaps(layout.Buttons[index]))
                {
                    throw new InvalidOperationException(
                        "The inline title overlaps button " + index + ".");
                }
            }
        }

        private static void NavigationChoosesStableUsableSource()
        {
                ProductionNavigationDecision decision =
                ProductionNavigationPolicy.Decide(
                    new ClassificationResult(
                        ProductionClassification.CanMakeNow,
                        "ready",
                        pathLabel: "Recipe_Alpha",
                        reason: ClassificationReason.General,
                        pathId: "Recipe_Alpha",
                        isVanillaRecipePath: true),
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
            Equal(1, decision.AlternativeCount);
        }

        private static void NavigationOpensMissingResearch()
        {
                ProductionNavigationDecision decision =
                ProductionNavigationPolicy.Decide(
                    new ClassificationResult(
                        ProductionClassification.CannotMakeYet,
                        "locked",
                        pathLabel: "Recipe_Locked",
                        reason: ClassificationReason.ResearchRequired,
                        pathId: "Recipe_Locked",
                        isVanillaRecipePath: true),
                    new[]
                    {
                        NavigationCandidate(
                            "Recipe_Locked",
                            research: false,
                            researchTarget: "research:Fabrication",
                            reason: ClassificationReason.ResearchRequired)
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
                    new ClassificationResult(
                        ProductionClassification.ResearchUnlocked,
                        "missing source",
                        pathLabel: "Recipe_Ready",
                        reason: ClassificationReason.MissingProductionSource,
                        pathId: "Recipe_Ready",
                        isVanillaRecipePath: true),
                    new[]
                    {
                        NavigationCandidate(
                            "Recipe_ZLater",
                            research: true,
                            sourcePresent: false,
                            researchTarget:
                                "research:AdvancedFabrication",
                            reason: ClassificationReason.MissingProductionSource),
                        NavigationCandidate(
                            "Recipe_Ready",
                            research: true,
                            sourcePresent: false,
                            buildTarget: "build:FabricationBench",
                            reason: ClassificationReason.MissingProductionSource)
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
                    new ClassificationResult(
                        ProductionClassification.ResearchUnlocked,
                        "no pawn",
                        pathLabel: "Recipe_NoPawn",
                        reason: ClassificationReason.NoCapableColonist,
                        pathId: "Recipe_NoPawn",
                        isVanillaRecipePath: true),
                    new[]
                    {
                        NavigationCandidate(
                            "Recipe_NoPawn",
                            research: true,
                            sourcePresent: true,
                            reason: ClassificationReason.NoCapableColonist)
                    });

            Equal(ProductionNavigationKind.None, decision.Kind);
            Equal(false, decision.IsActionable);
        }

        private static void NavigationFollowsWinningPathAndCause()
        {
            ClassificationResult result = ProductionClassifier.Classify(
                new[]
                {
                    Path(
                        "custom route",
                        research: true,
                        present: false,
                        usable: false,
                        pawn: true,
                        materials: true,
                        reason: ClassificationReason.MissingProductionSource,
                        pathId: "Provider.Custom"),
                    Path(
                        "vanilla bench",
                        research: true,
                        present: false,
                        usable: false,
                        pawn: true,
                        materials: true,
                        reason: ClassificationReason.MissingProductionSource,
                        pathId: "Recipe.Vanilla")
                });
            ProductionNavigationDecision decision =
                ProductionNavigationPolicy.Decide(
                    result,
                    new[]
                    {
                        NavigationCandidate(
                            "Provider.Custom",
                            research: true,
                            sourcePresent: false,
                            buildTarget: null,
                            reason: ClassificationReason.MissingProductionSource),
                        NavigationCandidate(
                            "Recipe.Vanilla",
                            research: true,
                            sourcePresent: false,
                            buildTarget: "build:VanillaBench",
                            reason: ClassificationReason.MissingProductionSource)
                    });

            Equal("Provider.Custom", result.PathId);
            Equal(
                ClassificationReason.MissingProductionSource,
                result.Reason);
            Equal(ProductionNavigationKind.None, decision.Kind);

            ClassificationResult reasonChanged =
                new ClassificationResult(
                    ProductionClassification.ResearchUnlocked,
                    "no capable colonist",
                    pathLabel: "Provider.Custom",
                    reason: ClassificationReason.NoCapableColonist,
                    pathId: "Provider.Custom");
            ProductionNavigationDecision mismatchedCause =
                ProductionNavigationPolicy.Decide(
                    reasonChanged,
                    new[]
                    {
                        NavigationCandidate(
                            "Provider.Custom",
                            research: true,
                            sourcePresent: false,
                            buildTarget: "build:WrongCause",
                            reason: ClassificationReason.MissingProductionSource)
                    });
            Equal(ProductionNavigationKind.None, mismatchedCause.Kind);
        }

        private static void LegacyProductionAssessmentCallsRemainCompatible()
        {
            ProductionPathAssessment named =
                new ProductionPathAssessment(
                    "legacy bench",
                    true,
                    true,
                    true,
                    true,
                    true,
                    lockedReason: "locked",
                    unavailableReason: "unavailable");
            Equal("locked", named.LockedReason);
            Equal("unavailable", named.UnavailableReason);

            ProductionPathAssessment positional =
                new ProductionPathAssessment(
                    "legacy bench",
                    true,
                    true,
                    true,
                    true,
                    true,
                    "locked",
                    "unavailable");
            Equal("locked", positional.LockedReason);
            Equal("unavailable", positional.UnavailableReason);

            Type[] legacyParameterTypes =
            {
                typeof(string),
                typeof(bool),
                typeof(bool),
                typeof(bool),
                typeof(bool),
                typeof(bool),
                typeof(string),
                typeof(string)
            };
            ConstructorInfo legacyConstructor =
                typeof(ProductionPathAssessment).GetConstructor(
                    legacyParameterTypes);
            if (legacyConstructor == null)
            {
                throw new InvalidOperationException(
                    "The exact eight-parameter public constructor is missing.");
            }

            ProductionPathAssessment reflected =
                (ProductionPathAssessment)legacyConstructor.Invoke(
                    new object[]
                    {
                        "reflected bench",
                        true,
                        true,
                        true,
                        true,
                        true,
                        "reflected locked",
                        "reflected unavailable"
                    });
            if (reflected.PathLabel != "reflected bench" ||
                reflected.LockedReason != "reflected locked" ||
                reflected.UnavailableReason != "reflected unavailable" ||
                reflected.Reason != ClassificationReason.General ||
                reflected.PathId != string.Empty)
            {
                throw new InvalidOperationException(
                    "The legacy constructor did not preserve its behavior " +
                    "when invoked through ConstructorInfo.");
            }

            Type[] extendedParameterTypes =
            {
                typeof(string),
                typeof(bool),
                typeof(bool),
                typeof(bool),
                typeof(bool),
                typeof(bool),
                typeof(string),
                typeof(string),
                typeof(ClassificationReason),
                typeof(string)
            };
            ConstructorInfo extendedConstructor =
                typeof(ProductionPathAssessment).GetConstructor(
                    extendedParameterTypes);
            if (extendedConstructor == null)
            {
                throw new InvalidOperationException(
                    "The extended production assessment constructor is " +
                    "missing.");
            }

            ProductionPathAssessment extended =
                (ProductionPathAssessment)extendedConstructor.Invoke(
                    new object[]
                    {
                        "extended bench",
                        true,
                        true,
                        true,
                        true,
                        false,
                        "extended locked",
                        "extended unavailable",
                        ClassificationReason.MaterialShortage,
                        "Recipe.Extended"
                    });
            if (extended.Reason != ClassificationReason.MaterialShortage ||
                extended.PathId != "Recipe.Extended" ||
                extended.CanMakeNow)
            {
                throw new InvalidOperationException(
                    "The extended constructor did not preserve metadata or " +
                    "classification behavior.");
            }
        }

        private static void AmbiguousWinningNavigationPathsAreRejected()
        {
            ProductionNavigationDecision decision =
                ProductionNavigationPolicy.Decide(
                    new ClassificationResult(
                        ProductionClassification.CannotMakeYet,
                        "locked",
                        pathLabel: "Recipe_Duplicate",
                        reason: ClassificationReason.ResearchRequired,
                        pathId: "Recipe_Duplicate",
                        isVanillaRecipePath: true),
                    new[]
                    {
                        NavigationCandidate(
                            "Recipe_Duplicate",
                            research: false,
                            researchTarget: "research:One",
                            reason: ClassificationReason.ResearchRequired),
                        NavigationCandidate(
                            "Recipe_Duplicate",
                            research: false,
                            researchTarget: "research:Two",
                            reason: ClassificationReason.ResearchRequired)
                    });

            Equal(ProductionNavigationKind.None, decision.Kind);
            Equal(false, decision.IsActionable);
        }

        private static void MaterialShortageKeepsPublicEnumValue()
        {
            Equal(1, (int)ClassificationReason.MaterialShortage);
        }

        private static ProductionNavigationCandidate NavigationCandidate(
            string pathId,
            bool canMake = false,
            bool research = true,
            bool sourcePresent = true,
            string sourceTarget = null,
            string researchTarget = null,
            string buildTarget = null,
            ClassificationReason reason = ClassificationReason.General)
        {
            return new ProductionNavigationCandidate(
                pathId,
                pathId,
                canMake,
                research,
                sourcePresent,
                sourceTarget,
                researchTarget,
                buildTarget,
                reason);
        }

        private static ProductionPathAssessment Path(
            string label,
            bool research,
            bool present,
            bool usable,
            bool pawn,
            bool materials,
            string locked = null,
            ClassificationReason reason = ClassificationReason.General,
            string pathId = null)
        {
            return new ProductionPathAssessment(
                label,
                research,
                present,
                usable,
                pawn,
                materials,
                locked,
                null,
                reason,
                pathId);
        }

    }
}
