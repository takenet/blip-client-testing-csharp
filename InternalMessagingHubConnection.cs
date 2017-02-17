using Lime.Protocol.Client;
using Lime.Protocol.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Connection;
using Takenet.MessagingHub.Client.Host;

namespace Take.Blip.Client.Testing
{

    internal class InternalMessagingHubConnection : IMessagingHubConnection
    {
        private static readonly TimeSpan DefaultSendTimeout = TimeSpan.FromSeconds(10);

        public bool IsConnected => OnDemandClientChannel?.IsEstablished ?? false;

        public TimeSpan SendTimeout { get; private set; }

        public int MaxConnectionRetries { get; set; }

        public IOnDemandClientChannel OnDemandClientChannel { get; }

        public InternalMessagingHubConnection(IOnDemandClientChannel onDemandClientChannel, IEnvelopeSerializer serializer, Application applicationSettings)
        {
            OnDemandClientChannel = onDemandClientChannel;
            SendTimeout = applicationSettings.SendTimeout <= 0 ? DefaultSendTimeout : TimeSpan.FromMilliseconds(applicationSettings.SendTimeout);
        }

        public Task ConnectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnDemandClientChannel.EstablishAsync(cancellationToken);
        }

        public Task DisconnectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnDemandClientChannel.FinishAsync(cancellationToken);
        }
    }
}
