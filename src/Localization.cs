// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using NGettext;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
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
            ConfigMigrator.CompatibilizeLocale();
            currentLocale = JsonConfig.settings.language;
            LoadLanguages();

            if (currentLocale == null)
            {
                string systemLocale = CultureInfo.CurrentUICulture.Name;
                currentLocale = languageCodes.Contains(systemLocale) ? systemLocale : "en";
                JsonConfig.settings.language = currentLocale;
            }
            else if (!JsonConfig.settings.usePoeditorLanguage)
            {
                LoadLocaleFromFile();
            }
            else
            {
                LoadLocaleFromWeb();
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

            foreach (Control childControl in GetControls(form))
            {
                if (childControl.GetType().GetProperty("Text") != null
                    && childControl.Text != null)
                {
                    childControl.Text = GetTranslation(childControl.Text);
                }
            }
        }

        public static void NotifyIfTestMode()
        {
            if (JsonConfig.settings.usePoeditorLanguage && catalog != null)
            {
                AppContext.ShowPopup(string.Format(
                    Localization.GetTranslation("Downloaded '{0}' translation from POEditor"), currentLocale));
            }
        }

        private static void LoadLanguages()
        {
            AddLanguage("Bahasa Indonesia", "id");  // Indonesian
            AddLanguage("Dansk", "da");  // Danish
            AddLanguage("Deutsch", "de");  // German
            AddLanguage("English", "en");  // English
            AddLanguage("Español", "es");  // Spanish
            AddLanguage("Français", "fr");  // French
            AddLanguage("Hrvatski", "hr");  // Croatian
            AddLanguage("Italiano", "it");  // Italian
            AddLanguage("Nederlands", "nl");  // Dutch
            AddLanguage("Polski", "pl");  // Polish
            AddLanguage("Português (do Brasil)", "pt-br");  // Portuguese (BR)
            AddLanguage("Pусский", "ru");  // Russian
            AddLanguage("Română", "ro");  // Romanian
            AddLanguage("Svenska", "sv");  // Swedish
            AddLanguage("Tiếng Việt", "vi");  // Vietnamese
            AddLanguage("Türkçe", "tr");  // Turkish
            AddLanguage("magyar", "hu");  // Hungarian
            AddLanguage("suomi", "fi");  // Finnish
            AddLanguage("íslenska", "is");  // Icelandic
            AddLanguage("Čeština", "cs");  // Czech
            AddLanguage("Ελληνικά", "el");  // Greek
            AddLanguage("Български", "bg");  // Bulgarian
            AddLanguage("Македонски", "mk");  // Macedonian
            AddLanguage("Українська", "uk");  // Ukrainian
            AddLanguage("عربي", "ar");  // Arabic
            AddLanguage("عربي (متحده عرب امارات)", "ar-ae");  // Arabic (U.A.E.)
            AddLanguage("हिन्दी", "hi");  // Hindi
            AddLanguage("বাংলা", "bn");  // Bengali
            AddLanguage("中文 (简体)", "zh-Hans");  // Chinese (simplified)
            AddLanguage("日本語", "ja");  // Japanese
            AddLanguage("正體中文 (繁體)", "zh-Hant");  // Chinese (traditional)
            AddLanguage("한국어", "ko");  // Korean
        }

        private static void AddLanguage(string languageName, string languageCode)
        {
            languageNames.Add(languageName);
            languageCodes.Add(languageCode);
        }

        private static void LoadLocaleFromWeb()
        {
            var client = new RestClient("https://api.poeditor.com");
            ProxyWrapper.ApplyProxyToClient(client);

            var request = new RestRequest("/v2/projects/export", Method.POST);
            request.AddParameter("api_token", poeditorApiToken);
            request.AddParameter("id", "293081");
            request.AddParameter("language", currentLocale);
            request.AddParameter("type", "mo");

            var response = client.Execute<PoEditorApiData>(request);
            if (!response.IsSuccessful)
            {
                return;
            }

            using (WebClient wc = new WebClient())
            {
                ProxyWrapper.ApplyProxyToClient(wc);
                byte[] moBinary = wc.DownloadData(response.Data.result.url);

                using (Stream stream = new MemoryStream(moBinary))
                {
                    catalog = new Catalog(stream, new CultureInfo(currentLocale));
                }
            }
        }

        // Code from https://stackoverflow.com/a/664083/5504760
        private static IEnumerable<Control> GetControls(Control form)
        {
            foreach (Control childControl in form.Controls)
            {
                foreach (Control grandChild in GetControls(childControl))
                {
                    yield return grandChild;
                }

                yield return childControl;
            }
        }
    }
}
