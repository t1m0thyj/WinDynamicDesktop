// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using NGettext;
using RestSharp;

namespace WinDynamicDesktop
{
    class Localization
    {
        public static List<string> languageCodes = new List<string>();
        public static List<string> languageNames = new List<string>();

        public static string currentLocale;
        private static ICatalog catalog = null;

        public static void Initialize()
        {
            UpdateHandler.CompatibilizeLocale();
            currentLocale = JsonConfig.settings.language;
            LoadLanguages();

            if (currentLocale == null)
            {
                string systemLocale = CultureInfo.CurrentUICulture.Name;
                currentLocale = languageCodes.Contains(systemLocale) ? systemLocale : "en";
                JsonConfig.settings.language = currentLocale;
            }
            else if (JsonConfig.settings.poeditorApiToken == null)
            {
                LoadLocaleFromFile();
            }
            else
            {
                LoadLocaleFromWeb();
            }


            if (JsonConfig.firstRun)
            {
                SelectLanguage();
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

        public static void SelectLanguage()
        {
            LanguageDialog langDialog = new LanguageDialog();
            langDialog.ShowDialog();
        }

        public static string GetCefLocale()
        {
            string cefLocale = (currentLocale == "zh-Hans") ? "zh-CN" : currentLocale;

            if (File.Exists(Path.Combine("cef", "locales", cefLocale.ToLower() + ".pak")))
            {
                return cefLocale;
            }

            return "en-US";
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

        private static void LoadLanguages()
        {
            AddLanguage("Bahasa Indonesia", "id");  // Indonesian
            AddLanguage("Deutsch", "de");  // German
            AddLanguage("English", "en");  // English
            AddLanguage("Español", "es");  // Spanish
            AddLanguage("Français", "fr");  // French
            AddLanguage("Italiano", "it");  // Italian
            AddLanguage("Nederlands", "nl");  // Dutch
            AddLanguage("Polski", "pl");  // Polish
            AddLanguage("Português (do Brasil)", "pt-br");  // Portuguese (BR)
            AddLanguage("Pусский", "ru");  // Russian
            AddLanguage("Română", "ro");  // Romanian
            AddLanguage("Türkçe", "tr");  // Turkish
            AddLanguage("Čeština", "cs");  // Czech
            AddLanguage("Български", "bg");  // Bulgarian
            AddLanguage("Македонски", "mk");  // Macedonian
            AddLanguage("हिन्दी", "hi");  // Hindi
            AddLanguage("বাংলা", "bn");  // Bengali
            AddLanguage("中文 (简体)", "zh-Hans");  // Chinese (Simplified)
            AddLanguage("日本語", "ja");  // Japanese
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
            request.AddParameter("api_token", JsonConfig.settings.poeditorApiToken);
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
                ProxyWrapper.ApplyProxyToClient(client);
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
