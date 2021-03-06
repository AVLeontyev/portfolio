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
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;

namespace strans
{
    public partial class Creator
    {
        /// <summary>
        /// Перечисление - индекс(или тип) объектов для которых может юыть установлен режим 'MODE'
        /// </summary>
        public enum INDEX_MODE
        {
            Unknown = -1
            , Host
            , Client
            , All
        }

        /// <summary>
        /// Перечисление - признак ошибки при создании объекта того или иного типа
        /// </summary>
        public enum ERROR
        {
            Host = -3
            , Client = -2
            , Unknown = -1
            , Ok
        }

        /// <summary>
        /// Наименование оконечной точки при развертывании служб
        ///  , при подключении к службам клиентов
        /// </summary>
        internal enum NameEndPoint {
            EndPointServiceTransModesTerminale
            , EndPointServiceTransModesCentre
        }

        /// <summary>
        /// Перечисление - составная часть режима работы приложения
        ///  , в ~ от аргументов командной строки
        ///  , некоторые элементы несовместимы друг с другом; например: Install+Uninstall
        /// </summary>
        [Flags]
        public enum MODE
        {
            Unknown = 0x0
            , Host = 0x1
            , Client = 0x2
            , Console = 0x4
            , ScNet = 0x8
            , Gui = 0x10
            , Install = 0x20
            , Uninstall = 0x40
            , Start = 0x100
            , Stop = 0x200
            , Restart = 0x400
            , Error = 0x800
        }

        public ERROR Error
        {
            get; private set;
        }

        public static List<ChannelEndpointElement> ListEndPoint
        {
            get
            {
                return ((ClientSection)ConfigurationManager.GetSection ("system.serviceModel/client")).Endpoints.Cast<ChannelEndpointElement> ().ToList ();
            }
        }

        private Host _host;

        private Client _client;

        private bool IsClientRunable
        {
            get
            {
                return (CmdArg.IsClientRunable == true) // нет необходимости выполнять клиент
                    && (((CmdArg.IsHostRunable == true) && (!(Error == ERROR.Host))) // произошла ошибка при развертывании служб - выполнение клиента не имеет смысла
                        || (CmdArg.IsHostRunable == false)); // развертывание служб не учитывать
            }
        }

        public Creator()
        {
            ConfigurationSection configSection;

            Error = ERROR.Unknown;

            //TODO: аргумент командной строки должен быть единственным (только 'Host' ИЛИ только 'Client')
            if (CmdArg.IsHostRunable == true) {
                Logging.Logg ().Action ($"ModeHost={CmdArg.ModeHost.ToString()}", Logging.INDEX_MESSAGE.NOT_SET);
                _host = new Host (CmdArg.ModeHost);
                Error = _host.Error == Host.ERROR.Ok
                    ? ERROR.Ok
                        : ERROR.Host;
            } else if (IsClientRunable == true) {
                Logging.Logg ().Action ($"ModeClient={CmdArg.ModeHost.ToString ()}", Logging.INDEX_MESSAGE.NOT_SET);
                _client = new Client (CmdArg.ModeClient);
                Error = _client.Error == Client.ERROR.Ok
                    ? ERROR.Ok
                        : ERROR.Client;
            } else
                ;
        }

        public void Start ()
        {
            if (CmdArg.IsHostRunable == true) {
                _host?.Start ();
                Error = _host.Error == Host.ERROR.Ok
                    ? ERROR.Ok
                        : ERROR.Host;
            } else
                ;

            if (IsClientRunable == true) {
                _client.Start();
                Error = _client.Error == Client.ERROR.Ok
                    ? ERROR.Ok
                        : ERROR.Client;
            } else
            // произошла ошибка при развертывании служб - выполнение клиента не имеет смысла
                ;
        }

        public void Stop ()
        {
            _client?.Stop ();

            _host?.Stop ();
        }
    }
}
