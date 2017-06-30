using System;
using Takenet.MessagingHub.Client.Host;

namespace Take.Blip.Client.Testing
{

    internal class OverridableServiceContainer : TypeServiceProvider
    {
        public OverridableServiceContainer(IServiceProvider secondaryServiceProvider = null)
            : base()
        {
            SecondaryServiceProvider = secondaryServiceProvider;
        }

        public override void RegisterService(Type serviceType, object instance)
        {
            TypeDictionary.Remove(serviceType);
            base.RegisterService(serviceType, instance);
        }

        public override void RegisterService(Type serviceType, Func<object> instanceFactory)
        {
            TypeDictionary.Remove(serviceType);
            base.RegisterService(serviceType, instanceFactory);
        }
    }
}
