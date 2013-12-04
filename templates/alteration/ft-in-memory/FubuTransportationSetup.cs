using System;
using FubuTransportation;
using FubuTransportation.Configuration;

namespace %NAMESPACE%
{
    public class TransportSettings
    {
        public TransportSettings()
        {
            Inbound = "memory://transport/inbound".ToUri();
            Outbound = "memory://transport/outbound".ToUri();
        }

        public Uri Inbound { get; set; }
        public Uri Outbound { get; set; }
    }

    public class %TRANSPORT_REGISTRY% : FubuTransportRegistry<TransportSettings>
    {
        public %TRANSPORT_REGISTRY%()
        {
            EnableInMemoryTransport();

            // More options are available for thread or task scheduling
            Channel(x => x.Inbound).ReadIncoming();

            // Static subscriptions
            Channel(x => x.Outbound)
                .AcceptsMessages(x => true);
        }
    }
}