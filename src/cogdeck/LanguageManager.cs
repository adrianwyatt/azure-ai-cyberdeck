using System;

namespace cogdeck
{
    internal class LanguageManager
    {
        private int _languageIndex = 0;
        private List<LanguageSet> _languages = new List<LanguageSet>();

        public LanguageManager()
        {
            _languages.Add(new LanguageSet()
            {
                Language = "en",
                Locale = "en-US",
                Voice = "en-US-DavisNeural"
            });

            _languages.Add(new LanguageSet()
            {
                Language = "fr",
                Locale = "fr-FR",
                Voice = "fr-FR-JeromeNeural"
            });

            _languages.Add(new LanguageSet()
            {
                Language = "es",
                Locale = "es-ES",
                Voice = "es-ES-NilNeural"
            });

        }
        public void Cycle() => _languageIndex = (_languageIndex + 1) % _languages.Count;

        public LanguageSet PeekNext() => _languages[(_languageIndex + 1) % _languages.Count];

        public LanguageSet Get() => _languages[_languageIndex];

        public class LanguageSet
        {
            public string Language { get; set; }
            public string Locale { get; set; }
            public string Voice { get; set; }
        }
    }
}
