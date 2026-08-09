using System;
using Verse;

namespace FilterSignals.Runtime
{
    /// <summary>
    /// Keeps compatibility-boundary failures observable without allowing a
    /// single broken extension to flood the log or interrupt filter drawing.
    /// </summary>
    internal static class ClassificationDiagnostics
    {
        internal static string SafeId(Func<string> readId)
        {
            try
            {
                string id = readId?.Invoke();
                return string.IsNullOrWhiteSpace(id) ? "<unnamed>" : id;
            }
            catch
            {
                return "<unavailable>";
            }
        }

        internal static void LogFailure(
            string boundary,
            string identity,
            string outcome,
            Exception exception)
        {
            string safeBoundary = string.IsNullOrWhiteSpace(boundary)
                ? "compatibility boundary"
                : boundary;
            string safeIdentity = string.IsNullOrWhiteSpace(identity)
                ? "<unnamed>"
                : identity;
            string message = "[Filter Signals] " + safeBoundary + " '" +
                safeIdentity + "' failed; " + outcome + ": " + exception;
            Log.ErrorOnce(message, StableHash(
                "FilterSignals." + safeBoundary + "." + safeIdentity));
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                for (int index = 0; index < value.Length; index++)
                {
                    hash = (hash * 31) + value[index];
                }

                return hash;
            }
        }
    }
}
