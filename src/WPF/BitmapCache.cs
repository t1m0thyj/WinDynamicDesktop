// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace WinDynamicDesktop.WPF
{
    sealed class BitmapCache
    {
        readonly int decodeWidth;
        readonly int decodeHeight;
        readonly object cacheLock = new object();
        readonly Dictionary<Uri, BitmapImage> images = new Dictionary<Uri, BitmapImage>();

        public BitmapImage this[Uri uri]
        {
            get
            {
                lock (cacheLock)
                {
                    if (images.ContainsKey(uri))
                    {
                        return images[uri];
                    }
                    else
                    {
                        var img = CreateImage(uri);
                        images.Add(uri, img);
                        return img;
                    }
                }
            }
        }

        public void Clear()
        {
            foreach (var uri in images.Keys.ToList())
            {
                images.Remove(uri);
            }
            GC.Collect();
        }

        public BitmapCache(bool limitDecodeSize = true)
        {
            if (limitDecodeSize)
            {
                int maxArea = 0;
                foreach (Screen screen in Screen.AllScreens)
                {
                    int area = screen.Bounds.Width * screen.Bounds.Height;
                    if (area > maxArea)
                    {
                        maxArea = area;
                        decodeWidth = screen.Bounds.Width;
                        decodeHeight = screen.Bounds.Height;
                    }
                }
            }
        }

        private BitmapImage CreateImage(Uri uri)
        {
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;

            if (uri.IsAbsoluteUri)
            {
                img.UriSource = uri;
            }
            else
            {
                img.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream(uri.OriginalString);
            }

            if (decodeWidth >= decodeHeight)
            {
                img.DecodePixelWidth = decodeWidth;
            }
            else
            {
                img.DecodePixelHeight = decodeHeight;
            }

            img.EndInit();
            img.Freeze();
            return img;
        }
    }
}
