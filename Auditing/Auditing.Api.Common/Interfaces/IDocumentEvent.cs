using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing.Api.Common.Interfaces
{
    public interface IAuditingEvent
    {
        Guid Id { get; }
        DateTime OccurredAt { get; }
        float? Version { get; }
    }
}
