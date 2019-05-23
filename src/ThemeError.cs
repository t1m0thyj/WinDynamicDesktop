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

        public ThemeError(string themeId_)
        {
            themeId = themeId_;
        }
    }

    class FailedToCopyImage : ThemeError
    {
        public FailedToCopyImage(string themeId, string imagePath) : base(themeId)
        {
            errorMsg = string.Format(Localization.GetTranslation("Failed to copy the image file {0}"), imagePath);
        }
    }

    class FailedToDownloadImages : ThemeError
    {
        public FailedToDownloadImages(string themeId) : base(themeId)
        {
            errorMsg = string.Format(Localization.GetTranslation("Failed to download images for the '{0}' theme."), themeId);
        }
    }

    class FailedToFindLocation : ThemeError
    {
        public FailedToFindLocation(string themeId, string path) : base(themeId)
        {
            errorMsg = string.Format(Localization.GetTranslation("Failed to find the location {0}"), path);
        }
    }

    class InvalidThemeJSON : ThemeError
    {
        public InvalidThemeJSON(string themeId) : base(themeId)
        {
            errorMsg = Localization.GetTranslation("Theme JSON file could not be read because its format is invalid.");
        }
    }

    class InvalidZIP : ThemeError
    {
        public InvalidZIP(string themeId, string zipPath) : base(themeId)
        {
            errorMsg = string.Format(Localization.GetTranslation("Failed to read the ZIP file at {0} because its " +
                "format is invalid."), zipPath);
        }
    }

    class MissingFieldsInThemeJSON : ThemeError
    {
        public MissingFieldsInThemeJSON(string themeId) : base(themeId)
        {
            errorMsg = Localization.GetTranslation("Theme JSON file is missing required fields. These include " +
                "'dayImageList', 'imageFilename', 'nightImageList', 'sunriseImageList', and " +
                "'sunsetImageList'.");
        }
    }

    class NoImagesInFolder : ThemeError
    {
        public NoImagesInFolder(string themeId, string path) : base(themeId)
        {
            errorMsg = string.Format(Localization.GetTranslation("No images found in the folder {0}"), path);
        }
    }

    class NoImagesInZIP : ThemeError
    {
        public NoImagesInZIP(string themeId, string zipPath) : base(themeId)
        {
            errorMsg = string.Format(Localization.GetTranslation("No images found in the ZIP file {0}"), zipPath);
        }
    }

    class NoThemeJSON : ThemeError
    {
        public NoThemeJSON(string themeId) : base(themeId)
        {
            errorMsg = Localization.GetTranslation("Theme JSON file could not be found.");
        }
    }

    class NoThemeJSONInZIP : ThemeError
    {
        public NoThemeJSONInZIP(string themeId, string zipPath) : base(themeId)
        {
            errorMsg = string.Format(Localization.GetTranslation("No theme JSON found in the ZIP file {0}"), zipPath);
        }
    }
}
