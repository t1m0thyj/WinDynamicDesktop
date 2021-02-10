// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using ImageMagick;
using System;
using System.IO;

namespace WinDynamicDesktop
{
    class HeicConverter
    {
        public static void Convert(string heicFile)
        {
            string themeId = Path.GetFileNameWithoutExtension(heicFile).Replace(' ', '_');
            Directory.CreateDirectory(Path.Combine("themes", themeId));

            using (var imageList = new MagickImageCollection(heicFile))
            {
                for (int i = 0; i < imageList.Count; i++)
                {
                    Console.WriteLine(i);
                    imageList[i].Format = MagickFormat.Jpeg;
                    imageList[i].Quality = 98;
                    imageList[i].Write(Path.Combine("themes", themeId, string.Format("image-{0}.jpg", i + 1)));
                }
            }

            ThemeResult result = HeicMetadata.CreateThemeForHeic(heicFile);
        }
    }
}
