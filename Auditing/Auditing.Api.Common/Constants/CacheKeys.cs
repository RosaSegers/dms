using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing.Api.Common.Constants
{
    public static class CacheKeys
    {
        public static string GetAuditingsCacheKey(int pageNumber, int pageSize, bool isDeleted) =>
            $"Auditings_page_{pageNumber}_size_{pageSize}_deleted_{isDeleted}";

        public static string GetAuditingCacheKey(Guid AuditingId) =>
            $"Auditing_{AuditingId}";
    }
}
