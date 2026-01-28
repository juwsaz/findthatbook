namespace FindThatBook.Domain.Constants;

/// <summary>
/// Domain constants for FindThatBook matching logic.
/// </summary>
public static class DomainConstants
{
    /// <summary>
    /// Constants related to book matching weights and thresholds.
    /// </summary>
    public static class Matching
    {
        // Strategy weights (must sum to 1.0)
        public const double TitleWeight = 0.40;
        public const double AuthorWeight = 0.35;
        public const double YearWeight = 0.10;
        public const double KeywordWeight = 0.15;

        // Match strength thresholds
        public const double ExactMatchThreshold = 0.95;
        public const double StrongMatchThreshold = 0.70;
        public const double HighTitleThreshold = 0.85;
        public const double PartialMatchThreshold = 0.60;
        public const double ModerateMatchThreshold = 0.40;
        public const double WeakMatchThreshold = 0.30;
        public const double YearConfirmationThreshold = 0.80;

        // Title matching scores
        public const double TitleExactScore = 1.0;
        public const double TitleContainsScore = 0.85;
        public const double TitleContainedScore = 0.75;
        public const double TitleWordMatchBaseScore = 0.50;
        public const double TitleWordMatchMultiplier = 0.30;
        public const double TitleLowWordMatchMultiplier = 0.40;
        public const double TitleWordMatchRatioThreshold = 0.50;

        // Author matching scores
        public const double AuthorExactScore = 1.0;
        public const double AuthorLastNameOnlyScore = 0.70;
        public const double AuthorLastNameWithFirstScore = 0.90;
        public const double AuthorPartialBaseScore = 0.50;
        public const double AuthorPartialMultiplier = 0.30;
        public const double AuthorPartialRatioThreshold = 0.50;
        public const int AuthorMinPartLength = 2;

        // Year matching scores
        public const double YearExactScore = 1.0;
        public const double YearCloseScore = 0.80;
        public const double YearApproximateScore = 0.50;
        public const int YearCloseRange = 2;
        public const int YearApproximateRange = 5;
    }

    /// <summary>
    /// API-related constants.
    /// </summary>
    public static class Api
    {
        public const int DefaultMaxResults = 5;
        public const int MinMaxResults = 1;
        public const int MaxMaxResults = 10;
    }
}
