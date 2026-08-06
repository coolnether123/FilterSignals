namespace FilterSignals.Domain
{
    /// <summary>
    /// Defines the four player-facing states used consistently by filtering,
    /// indicators, and compatibility integrations.
    /// </summary>
    public enum ProductionClassification
    {
        CanMakeNow = 0,
        ResearchUnlocked = 1,
        CannotMakeYet = 2,
        NotApplicable = 3
    }
}
