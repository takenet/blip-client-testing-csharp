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
        InternalOnDemandClientChannel _onDemandClientChannel;
        Assembly _assembly;
        IMessagingHubClient _client;

        public TestHost(Assembly assembly)
        {
            _assembly = assembly;
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

        public bool IsListening
            => _client != null ? _client.Listening : false;

        public Task DeliverIncomingMessageAsync(Message message)
            => _onDemandClientChannel.IncomingMessages.SendAsync(message);

        public Task<Message> RetrieveOutgoingMessageAsync()
            => _onDemandClientChannel.OutgoingMessages.ReceiveAsync();

        public Task<Notification> RetrieveOutgoingNotificationAsync()
            => _onDemandClientChannel.OutgoingNotifications.ReceiveAsync();

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
