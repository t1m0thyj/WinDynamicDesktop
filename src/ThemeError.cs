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
    public abstract class ThemeError
    {
        public string errorMsg;
        public string themeId;
        internal static readonly Func<string, string> _ = Localization.GetTranslation;

        public ThemeError(string themeId_)
        {
            themeId = themeId_;
        }
    }

    class FailedToCopyImage : ThemeError
    {
        public FailedToCopyImage(string themeId, string imagePath) : base(themeId)
        {
            errorMsg = string.Format(_("Could not copy image file {0}"), imagePath);
        }
    }

    class FailedToDownloadImages : ThemeError
    {
        public FailedToDownloadImages(string themeId) : base(themeId)
        {
            errorMsg = string.Format(_("Could not download images for '{0}' theme"), themeId);
        }
    }

    class FailedToFindLocation : ThemeError
    {
        public FailedToFindLocation(string themeId, string path) : base(themeId)
        {
            errorMsg = string.Format(_("Could not find location {0}"), path);
        }
    }

    class InvalidThemeJSON : ThemeError
    {
        public InvalidThemeJSON(string themeId) : base(themeId)
        {
            errorMsg = _("Could not read theme JSON file because its format is invalid");
        }
    }

    class InvalidZIP : ThemeError
    {
        public InvalidZIP(string themeId, string zipPath) : base(themeId)
        {
            errorMsg = string.Format(_("Could not read ZIP file at {0} because its format is invalid"), zipPath);
        }
    }

    class MissingFieldsInThemeJSON : ThemeError
    {
        public MissingFieldsInThemeJSON(string themeId) : base(themeId)
        {
            errorMsg = _("Theme JSON file is missing one or more of these required fields: 'dayImageList', " +
                "'imageFilename', 'nightImageList', 'sunriseImageList', 'sunsetImageList'");
        }
    }

    class NoImagesInFolder : ThemeError
    {
        public NoImagesInFolder(string themeId, string path) : base(themeId)
        {
            errorMsg = string.Format(_("No images found in folder {0}"), path);
        }
    }

    class NoImagesInZIP : ThemeError
    {
        public NoImagesInZIP(string themeId, string zipPath) : base(themeId)
        {
            errorMsg = string.Format(_("No images found in ZIP file {0}"), zipPath);
        }
    }

    class NoThemeJSON : ThemeError
    {
        public NoThemeJSON(string themeId) : base(themeId)
        {
            errorMsg = _("Theme JSON file not found");
        }
    }

    class NoThemeJSONInZIP : ThemeError
    {
        public NoThemeJSONInZIP(string themeId, string zipPath) : base(themeId)
        {
            errorMsg = string.Format(_("Theme JSON not found in ZIP file {0}"), zipPath);
        }
    }
}
