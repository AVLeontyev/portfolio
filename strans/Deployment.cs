using ASUTP;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace strans
{
    internal abstract partial class Deployment
    {
        public enum ERROR { Open = -4, Service = -3, Contract = -2, Unknown = -1, Ok, }

        public class Service
        {
            /// <summary>
            /// Ссылка на контейнер, содержащий развертываемую службу
            /// </summary>
            public ServiceHost Host;
            /// <summary>
            /// Тип развертываемой службы
            /// </summary>
            public Type Type;
            /// <summary>
            /// Конфигурация развертываемой службы
            /// </summary>
            public ConfigSectionServiceSettings.ServiceSettingsElement SSE;
            /// <summary>
            /// Конфигурация канала связи развертываемой службы
            /// </summary>
            public ChannelEndpointElement CEPE;
            /// <summary>
            /// Ошибка при развертывании службы
            /// </summary>
            public ERROR Error;
        }

        /// <summary>
        /// Секция в файле конфигурации развертываемой службы
        /// </summary>
        protected ConfigSectionServiceSettings _serviceSettingsSection;

        /// <summary>
        /// Список развертываемых служб
        /// </summary>
        protected List<Service> _services;

        /// <summary>
        /// Ошибка совокупная при развертывании служб
        /// </summary>
        public ERROR Error { get; protected set; }

        /// <summary>
        /// Конструктор - основной (без аргументов)
        /// </summary>
        public Deployment (Creator.MODE mode)
        {
            Error = ERROR.Unknown;

            List<Assembly> allAsssemblies = null;
            List<Type> contractTypes = null
                , serviceTypes = null;

            string addresses = string.Empty;
            IEnumerable<Creator.NameEndPoint> nameEndPointServices =  null;

            try {
                if (mode.HasFlag(Creator.MODE.Console) == true) {
                    // прочитать секцию в файле конфигурации
                    _serviceSettingsSection =
                        (ConfigSectionServiceSettings)ConfigurationManager.GetSection ($"{ConfigSectionServiceSettings.NameSection}");
                    // получить все сборки
                    allAsssemblies = AppDomain.CurrentDomain.GetAssemblies ().ToList ();
                    // получить все типы контрактов развертываемых служб
                    contractTypes = _serviceSettingsSection.GetContractTypes(allAsssemblies);

                    // проверить загрузку сборок для контрактов
                    if (_serviceSettingsSection.ContainsContractTypes (contractTypes) == true) {
                    // сборки для котрактов загружены
                        // загрузить сборки для служб
                        _serviceSettingsSection.ServiceSettings.Cast<ConfigSectionServiceSettings.ServiceSettingsElement> ().ToList ()
                            .ForEach (sse => {
                                Assembly svcAsm;
                                svcAsm = Assembly.LoadFrom ($"{sse.TAssembly}.dll");
                                if (Equals (svcAsm, null) == true)
                                    throw new InvalidOperationException ($"Assembliy a path <{sse.TAssembly}.dll> can't loaded...");
                                else
                                    ;
                            });

                        //!!! обновить (добавить сборки для служб)
                        allAsssemblies = AppDomain.CurrentDomain.GetAssemblies ().ToList ();
                        // получить все типы служб
                        serviceTypes = _serviceSettingsSection.GetServicesTypes(allAsssemblies);

                        // проверить загрузку сборок для служб
                        if (_serviceSettingsSection.ContainsServicesTypes(serviceTypes) == true) {
                        // сборки для служб загружены
                            nameEndPointServices = (from see in _serviceSettingsSection.ServiceSettings.Cast<ConfigSectionServiceSettings.ServiceSettingsElement> ()
                                select
                                    //(Creator.NameEndPoint)Enum.Parse(typeof(Creator.NameEndPoint), see.NameEndPoint)
                                    see.NameEndPoint
                                    );

                            _services = _serviceSettingsSection.GetServices(serviceTypes, Creator.ListEndPoint);

                            Error = ERROR.Ok;
                        } else {
                            // признак ошибки
                            Error = ERROR.Service;

                            _serviceSettingsSection = null;
                        }
                    } else {
                    // признак ошибки
                        Error = ERROR.Contract;

                        _serviceSettingsSection = null;
                    }
                } else
                    ;
            } catch (Exception e) {
                _serviceSettingsSection = null;

                Console.Write ($"service settings section <{ConfigSectionServiceSettings.NameSection}> can't passed...{Environment.NewLine}message={e.Message}...");
                Console.ReadKey (false);
            }
        }

        public abstract void Start ();

        public abstract void Stop ();

        internal Type GetTypeContract (Creator.NameEndPoint nameEndPoint)
        {
            Service service;

            service = GetService (nameEndPoint);

            return (from type in _serviceSettingsSection.GetContractTypes(AppDomain.CurrentDomain.GetAssemblies ().ToList()) where type.FullName == service.SSE.Contract select type).ElementAt(0);
        }

        internal Type GetTypeService (Creator.NameEndPoint nameEndPoint)
        {
            return _services.FirstOrDefault (service => service.SSE.NameEndPoint.Equals (nameEndPoint) == true).Type;
        }

        internal Service GetService (Creator.NameEndPoint nameEndPoint)
        {
            return _services.FirstOrDefault (service => service.SSE.NameEndPoint.Equals (nameEndPoint) == true);
        }

        internal ConfigSectionServiceSettings.ServiceSettingsElement GetSSE (ServiceDescription desc)
        {
            ConfigSectionServiceSettings.ServiceSettingsElement sseRes;
            IEnumerable<Service> servicesByCriteria;

            servicesByCriteria = from service in _services where service.Host.Description == desc select service;

            if (servicesByCriteria.Count () == 1)
                sseRes = servicesByCriteria.ElementAt (0).SSE;
            else
                sseRes = new ConfigSectionServiceSettings.ServiceSettingsElement ();

            return sseRes;
        }

        private void debug (ServiceHost host, string sense_message)
        {
            string mesToLog = string.Empty;
            ConfigSectionServiceSettings.ServiceSettingsElement sse = GetSSE (host.Description);

            mesToLog = $"Service contract: {sse.Contract} {sense_message}...";

            Console.WriteLine (mesToLog);
            Logging.Logg ().Debug (mesToLog, Logging.INDEX_MESSAGE.NOT_SET);
        }
    }

    static class ConfigSectionServiceSettingsExtensions
    {
        public static List<Type> GetContractTypes (this ConfigSectionServiceSettings settings, List<Assembly> assemblies)
        {
            return (from asm in assemblies
                select (from type in asm.GetTypes ()
                    join contract in (from sse in settings.ServiceSettings.Cast<ConfigSectionServiceSettings.ServiceSettingsElement> () select sse.Contract) on type.FullName equals contract
                    select type)).SelectMany (t => t).ToList ();
        }

        public static List<Type> GetServicesTypes (this ConfigSectionServiceSettings settings, List<Assembly> assemblies)
        {
            return (from asm in assemblies
                select (from type in asm.GetTypes ()
                    join service in (from sse in settings.ServiceSettings.Cast<ConfigSectionServiceSettings.ServiceSettingsElement> () select sse.Type) on type.FullName equals service
                    select type)).SelectMany (t => t).ToList ();
        }

        public static bool ContainsContractTypes (this ConfigSectionServiceSettings settings, List<Type> contractTypes)
        {
            return (from sse in settings.ServiceSettings.Cast<ConfigSectionServiceSettings.ServiceSettingsElement> () select sse.CAssembly).ToList ()
                .TrueForAll (cassembly => {
                    return (from type in contractTypes where type.AssemblyQualifiedName.Contains (cassembly) select type).Count () > 0;
                });
        }

        public static bool ContainsServicesTypes (this ConfigSectionServiceSettings settings, List<Type> serviceTypes)
        {
            return (from sse in settings.ServiceSettings.Cast<ConfigSectionServiceSettings.ServiceSettingsElement> () select sse.TAssembly).ToList ()
                .TrueForAll (tassembly => {
                    return (from type in serviceTypes where type.AssemblyQualifiedName.Contains (tassembly) select type).Count () > 0;
                });
        }

        public static List<Host.Service> GetServices (this ConfigSectionServiceSettings settings, List<Type> serviceTypes, List<ChannelEndpointElement> epChannels)
        {
            return (from sse in settings.ServiceSettings.Cast<ConfigSectionServiceSettings.ServiceSettingsElement> ()
                join serviceType in serviceTypes on sse.Type equals serviceType.FullName
                join ePointChannel in epChannels on sse.NameEndPoint equals Enum.Parse (typeof(Creator.NameEndPoint), ePointChannel.Name)
                    select new Host.Service { Host = new ServiceHost (serviceType), Type = serviceType, SSE = sse, CEPE = ePointChannel, Error = Host.ERROR.Ok })
                        .ToList ();
        }
    }
}
