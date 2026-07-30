using System;
using System.Collections.Generic;
using System.Linq;
using TechSenseFilters.Runtime;
using Verse;

namespace TechSenseFilters.Compatibility
{
    public static class TechSenseApi
    {
        private static readonly List<ITechSenseProductionProvider>
            ProductionProviders = new List<ITechSenseProductionProvider>();
        private static readonly List<ITechSenseClassificationOverride>
            ClassificationOverrides =
                new List<ITechSenseClassificationOverride>();

        public static void RegisterProductionProvider(
            ITechSenseProductionProvider provider)
        {
            Register(
                provider,
                ProductionProviders,
                item => item.Id,
                "production provider");
        }

        public static void RegisterClassificationOverride(
            ITechSenseClassificationOverride classificationOverride)
        {
            Register(
                classificationOverride,
                ClassificationOverrides,
                item => item.Id,
                "classification override");
        }

        public static void InvalidateAll()
        {
            ClassificationService.InvalidateAll();
        }

        public static void Invalidate(Map map)
        {
            ClassificationService.Invalidate(map);
        }

        internal static IReadOnlyList<ITechSenseProductionProvider>
            GetProductionProviders()
        {
            return ProductionProviders
                .OrderBy(provider => provider.Id, StringComparer.Ordinal)
                .ToArray();
        }

        internal static IReadOnlyList<ITechSenseClassificationOverride>
            GetClassificationOverrides()
        {
            return ClassificationOverrides
                .OrderBy(item => item.Id, StringComparer.Ordinal)
                .ToArray();
        }

        private static void Register<T>(
            T item,
            List<T> items,
            Func<T, string> getId,
            string kind)
            where T : class
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            string id = getId(item);
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "TechSense " + kind + " ID is required.",
                    nameof(item));
            }

            if (items.Any(existing => string.Equals(
                getId(existing),
                id,
                StringComparison.Ordinal)))
            {
                throw new InvalidOperationException(
                    "Duplicate TechSense " + kind + " ID: " + id);
            }

            items.Add(item);
            ClassificationService.InvalidateAll();
        }
    }
}
