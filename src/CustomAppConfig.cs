using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;

namespace WinDynamicDesktop
{
    public class ThemeElement : ConfigurationElement
    {
        private string _value;

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            reader.MoveToAttribute(0);
            this[reader.Name] = reader.Value;
            reader.MoveToElement();
            _value = (string)reader.ReadElementContentAs(typeof(string), null);
        }

        [ConfigurationProperty("id", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Id
        {
            get { return (string)this["id"]; }
        }

        public string[] UriList
        {
            get { return _value.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries); }
        }
    }

    [ConfigurationCollection(typeof(ThemeElement), AddItemName = "theme")]
    public class ThemeElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ThemeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ThemeElement)element).Id;
        }
    }

    public class CustomConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("defaultThemes", IsDefaultCollection = true)]
        public ThemeElementCollection DefaultThemes
        {
            get { return (ThemeElementCollection)this["defaultThemes"]; }
        }
    }

    public class CustomAppConfig
    {
        public static string[] GetDefaultThemes()
        {
            CustomConfigSection section = (CustomConfigSection)ConfigurationManager.GetSection("customConfig");
            List<string> defaultThemes = new List<string>();

            foreach (ThemeElement elem in section.DefaultThemes)
            {
                defaultThemes.Add(elem.Id);
            }

            return defaultThemes.ToArray();
        }

        public static Uri[] GetThemeUriList(string themeId)
        {
            CustomConfigSection section = (CustomConfigSection)ConfigurationManager.GetSection("customConfig");

            foreach (ThemeElement elem in section.DefaultThemes)
            {
                if (elem.Id == themeId)
                {
                    return elem.UriList.Select(uri => new Uri(uri)).ToArray();
                }
            }

            return new Uri[0];
        }
    }
}
