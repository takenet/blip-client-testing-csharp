using Lime.Protocol;
using System;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Extensions.Bucket;

namespace Take.Blip.Client.Testing.Fakes
{
    public class MemoryBucketExtension : IBucketExtension, IDisposable
    {
        private readonly MemoryCache _db;

        public MemoryBucketExtension()
        {
            _db = new MemoryCache(nameof(MemoryBucketExtension));
        }

        public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            _db.Remove(id);
            return Task.CompletedTask;
        }

        public Task<T> GetAsync<T>(string id, CancellationToken cancellationToken = default(CancellationToken)) 
            where T : Document
        {
            return Task.FromResult((T)_db.Get(id));
        }

        public Task SetAsync<T>(string id, T document, TimeSpan expiration = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken)) 
            where T : Document
        {
            _db.Remove(id);
            var item = new CacheItem(id, document);
            var policy = new CacheItemPolicy();
            if (expiration != default(TimeSpan))
            {
                policy.AbsoluteExpiration = DateTimeOffset.UtcNow.Add(expiration);
            }
            _db.Add(item, policy);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

    }
}
