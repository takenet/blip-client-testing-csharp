using System;
using System.Linq;
using System.Reflection;
using Takenet.MessagingHub.Client.Host;

namespace Take.Blip.Client.Testing
{

    internal class InternalTypeResolver : ITypeResolver
    {
        private readonly Assembly _assembly;

        public InternalTypeResolver(Assembly assembly)
        {
            _assembly = assembly;
        }

        public Type Resolve(string typeName)
        {
            return _assembly
                .GetTypes()
                .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                ??
                TypeResolver.Instance.Resolve(typeName);
        }
    }
}
