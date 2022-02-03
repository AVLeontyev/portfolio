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
    partial class Host : Deployment
    {
        /// <summary>
        /// Конструктор - основной (без аргументов)
        /// </summary>
        public Host (Creator.MODE mode)
            : base(mode)
        {
        }

        public override void Start ()
        {
            ServiceHost serviceHost;
            StatisticTrans.Contract.IServiceTransModes svcSingleInstanse = null;
            ServiceEndpoint serviceEndPoint;

            if ((Error == ERROR.Ok)
                && (Equals (_serviceSettingsSection, null) == false)) {
                _services.ToList ().ForEach (service => {
                    serviceHost = null;

                    try {
                        serviceHost = service.Host;
                        serviceEndPoint = serviceHost.AddServiceEndpoint (service.CEPE.Contract, new WSDualHttpBinding (service.CEPE.BindingConfiguration), service.CEPE.Address);
                        serviceEndPoint.Behaviors.Add (new EndpointBehavior());

                        serviceHost.Open ();

                        serviceHost.Faulted += Svc_Faulted;
                        serviceHost.Closing += Svc_Closing;

                        //!!! behavior - 'InstanceContextMode = InstanceContextMode.PerSession'
                        //svcSingleInstanse = (StatisticTrans.Contract.IServiceTransModes)serviceHost.SingletonInstance;
                        //svcSingleInstanse.Initialize ();
                        //svcSingleInstanse.Start ();

                        var dispatcher = serviceHost.ChannelDispatchers [0] as ChannelDispatcher;
                        //serviceHost.ChannelDispatchers [0].Opened += channelDispatcher_Opened;
                        //serviceHost.ChannelDispatchers [0].Closed += channelDispatcher_Closed;
                        //serviceHost.ChannelDispatchers [0].Listener.Opened += channelDispatcherListener_Opened;
                        //serviceHost.ChannelDispatchers [0].Listener.Closed += channelDispatcherListener_Closed;

                        ////???
                        //serviceHost.Description.Behaviors.Add (new ServiceBehavior ());

                        Console.WriteLine ($"Service contract: {service.SSE.Contract} <{serviceHost.State}>: [{string.Join (";", string.Join (";", from sep in serviceHost.Description.Endpoints.Cast<ServiceEndpoint> () select sep.Address.Uri.AbsoluteUri))}]...");
                    } catch (Exception e) {
                        service.Error =
                        Error =
                            ERROR.Open;

                        Console.WriteLine ($"Service contract: {service.SSE.Contract} opened error: {e.Message}...");
                        Logging.Logg ().Exception (e, "Main - service opened...", Logging.INDEX_MESSAGE.NOT_SET);
                    } finally {
                        if ((Equals (serviceHost, null) == false)
                            && (!(service.Error == ERROR.Ok))
                            && (serviceHost.State == CommunicationState.Opened)) {
                            serviceHost.Close ();
                        } else
                            ;
                    }
                });
            } else
                ;
        }

        private void channelDispatcher_Opened (object sender, EventArgs e)
        {
            
        }

        private void channelDispatcher_Closed (object sender, EventArgs e)
        {
            
        }

        private void channelDispatcherListener_Opened (object sender, EventArgs e)
        {

        }

        private void channelDispatcherListener_Closed (object sender, EventArgs e)
        {

        }

        public override void Stop ()
        {
            try {
                if ((Error == ERROR.Ok)
                    && (Equals (_services, null) == false))
                    _services.ToList ().ForEach (service => {
                        try {
                            //TODO: Single instanse to stopped...
                            service.Host.Close ();
                        } catch (Exception e) {
                            Console.Write ($"Service contract: {service.CEPE.Contract} closed error: {e.Message}...");

                            Logging.Logg ().Exception (e, "Main - service closed...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    });
                else
                    ;
            } catch { }
        }

        private void Svc_Closing (object sender, EventArgs e)
        {
            debug (sender as ServiceHost, "closing");
        }

        private void Svc_Faulted (object sender, EventArgs e)
        {
            debug (sender as ServiceHost, "faulted");
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
}
