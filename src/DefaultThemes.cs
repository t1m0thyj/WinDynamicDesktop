// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDynamicDesktop
{
    class DefaultThemes
    {
        private static string[] yamlLines = Array.Empty<string>();

        public static string[] GetDefaultThemes()
        {
            string yamlText = Properties.Resources.default_themes_yaml;
            yamlLines = yamlText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select((line) => line.Trim()).ToArray();

            return yamlLines.Where((line) => !line.StartsWith("-"))
                .Select((line) => line.Substring(0, line.Length - 1)).ToArray();
        }

        public static Uri[] GetThemeUriList(string themeId)
        {
            int startIndex = yamlLines.ToList().FindIndex((line) => line == themeId + ":") + 1;
            List<Uri> uriList = new List<Uri>();

            while ((startIndex < yamlLines.Length) && yamlLines[startIndex].StartsWith("-"))
            {
                uriList.Add(new Uri(yamlLines[startIndex].Substring(yamlLines[startIndex].LastIndexOf(" ") + 1)));
                startIndex++;
            }

            return uriList.ToArray();
        }
    }
}
