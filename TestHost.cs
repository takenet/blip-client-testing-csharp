using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization.Newtonsoft;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Host;

namespace Take.Blip.Client.Testing
{
    public class TestHost
    {
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(1);

        readonly TimeSpan _messageWaitTimeout;
        readonly TimeSpan _notificationWaitTimeout;
        readonly Assembly _assembly;

        InternalOnDemandClientChannel _onDemandClientChannel;
        IMessagingHubClient _client;

        /// <summary>
        /// In-memory host for a Blip SDK chatbot implementation
        /// </summary>
        /// <param name="assembly">The assembly for the full chatbot implementation</param>
        public TestHost(Assembly assembly, TimeSpan? messageWaitTimeout = null, TimeSpan? notificationWaitTimeout = null)
        {
            _assembly = assembly;
            _messageWaitTimeout = messageWaitTimeout ?? DefaultTimeout;
            _notificationWaitTimeout = notificationWaitTimeout ?? DefaultTimeout;
        }

        public async Task<IServiceContainer> StartAsync(Action<IServiceContainer> serviceOverrides = null)
        {
            var applicationFileName = Bootstrapper.DefaultApplicationFileName;
            var application = Application.ParseFromJsonFile(Path.Combine(GetAssemblyRoot(), applicationFileName));
            var typeResolver = new InternalTypeResolver(_assembly);

            var localServiceProvider = BuildServiceContainer(application, TypeResolver.Instance);
            localServiceProvider.RegisterService(typeof(IServiceProvider), localServiceProvider);
            localServiceProvider.RegisterService(typeof(IServiceContainer), localServiceProvider);
            localServiceProvider.RegisterService(typeof(Application), application);

            Bootstrapper.RegisterSettingsContainer(application, localServiceProvider, TypeResolver.Instance);

            var serializer = new JsonNetSerializer();
            _onDemandClientChannel = new InternalOnDemandClientChannel(serializer, application);
            _client = await Bootstrapper.BuildMessagingHubClientAsync(
                application,
                () => new MessagingHubClient(new InternalMessagingHubConnection(_onDemandClientChannel, serializer, application)),
                localServiceProvider,
                TypeResolver.Instance,
                serviceOverrides);

            await _client.StartAsync().ConfigureAwait(false);
            await Bootstrapper.BuildStartupAsync(application, localServiceProvider, TypeResolver.Instance);
            return localServiceProvider;
        }

        public Task StopAsync()
        {
            return _client?.StopAsync();
        }

        /// <summary>
        /// Helper to indicate successfull initalization
        /// </summary>
        public bool IsListening
            => _client != null ? _client.Listening : false;

        /// <summary>
        /// Deliver a message to be processed by the chatbot
        /// </summary>
        public Task DeliverIncomingMessageAsync(Message message)
            => _onDemandClientChannel.IncomingMessages.SendAsync(message);

        /// <summary>
        /// Retrieve next chatbot generated message, using current message wait timeout
        /// (default: 1s)
        /// </summary>
        public Task<Message> RetrieveOutgoingMessageAsync()
            => RetrieveOutgoingMessageAsync(_messageWaitTimeout);

        /// <summary>
        /// Retrieve next bot generated message, using specified timeout
        /// </summary>
        public async Task<Message> RetrieveOutgoingMessageAsync(TimeSpan timeout)
        {
            try
            {
                return await _onDemandClientChannel.OutgoingMessages.ReceiveAsync(timeout);
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve next chatbot generated notification, using current notification wait timeout
        /// (default: 1s)
        /// </summary>
        public Task<Notification> RetrieveOutgoingNotificationAsync()
            => RetrieveOutgoingNotificationAsync(_notificationWaitTimeout);

        /// <summary>
        /// Retrieve next bot generated notification, using specified timeout
        /// </summary>
        public async Task<Notification> RetrieveOutgoingNotificationAsync(TimeSpan timeout)
        {
            try
            {
                return await _onDemandClientChannel.OutgoingNotifications.ReceiveAsync(timeout);
            }
            catch (TimeoutException)
            {
                return null;
            }
        }


        private IServiceContainer BuildServiceContainer(Application application, TypeResolver typeResolver)
        {
            var serviceProviderType = typeResolver.Resolve(application.ServiceProviderType);
            var serviceProvider = (IServiceProvider)Activator.CreateInstance(serviceProviderType);

            return new OverridableServiceContainer(serviceProvider);
        }

        private string GetAssemblyRoot()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
