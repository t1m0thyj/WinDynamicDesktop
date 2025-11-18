// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using SkiaSharp;

namespace WinDynamicDesktop.Skia
{
    sealed class BitmapCache
    {
        readonly int maxWidth;
        readonly int maxHeight;
        readonly object cacheLock = new object();
        readonly Dictionary<Uri, SKBitmap> images = new Dictionary<Uri, SKBitmap>();

        public SKBitmap this[Uri uri]
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
                        if (img != null)
                        {
                            images.Add(uri, img);
                        }
                        return img;
                    }
                }
            }
        }

        public void Clear()
        {
            lock (cacheLock)
            {
                foreach (var bitmap in images.Values)
                {
                    bitmap?.Dispose();
                }
                images.Clear();
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
                        maxWidth = screen.Bounds.Width;
                        maxHeight = screen.Bounds.Height;
                    }
                }
            }
            else
            {
                maxWidth = int.MaxValue;
                maxHeight = int.MaxValue;
            }
        }

        private SKBitmap CreateImage(Uri uri)
        {
            try
            {
                Stream stream = null;

                if (uri.IsAbsoluteUri && uri.Scheme == "file")
                {
                    string path = uri.LocalPath;
                    if (File.Exists(path))
                    {
                        stream = File.OpenRead(path);
                    }
                }
                else if (!uri.IsAbsoluteUri)
                {
                    // Embedded resource
                    stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(uri.OriginalString);
                }

                if (stream == null)
                {
                    return null;
                }

                using (stream)
                {
                    using (var codec = SKCodec.Create(stream))
                    {
                        if (codec == null)
                        {
                            return null;
                        }

                        var info = codec.Info;
                        
                        // Calculate scaled dimensions
                        int targetWidth = info.Width;
                        int targetHeight = info.Height;

                        if (info.Width > maxWidth || info.Height > maxHeight)
                        {
                            float scale = Math.Min((float)maxWidth / info.Width, (float)maxHeight / info.Height);
                            targetWidth = (int)(info.Width * scale);
                            targetHeight = (int)(info.Height * scale);
                        }

                        var bitmap = new SKBitmap(targetWidth, targetHeight, info.ColorType, info.AlphaType);
                        
                        if (targetWidth == info.Width && targetHeight == info.Height)
                        {
                            // No scaling needed
                            codec.GetPixels(bitmap.Info, bitmap.GetPixels());
                        }
                        else
                        {
                            // Decode at full size then scale down with high quality
                            using (var fullBitmap = new SKBitmap(info))
                            {
                                codec.GetPixels(fullBitmap.Info, fullBitmap.GetPixels());
                                fullBitmap.ScalePixels(bitmap, new SKSamplingOptions(SKCubicResampler.Mitchell));
                            }
                        }
                        
                        return bitmap;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
