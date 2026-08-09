using System;
using System.Collections.Generic;
using System.Linq;
using FilterSignals.Runtime;
using Verse;

namespace FilterSignals.Compatibility
{
    /// <summary>
    /// Provides the narrow extension boundary for production systems that
    /// cannot describe themselves with ordinary RimWorld recipes.
    /// </summary>
    public static class FilterSignalsApi
    {
        private static readonly List<IFilterSignalsProductionProvider>
            ProductionProviders = new List<IFilterSignalsProductionProvider>();
        private static readonly List<IFilterSignalsClassificationOverride>
            ClassificationOverrides =
                new List<IFilterSignalsClassificationOverride>();

        public static void RegisterProductionProvider(
            IFilterSignalsProductionProvider provider)
        {
            Register(
                provider,
                ProductionProviders,
                item => item.Id,
                "production provider");
        }

        public static void RegisterClassificationOverride(
            IFilterSignalsClassificationOverride classificationOverride)
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

        internal static IReadOnlyList<IFilterSignalsProductionProvider>
            GetProductionProviders()
        {
            return ProductionProviders
                .OrderBy(
                    provider => ClassificationDiagnostics.SafeId(
                        () => provider.Id),
                    StringComparer.Ordinal)
                .ToArray();
        }

        internal static IReadOnlyList<IFilterSignalsClassificationOverride>
            GetClassificationOverrides()
        {
            return ClassificationOverrides
                .OrderBy(
                    item => ClassificationDiagnostics.SafeId(
                        () => item.Id),
                    StringComparer.Ordinal)
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

            string id = ReadRegistrationId(item, getId, kind);
            foreach (T existing in items)
            {
                string existingId = ReadRegistrationId(existing, getId, kind);
                if (string.Equals(
                    existingId,
                    id,
                    StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "Duplicate Filter Signals " + kind + " ID: " + id);
                }
            }

            items.Add(item);
            ClassificationService.InvalidateAll();
        }

        private static string ReadRegistrationId<T>(
            T item,
            Func<T, string> getId,
            string kind)
            where T : class
        {
            string id;
            try
            {
                id = getId(item);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Filter Signals " + kind + " ID could not be read.",
                    exception);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "Filter Signals " + kind + " ID is required.",
                    nameof(item));
            }

            return id;
        }
    }
}
