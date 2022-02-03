using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace strans
{
    partial class Host
    {
        class HookChannelInitializer : IChannelInitializer
        {
            internal static int ConnectedClientCount = 0;

            public void Initialize (IClientChannel channel)
            {
                //TODO:
                ConnectedClientCount++;
                debug (channel, "initialized", channel.RemoteAddress.Uri.AbsolutePath);
                channel.Closed += delegate (object sender, EventArgs e) {
                    disconnected (sender, "closed");
                };
                channel.Faulted += delegate (object sender, EventArgs e) {
                    disconnected (sender, "faulted");
                };
            }

            static void disconnected (object sender, string sense_message)
            {
                //TODO:
                debug ((IClientChannel)sender, "disconnected", sense_message);
                ConnectedClientCount--;
            }

            static void debug (IClientChannel channel, params string [] sense_messages)
            {
                //TODO:
                Console.WriteLine ("[{0}]: client {1} <{2}>", DateTime.Now.ToString("HH:mm:ss.fff"), channel.SessionId, sense_messages [0], sense_messages [1]);
                ConnectedClientCount--;
            }
        }
    }
}
