namespace cogdeck.Configuration
{
    public record LanguageOptions
    {
        public required LanguageSet[] Languages { get; init; }

        public record LanguageSet
        {
            public required string Language { get; init; }
            public required string Locale { get; init; }
            public required string Voice { get; init; }
        }
    }
}