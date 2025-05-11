using Auditing.Api.Common.Interfaces;
using Auditing.Api.Domain.Events;
using Microsoft.Extensions.Caching.Memory;

namespace Auditing.Api.Infrastructure.Persistance
{
    public class AuditingStorage(ICacheService cache) : IAuditingStorage
    {
        private List<IAuditingEvent> AuditingList = new List<IAuditingEvent>();
        private readonly ICacheService _cache = cache;

        public async Task<bool> AddAuditing(IAuditingEvent Auditing)
        {
            AuditingList.Add(Auditing);
            _cache.InvalidateCaches();

            return true;
        }

        public async Task<List<IAuditingEvent>> GetAuditingList()
        {
            return AuditingList;
        }

        public async Task<List<IAuditingEvent>> GetAuditingById(Guid id)
        {
            return AuditingList.Where(x => x.Id == id).ToList();
        }
    }
}
