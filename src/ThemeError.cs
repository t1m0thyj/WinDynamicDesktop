// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Linq;

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

    class FailedToCreateThumbnail : ThemeError
    {
        public FailedToCreateThumbnail(string themeId) : base(themeId)
        {
            errorMsg = _("Failed to generate thumbnail: The image could not be loaded");
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

    class InvalidImageInThemeJSON : ThemeError
    {
        public InvalidImageInThemeJSON(string themeId, int imageId, string filename) : base(themeId)
        {
            errorMsg = string.Format(_("Could not find image {0} at {1}"), imageId, filename);
        }
    }

    class InvalidThemeJSON : ThemeError
    {
        public InvalidThemeJSON(string themeId, string message) : base(themeId)
        {
            errorMsg = string.Format(_("Could not read theme JSON file: {0}"), message);
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
        private string[] requiredFields = new string[] { "dayImageList", "imageFilename", "nightImageList" };

        public MissingFieldsInThemeJSON(string themeId) : base(themeId)
        {
            errorMsg = string.Format(_("Theme JSON file is missing one or more of these required fields: {0}"),
                string.Join(", ", requiredFields.Select((field) => "'" + field + "'")));
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

    class NoImagesMatchingPattern : ThemeError
    {
        public NoImagesMatchingPattern(string themeId, string pattern) : base(themeId)
        {
            errorMsg = string.Format(_("No images found that match the pattern {0}"), pattern);
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
