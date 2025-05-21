using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing.Api.Common.Interfaces
{
    public interface IAuditingStorage
    {
        Task<bool> AddAuditing(IAuditingEvent Auditing);
        Task<List<IAuditingEvent>> GetAuditingList();
        Task<List<IAuditingEvent>> GetAuditingById(Guid id);
    }
}
