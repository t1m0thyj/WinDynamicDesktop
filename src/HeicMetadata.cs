// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using PListNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace WinDynamicDesktop
{
    class H24Info
    {
        public double time;
        public int index;
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
            string line;
            string metadataType = null;
            string metadataRaw = null;

            using (StreamReader file = new StreamReader(heicFile))
            {
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
                    return new ThemeResult(ParseH24Metadata(metadataStream));
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

        private static List<int> GetImageListFromAngles(List<double> hourAngles, int startAngle, int endAngle)
        {
            List<int> imageList = new List<int>();
            for (int i = 0; i < hourAngles.Count; i++)
            {
                if (startAngle <= hourAngles[i] && hourAngles[i] < endAngle)
                {
                    imageList.Add(i + 1);
                }
            }
            imageList.Sort((a, b) => hourAngles[a - 1].CompareTo(hourAngles[b - 1]));
            return imageList;
        }

        private static ThemeConfig ParseH24Metadata(Stream metadataStream)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(metadataStream);
            List<H24Info> h24Info = new List<H24Info>();

            foreach (XmlNode node in doc.DocumentElement.SelectNodes("/plist/dict/array/dict"))
            {
                h24Info.Add(new H24Info
                {
                    time = double.Parse(node.SelectSingleNode(".//key[.=\"t\"]").NextSibling.InnerText),
                    index = int.Parse(node.SelectSingleNode(".//key[.=\"i\"]").NextSibling.InnerText)
                });
            }

            h24Info.Sort((a, b) => a.index.CompareTo(b.index));
            List<double> hourAngles = new List<double>();

            foreach (H24Info info in h24Info)
            {
                double hourAngle = (info.time - 0.5) * 360;
                hourAngles.Add(hourAngle);
                Console.WriteLine(string.Format("{0}\t{1}", info.index + 1, hourAngle));
            }

            List<int> dayImageList = GetImageListFromAngles(hourAngles, -75, 75);
            List<int> nightImageList = GetImageListFromAngles(hourAngles, 105, 180);
            nightImageList.AddRange(GetImageListFromAngles(hourAngles, -180, -105));
            List<int> sunriseImageList = GetImageListFromAngles(hourAngles, -105, -75);
            List<int> sunsetImageList = GetImageListFromAngles(hourAngles, 75, 105);

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

        private static ThemeConfig ParseSolarMetadata(Stream metadataStream)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(metadataStream);
            List<SolarInfo> solarInfo = new List<SolarInfo>();

            foreach (XmlNode node in doc.DocumentElement.SelectNodes("/plist/dict/array/dict"))
            {
                solarInfo.Add(new SolarInfo
                {
                    altitude = double.Parse(node.SelectSingleNode(".//key[.=\"a\"]").NextSibling.InnerText),
                    azimuth = double.Parse(node.SelectSingleNode(".//key[.=\"z\"]").NextSibling.InnerText),
                    index = int.Parse(node.SelectSingleNode(".//key[.=\"i\"]").NextSibling.InnerText)
                });
            }

            solarInfo.Sort((a, b) => a.index.CompareTo(b.index));
            List<double> hourAngles = new List<double>();

            foreach (SolarInfo info in solarInfo)
            {
                double hourAngle = Math.Asin(-Math.Cos(info.altitude * (Math.PI / 180d)) *
                    Math.Sin(info.azimuth * (Math.PI / 180d))) * (180d / Math.PI);
                if (info.altitude < 0)
                {
                    hourAngle = 180 - hourAngle;
                }
                if (hourAngle > 180)
                {
                    hourAngle -= 360;
                }
                hourAngles.Add(hourAngle);
                Console.WriteLine(string.Format("{0}\t{1}", info.index + 1, hourAngle));
            }

            List<int> dayImageList = GetImageListFromAngles(hourAngles, -84, 84);
            List<int> nightImageList = GetImageListFromAngles(hourAngles, 106, 180);
            nightImageList.AddRange(GetImageListFromAngles(hourAngles, -180, -106));
            List<int> sunriseImageList = GetImageListFromAngles(hourAngles, -106, -84);
            List<int> sunsetImageList = GetImageListFromAngles(hourAngles, 84, 106);

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
