using Auditing.Api.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing.Api.Domain.Factories
{
    public static class AuditingFactory
    {
        public static Domain.Entities.Auditing FromEvents(IEnumerable<IAuditingEvent> events)
        {
            var doc = new Domain.Entities.Auditing();
            foreach (var evt in events.OrderBy(e => e.OccurredAt))
                doc.Apply(evt);

            return doc;
        }
    }
}
