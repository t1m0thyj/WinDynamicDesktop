using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using PListNet;

namespace WinDynamicDesktop
{
    class ImageInfo
    {
        public int imageId;
        public double confidence;
        public double azimuth;
    }

    class SolarInfo
    {
        public double altitude;
        public double azimuth;
        public int index;
    }

    class HeicMetadata
    {
        public static ThemeResult CreateThemeForHeic(string heicFile)
        {
            using StreamReader file = new StreamReader(heicFile);
            string line;
            string metadataType = null;
            string metadataRaw = null;

            while ((line = file.ReadLine()) != null)
            {
                int startIndex = line.IndexOf("apple_desktop:");

                if (startIndex != -1)
                {
                    Match match = Regex.Match(line.Substring(startIndex), "^apple_desktop:(\\w+)=\"(.+?)\"");

                    if (match.Success)
                    {
                        metadataType = match.Groups[1].Value;
                        metadataRaw = match.Groups[2].Value;
                        break;
                    }
                }
            }

            if (metadataRaw == null)
            {
                // TODO Throw error
                return new ThemeResult(new NoThemeJSON(""));
            }

            PNode metadataNode = PList.Load(new MemoryStream(Convert.FromBase64String(metadataRaw)));
            MemoryStream metadataStream = new MemoryStream();
            PList.Save(metadataNode, metadataStream, PListFormat.Xml);
            metadataStream.Position = 0;

            switch (metadataType)
            {
                case "h24":
                    // TODO Throw error
                    return new ThemeResult(new NoThemeJSON(""));
                    // return new ThemeResult(ParseH24Metadata(metadataStream));
                case "solar":
                    //try
                    //{
                    //    return new ThemeResult(ParseSolarMetadata(metadataStream));
                    //}
                    //catch (Exception e)
                    //{
                    //    // TODO Throw error
                    //    return new ThemeResult(new NoThemeJSON(""));
                    //}
                    return new ThemeResult(ParseSolarMetadata(metadataStream));
                default:
                    // TODO Throw error
                    return new ThemeResult(new NoThemeJSON(""));
            }
        }

        private static ImageInfo FindImageSolar(List<SolarInfo> solarInfo, int desiredAltitude, int desiredAzimuth)
        {
            bool wrapAround = desiredAzimuth < 45 || desiredAzimuth > 315;
            double minVariance = double.MaxValue;
            int bestIndex = 0;

            for (int i = 0; i < solarInfo.Count; i++)
            {
                SolarInfo si = solarInfo[i];

                double azimuthVariance = Math.Pow(si.azimuth - desiredAzimuth, 2);
                if (wrapAround)
                {
                    azimuthVariance = Math.Min(azimuthVariance, Math.Pow(360 - si.azimuth - desiredAzimuth, 2));
                }

                double variance = Math.Sqrt(Math.Pow(si.altitude - desiredAltitude, 2) + azimuthVariance);
                if (variance < minVariance)
                {
                    minVariance = variance;
                    bestIndex = i;
                }
            }

            return new ImageInfo
            {
                imageId = solarInfo[bestIndex].index + 1,
                confidence = (minVariance != 0) ? (1 / minVariance) : double.MaxValue,
                azimuth = desiredAzimuth
            };
        }

        private static List<int> GetImageListFromInfo(List<ImageInfo> imageInfo, int startAzimuth, int endAzimuth)
        {
            List<int> imageList = new List<int>();
            foreach (ImageInfo info in imageInfo)
            {
                if (startAzimuth <= info.azimuth && info.azimuth < endAzimuth)
                {
                    Console.WriteLine(info.confidence);
                    imageList.Add(info.imageId);
                }
            }
            return imageList;
        }

        private static ThemeConfig ParseSolarMetadata(Stream metadataStream)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(metadataStream);
            List<SolarInfo> solarInfo = new List<SolarInfo>();
            // metadataStream.Position = 0;
            // System.Windows.Forms.MessageBox.Show(new StreamReader(metadataStream).ReadToEnd());

            foreach (XmlNode node in doc.DocumentElement.SelectNodes("/plist/dict/array/dict"))
            {
                solarInfo.Add(new SolarInfo
                {
                    altitude = double.Parse(node.SelectSingleNode(".//key[.=\"a\"]").NextSibling.InnerText),
                    azimuth = double.Parse(node.SelectSingleNode(".//key[.=\"z\"]").NextSibling.InnerText),
                    index = int.Parse(node.SelectSingleNode(".//key[.=\"i\"]").NextSibling.InnerText)
                });
            }

            List<ImageInfo> imageInfo = new List<ImageInfo>();
            int lastImageId = 0;
            // -90 -> 0, 0 -> 90
            // 0 -> 90, 90 -> 180
            // 90 -> 0, 180 -> 270
            // 0 -> -90, 270 -> 360
            int altitude = -90;
            int azimuth = 0;

            while (azimuth < 360)
            {
                ImageInfo info = FindImageSolar(solarInfo, altitude, azimuth);
                if (info.imageId != lastImageId)
                {
                    int prevInfoIdx = imageInfo.FindIndex(info2 => info.imageId == info2.imageId);
                    if (prevInfoIdx == -1 || info.confidence > imageInfo[prevInfoIdx].confidence)
                    {
                        imageInfo.Add(info);
                        lastImageId = info.imageId;
                        if (prevInfoIdx != -1)
                        {
                            imageInfo.RemoveAt(prevInfoIdx);
                        }
                    }
                }
                else if (imageInfo.Count > 0 && info.confidence > imageInfo.Last().confidence)
                {
                    imageInfo.Last().confidence = info.confidence;
                    imageInfo.Last().azimuth = azimuth;
                }
                altitude += (azimuth < 180) ? 1 : -1;
                azimuth++;
            }

            System.Windows.Forms.MessageBox.Show(string.Join("\n", imageInfo.Select(info => info.imageId.ToString() + " " + info.azimuth.ToString() + " " + info.confidence.ToString())));

            List<int> dayImageList = GetImageListFromInfo(imageInfo, 92, 268);
            List<int> nightImageList = GetImageListFromInfo(imageInfo, 282, 360);
            nightImageList.AddRange(GetImageListFromInfo(imageInfo, 0, 78));
            List<int> sunriseImageList = GetImageListFromInfo(imageInfo, 78, 92);
            List<int> sunsetImageList = GetImageListFromInfo(imageInfo, 268, 282);

            ThemeConfig theme = new ThemeConfig
            {
                dayImageList = dayImageList.ToArray(),
                nightImageList = nightImageList.ToArray(),
                sunriseImageList = sunriseImageList.ToArray(),
                sunsetImageList = sunsetImageList.ToArray()
            };

            System.Windows.Forms.MessageBox.Show(Newtonsoft.Json.JsonConvert.SerializeObject(theme,
                Newtonsoft.Json.Formatting.Indented));
            return new ThemeConfig();
        }
    }
}
