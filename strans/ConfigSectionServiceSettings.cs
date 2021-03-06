using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Text;

namespace strans
{
    //public partial class Deployment
    //{
        internal class ConfigSectionServiceSettings : ConfigurationSection
        {
            public static string NameSection = "ServiceSettings";

            private const string _logging = "logging";
            private const string _elements = "settings";

            [ConfigurationProperty (_logging)]
            public ConfigurationLoggingElement Logging
            {
                get { return ((ConfigurationLoggingElement)(base [_logging])); }
            }

            [ConfigurationProperty (_elements)]
            public ServiceSettingsCollection ServiceSettings
            {
                get { return ((ServiceSettingsCollection)(base [_elements])); }
            }

            [ConfigurationCollection (typeof (ServiceSettingsElement))]
            public class ServiceSettingsCollection : ConfigurationElementCollection
            {
                private const string _tassembly = "tassembly";

                [ConfigurationProperty (_tassembly, DefaultValue = "STrans.Service", IsRequired = true)]
                public string TAssembly
                {
                    get { return (String)this [_tassembly]; }

                    set { this [_tassembly] = value; }
                }

                protected override ConfigurationElement CreateNewElement ()
                {
                    return new ServiceSettingsElement ();
                }

                protected override object GetElementKey (ConfigurationElement element)
                {
                    return ((ServiceSettingsElement)(element)).Type;
                }

                public ServiceSettingsElement this [int idx]
                {
                    get { return (ServiceSettingsElement)BaseGet (idx); }
                }
            }

            internal class ServiceSettingsElement : ConfigurationElement
            {
                private const string NameElementContract = "contract";
                private const string NameElementCAssembly = "cassembly";
                private const string NameElementType = "type";
                private const string NameElementTAssembly = "tassembly";
                private const string NameElementNameEndPoint = "nameendpoint";

                [ConfigurationProperty (NameElementContract, DefaultValue = "", IsKey = true, IsRequired = true)]
                public string Contract
                {
                    get { return ((string)(base [NameElementContract])); }

                    set { base [NameElementContract] = value; }
                }

                [StringValidator (InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 5, MaxLength = 667)]
                [ConfigurationProperty (NameElementCAssembly, DefaultValue = "StatisticTrans", IsKey = false, IsRequired = false)]
                public string CAssembly
                {
                    get { return ((string)(base [NameElementCAssembly])); }

                    set { base [NameElementCAssembly] = value; }
                }

                [ConfigurationProperty (NameElementType, DefaultValue = "", IsKey = true, IsRequired = true)]
                public string Type
                {
                    get { return ((string)(base [NameElementType])); }

                    set { base [NameElementType] = value; }
                }

                [StringValidator (InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 5, MaxLength = 667)]
                [ConfigurationProperty (NameElementTAssembly, DefaultValue = "STrans.Service", IsKey = false, IsRequired = false)]
                public string TAssembly
                {
                    get { return ((string)(base [NameElementTAssembly])); }

                    set { base [NameElementTAssembly] = value; }
                }

                //[StringValidator (InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MaxLength = 667)]
                [ConfigurationProperty (NameElementNameEndPoint, DefaultValue = Creator.NameEndPoint.EndPointServiceTransModesTerminale, IsKey = false, IsRequired = false)]
                //[ConfigurationProperty (NameElementNameEndPoint, DefaultValue = "", IsKey = false, IsRequired = false)]
                public Creator.NameEndPoint NameEndPoint
                //public string NameEndPoint
                {
                    get { return (Creator.NameEndPoint)Enum.Parse (typeof (Creator.NameEndPoint), base [NameElementNameEndPoint].ToString ().Trim ()); }
                    //get { return (string)base [NameElementNameEndPoint]; }

                    set { base [NameElementNameEndPoint] = value; }
                }
            }
        }
    //}
}
