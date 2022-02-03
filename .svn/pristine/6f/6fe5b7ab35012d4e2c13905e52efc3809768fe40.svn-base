using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace strans
{
    //partial class Client
    //{
        internal class ConfigSectionModesTrans : ConfigurationSection
        {
            public static string NameSection = "ModesTrans";

            private const string _logging = "logging";
            private const string _elements = "settings";

            [ConfigurationProperty (_logging)]
            public ConfigurationLoggingElement Logging
            {
                get { return ((ConfigurationLoggingElement)(base [_logging])); }
            }

            [ConfigurationProperty (_elements)]
            public ModesTransCollection ModesTransSettings
            {
                get { return ((ModesTransCollection)(base [_elements])); }
            }
        }

        [ConfigurationCollection (typeof (ModesTransClientCollection))]
        internal class ModesTransCollection : ConfigurationElementCollection
        {
            private const string _season = "season";

            [ConfigurationProperty (_season)]
            public ConfigurationSeasonElement Season
            {
                get { return ((ConfigurationSeasonElement)(base [_season])); }
            }

            protected override ConfigurationElement CreateNewElement ()
            {
                return new ModesTransClientCollection ();
            }

            protected override object GetElementKey (ConfigurationElement element)
            {
                return ((ModesTransClientCollection)(element)).NameEndPoint;
            }

            public ModesTransClientCollection this [int idx]
            {
                get { return (ModesTransClientCollection)BaseGet (idx); }
            }
        }

        [ConfigurationCollection (typeof (ConfigurationAtomicElement))]
        internal class ModesTransClientCollection : ConfigurationElementCollection
        {
            private const string _nameendpoint = "nameendpoint";

            [ConfigurationProperty (_nameendpoint, IsKey =true, IsRequired =true)]
            public Creator.NameEndPoint NameEndPoint
            {
                get { return (Creator.NameEndPoint)Enum.Parse (typeof(Creator.NameEndPoint), this [_nameendpoint].ToString().Trim()); }

                set { base [_nameendpoint] = value; }
            }

            protected override ConfigurationElement CreateNewElement ()
            {
                return new ConfigurationAtomicElement ();
            }

            protected override object GetElementKey (ConfigurationElement element)
            {
                return ((ConfigurationAtomicElement)(element)).Key;
            }

            public ConfigurationAtomicElement this [int idx]
            {
                get { return (ConfigurationAtomicElement)BaseGet (idx); }
            }

            public string GetValue (string key)
            {
                return (from atom in this.Cast<ConfigurationAtomicElement> () where atom.Key == key select atom.Value).DefaultIfEmpty(string.Empty).ElementAt (0);
            }
        }

        internal class ConfigurationAtomicElement : ConfigurationElement
        {
            private const string _key = "key";
            private const string _value = "value";

            [ConfigurationProperty (_key, IsKey =true, IsRequired = true)]
            public string Key
            {
                get { return (string)this [_key]; }

                set { this [_key] = value; }    
            }

            [ConfigurationProperty (_value, IsRequired = true)]
            public string Value
            {
                get { return (string)this [_value]; }

                set { this [_value] = value; }
            }
        }

        internal class ConfigurationLoggingElement : ConfigurationElement
        {
            private const string _prefix = "prefix";

            [ConfigurationProperty (_prefix, IsRequired = true)]
            public string Prefix
            {
                get { return (string)this [_prefix]; }

                set { this[_prefix] = value; }
            }
        }

        internal class ConfigurationSeasonElement : ConfigurationElement
        {
            private const string _stump = "stamp";
            private const string _action = "action";

            private const string stamp_format = "dd.MM.yyyy HH:mm";

            [ConfigurationProperty (_stump, IsRequired = true)]
            public DateTime Stamp
            {
                get
                {
                    if (this [_stump] is string) {
                        return DateTime.ParseExact (
                            this [_stump].ToString ()
                            , stamp_format
                            , CultureInfo.InvariantCulture);
                    } else {
                        DateTime time = (DateTime)this [_stump];
                        return DateTime.ParseExact (
                            time.ToString (stamp_format)
                            , stamp_format
                            , CultureInfo.InvariantCulture);
                    }
                }

                set { this[_stump] = value.ToString(stamp_format); }
            }

            [ConfigurationProperty (_action, DefaultValue =-1, IsRequired = true)]
            public int Action
            {
                get { return (int)this [_action]; }

                set { this [_action] = value; }
            }
        }
    //}
}
