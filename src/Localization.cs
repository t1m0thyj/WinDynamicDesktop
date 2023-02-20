// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using NGettext;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class Localization
    {
        private static readonly string poeditorApiToken = Encoding.UTF8.GetString(Convert.FromBase64String(
            "ODdlMGM3ZjRmMjE1YjRiMjkwNTE4NDUyMWE4Y2FkNTE="));

        public static List<string> languageCodes = new List<string>();
        public static List<string> languageNames = new List<string>();

        public static string currentLocale;
        private static ICatalog catalog = null;

        public static void Initialize()
        {
            currentLocale = JsonConfig.settings.language?.Replace("poeditor:", "");
            LoadLanguages();

            if (currentLocale == null)
            {
                string systemLocale = CultureInfo.CurrentUICulture.Name;
                currentLocale = languageCodes.Contains(systemLocale) ? systemLocale : "en";
                JsonConfig.settings.language = currentLocale;
            }
            else if (!JsonConfig.settings.language.StartsWith("poeditor:"))
            {
                LoadLocaleFromFile();
            }
            else
            {
                LoadLocaleFromWeb().Wait();
            }

            if (JsonConfig.firstRun)
            {
                SelectLanguage(true);
            }
        }

        public static void LoadLocaleFromFile()
        {
            string moFile = currentLocale + ".mo";

            if (File.Exists(moFile))
            {
                using (Stream stream = File.OpenRead(moFile))
                {
                    catalog = new Catalog(stream, new CultureInfo(currentLocale));
                }
            }
            else
            {
                string resourceName = "WinDynamicDesktop.locale." + moFile;

                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        return;
                    }

                    catalog = new Catalog(stream, new CultureInfo(currentLocale));
                }
            }
        }

        public static void SelectLanguage(bool exitOnCancel)
        {
            LanguageDialog langDialog = new LanguageDialog();
            DialogResult result = langDialog.ShowDialog();
            if (result != DialogResult.OK && exitOnCancel)
            {
                Environment.Exit(0);
            }
        }

        public static string GetTranslation(string msg)
        {
            return (catalog != null) ? catalog.GetString(msg) : msg;
        }

        public static void TranslateForm(Form form)
        {
            if (form.Text != null)
            {
                form.Text = GetTranslation(form.Text);
            }

            foreach (Control childControl in AcrylicUI.GetControls(form))
            {
                if (childControl.GetType().GetProperty("Text") != null
                    && childControl.Text != null)
                {
                    childControl.Text = GetTranslation(childControl.Text);
                }

                if (childControl is ComboBox)
                {
                    for (int i = 0; i < ((ComboBox)childControl).Items.Count; i++)
                    {
                        ((ComboBox)childControl).Items[i] = GetTranslation((string)((ComboBox)childControl).Items[i]);
                    }
                }
            }
        }

        public static void NotifyIfTestMode()
        {
            if (JsonConfig.settings.language.StartsWith("poeditor:") && catalog != null)
            {
                AppContext.ShowPopup(string.Format(
                    Localization.GetTranslation("Downloaded '{0}' translation from POEditor"), currentLocale));
            }
        }

        private static void LoadLanguages()
        {
            // Language names from https://github.com/mozilla/language-mapping-list
            AddLanguage("Azərbaycan dili", "az");  // Azerbaijani
            AddLanguage("Basa Jawa", "jv");  // Javanese
            AddLanguage("Català", "ca");  // Catalan
            AddLanguage("Dansk", "da");  // Danish
            AddLanguage("Deutsch", "de");  // German
            AddLanguage("English", "en");  // English
            AddLanguage("Español", "es");  // Spanish
            AddLanguage("Français", "fr");  // French
            AddLanguage("Galego", "gl");  // Galician
            AddLanguage("Hrvatski", "hr");  // Croatian
            AddLanguage("Italiano", "it");  // Italian
            AddLanguage("Lëtzebuergesch", "lb");  // Luxembourgish
            AddLanguage("Magyar", "hu");  // Hungarian
            AddLanguage("Nederlands", "nl");  // Dutch
            AddLanguage("Polski", "pl");  // Polish
            AddLanguage("Português", "pt");  // Portuguese
            AddLanguage("Português (Brasil)", "pt-br");  // Portuguese (BR)
            AddLanguage("Pусский", "ru");  // Russian
            AddLanguage("Română", "ro");  // Romanian
            AddLanguage("Suomi", "fi");  // Finnish
            AddLanguage("Svenska", "sv");  // Swedish
            AddLanguage("Tiếng Việt", "vi");  // Vietnamese
            AddLanguage("Türkçe", "tr");  // Turkish
            AddLanguage("eesti keel", "et");  // Estonian
            AddLanguage("Íslenska", "is");  // Icelandic
            AddLanguage("Čeština", "cs");  // Czech
            AddLanguage("Ελληνικά", "el");  // Greek
            AddLanguage("Български", "bg");  // Bulgarian
            AddLanguage("Македонски", "mk");  // Macedonian
            AddLanguage("Українська", "uk");  // Ukrainian
            AddLanguage("Қазақша", "kk");  // Kazakh
            AddLanguage("עברית", "he");  // Hebrew
            AddLanguage("العربية", "ar");  // Arabic
            AddLanguage("العربية (الإمارات العربية المتحدة)", "ar-ae");  // Arabic (U.A.E.)
            AddLanguage("فارسی", "fa");  // Persian
            AddLanguage("हिन्दी", "hi");  // Hindi
            AddLanguage("বাংলা", "bn");  // Bengali
            AddLanguage("ภาษาไทย", "th");  // Thai
            AddLanguage("ဗမာစကာ", "my");  // Burmese
            AddLanguage("አማርኛ", "am");  // Amharic
            AddLanguage("中文简体", "zh-Hans");  // Chinese Simplified
            AddLanguage("中文繁體", "zh-Hant");  // Chinese Traditional
            AddLanguage("日本語", "ja");  // Japanese
            AddLanguage("한국어", "ko");  // Korean
        }

        private static void AddLanguage(string languageName, string languageCode)
        {
            languageNames.Add(languageName);
            languageCodes.Add(languageCode);
        }

        private static async Task LoadLocaleFromWeb()
        {
            var client = new RestClient("https://api.poeditor.com");

            var request = new RestRequest("v2/projects/export", Method.Post);
            request.AddParameter("api_token", poeditorApiToken);
            request.AddParameter("id", "293081");
            request.AddParameter("language", currentLocale);
            request.AddParameter("type", "mo");

            var response = await client.ExecuteAsync<PoEditorApiData>(request);
            if (!response.IsSuccessful)
            {
                return;
            }

            using (HttpClient httpClient = new HttpClient())
            {
                byte[] moBinary = await httpClient.GetByteArrayAsync(response.Data.result.url);

                using (Stream stream = new MemoryStream(moBinary))
                {
                    catalog = new Catalog(stream, new CultureInfo(currentLocale));
                }
            }
        }
    }
}
