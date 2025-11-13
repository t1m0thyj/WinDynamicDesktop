// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using NGettext;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class Localization
    {
        public static string[] languageCodes = new string[]
        {
            "am", "ar", "az",
            "bg", "bn",
            "ca", "cs",
            "da", "de",
            "el", "en", "es", "et",
            "fa", "fi", "fr",
            "gl",
            "he", "hi", "hr", "hu",
            "id", "is", "it",
            "ja", "jv",
            "kk", "ko",
            "lb",
            "mk", "my",
            "nl",
            "pl", "pt", "pt-br",
            "ro", "ru",
            "sk", "sv",
            "ta", "th", "tr",
            "ug", "uk",
            "vi",
            "zh-Hans", "zh-Hant", "zh-TW"
        };

        public static string currentLocale;
        private static ICatalog catalog = null;

        public static void Initialize()
        {
            currentLocale = JsonConfig.settings.language?.Replace("poeditor:", "");

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

        private static void LoadLocaleFromWeb()
        {
            var client = new RestClient("https://api.poeditor.com");

            var request = new RestRequest("v2/projects/export", Method.Post);
            request.AddParameter("api_token", Environment.GetEnvironmentVariable("POEDITOR_API_TOKEN"));
            request.AddParameter("id", "293081");
            request.AddParameter("language", currentLocale);
            request.AddParameter("type", "mo");

            var response = client.Execute<PoEditorApiData>(request);
            if (!response.IsSuccessful)
            {
                return;
            }

            using (HttpClient httpClient = new HttpClient())
            {
                byte[] moBinary = httpClient.GetByteArrayAsync(response.Data.result.url).Result;

                using (Stream stream = new MemoryStream(moBinary))
                {
                    catalog = new Catalog(stream, new CultureInfo(currentLocale));
                }
            }
        }
    }
}
