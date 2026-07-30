using System;
using TechSenseFilters.Domain;

namespace TechSenseFilters.Tests
{
    internal static class Program
    {
        private static int passed;

        private static int Main()
        {
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

            Console.WriteLine("PASS: " + passed + " TechSense domain tests");
            return 0;
        }

        private static void NoPathsAreNotApplicable()
        {
            ClassificationResult result =
                ProductionClassifier.Classify(
                    Array.Empty<ProductionPathAssessment>());
            Equal(
                ProductionClassification.NotApplicable,
                result.Classification);
            Contains("No player production recipe", result.Explanation);
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

        private static void Run(string name, Action test)
        {
            test();
            passed++;
            Console.WriteLine("PASS: " + name);
        }

        private static void Equal<T>(T expected, T actual)
        {
            if (!Equals(expected, actual))
            {
                throw new InvalidOperationException(
                    "Expected '" + expected + "' but received '" +
                    actual + "'.");
            }
        }

        private static void Contains(string expected, string actual)
        {
            if (actual == null ||
                actual.IndexOf(
                    expected,
                    StringComparison.OrdinalIgnoreCase) < 0)
            {
                throw new InvalidOperationException(
                    "Expected text containing '" + expected +
                    "' but received '" + actual + "'.");
            }
        }
    }
}
