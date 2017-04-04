using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Extensions.Session;

namespace Take.Blip.Client.Testing.Fakes
{
    /// <summary>
    /// In-memory implementation for ISessionManager, for testing purposes only.
    /// WARNING: node parameter is ignored, since when testing usually a 
    /// single node - the sender - is used
    /// </summary>
    public class SingleNodeMemorySessionManager : ISessionManager
    {
        private readonly Dictionary<string, string> _variables;

        public SingleNodeMemorySessionManager()
        {
            _variables = new Dictionary<string, string>();
        }

        public Task AddVariableAsync(Node node, string key, string value, CancellationToken cancellationToken)
        {
            _variables.Add(key, value);
            return Task.CompletedTask;
        }

        public Task ClearSessionAsync(Node node, CancellationToken cancellationToken)
        {
            _variables.Clear();
            return Task.CompletedTask;
        }

        public Task<string> GetCultureAsync(Node node, CancellationToken cancellationToken)
        {
            return Task.FromResult(CultureInfo.CurrentCulture.ToString());
        }

        public Task<NavigationSession> GetSessionAsync(Node node, CancellationToken cancellationToken)
        {
            var result = new NavigationSession
            {
                Variables = _variables.ToDictionary(p => p.Key, p => p.Value)
            };
            return Task.FromResult(result);
        }

        public Task<string> GetVariableAsync(Node node, string key, CancellationToken cancellationToken)
        {
            return Task.FromResult(_variables[key]);
        }

        public Task RemoveVariableAsync(Node node, string key, CancellationToken cancellationToken)
        {
            _variables.Remove(key);
            return Task.CompletedTask;
        }

        public Task SetCultureAsync(Node node, string culture, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
