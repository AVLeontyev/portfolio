using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace strans
{
    partial class Host
    {
        class EndpointBehavior : IEndpointBehavior
        {
            public void AddBindingParameters (ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
            {
                
            }

            public void ApplyClientBehavior (ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
                
            }

            public void ApplyDispatchBehavior (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
            {
                endpointDispatcher.ChannelDispatcher.ChannelInitializers.Add (new HookChannelInitializer ());
            }

            public void Validate (ServiceEndpoint endpoint)
            {
                
            }
        }
    }
}
