using cogdeck.Configuration;
using Microsoft.Extensions.Options;

namespace cogdeck
{
    /// <summary>
    /// Manages the languages available for translation and their associated text-to-speech voices.
    /// </summary>
    internal class LanguageManager
    {
        /// <summary>
        /// The index of the current language in the list.
        /// </summary>
        private int _languageIndex = 0;

        /// <summary>
        /// The list of languages available for translation.
        /// </summary>
        private List<LanguageSet> _languages = new List<LanguageSet>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageManager"/> class.
        /// </summary>
        public LanguageManager(
            IOptions<LanguageOptions> options)
        {
            // Add the languages from the options
            foreach (LanguageOptions.LanguageSet language in options.Value.Languages)
            {
                _languages.Add(new LanguageSet()
                {
                    Language = language.Language,
                    Locale = language.Locale,
                    Voice = language.Voice
                });
            }
        }

        /// <summary>
        /// Cycles to the next language in the list.
        /// </summary>
        public void Cycle() => _languageIndex = (_languageIndex + 1) % _languages.Count;

        /// <summary>
        /// Peeks at the next language in the list.
        /// </summary>
        public LanguageSet PeekNext() => _languages[(_languageIndex + 1) % _languages.Count];

        /// <summary>
        /// Gets the current language set.
        /// </summary>
        public LanguageSet Get() => _languages[_languageIndex];

        /// <summary>
        /// Represents a language set.
        /// </summary>
        internal record LanguageSet
        {
            /// <summary>
            /// Gets or sets the language code (e.g. "en").
            /// </summary>
            public required string Language { get; set; }

            /// <summary>
            /// Gets or sets the locale code (e.g. "en-US").
            /// </summary>
            public required string Locale { get; set; }

            /// <summary>
            /// Gets or sets the voice name (e.g. "en-US-DavisNeural").
            /// </summary>
            public required string Voice { get; set; }
        }
    }
}
