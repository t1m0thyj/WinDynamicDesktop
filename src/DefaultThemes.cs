// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace WinDynamicDesktop
{
    class DefaultThemes
    {
        private static string[] yamlLines = Array.Empty<string>();

        public static string[] GetDefaultThemes()
        {
            string yamlText = Properties.Resources.DefaultThemesYaml;
            yamlLines = yamlText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select((line) => line.Trim())
                .Where((line) => !line.StartsWith("#")).ToArray();

            return yamlLines.Where((line) => !line.StartsWith("-"))
                .Select((line) => line.TrimEnd(':')).ToArray();
        }

        public static Uri[] GetThemeUriList(string themeId)
        {
            int startIndex = Array.FindIndex(yamlLines, (line) => line == themeId + ":") + 1;
            List<Uri> uriList = new List<Uri>();

            while ((startIndex < yamlLines.Length) && yamlLines[startIndex].StartsWith("- "))
            {
                uriList.Add(new Uri(yamlLines[startIndex].Substring(2)));
                startIndex++;
            }

            return uriList.ToArray();
        }
    }
}
