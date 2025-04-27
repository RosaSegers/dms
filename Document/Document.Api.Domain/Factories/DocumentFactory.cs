using Document.Api.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Domain.Factories
{
    public static class DocumentFactory
    {
        public static Domain.Entities.Document FromEvents(IEnumerable<IDocumentEvent> events)
        {
            var doc = new Domain.Entities.Document();
            foreach (var evt in events.OrderBy(e => e.OccurredAt))
                doc.Apply(evt);

            return doc;
        }
    }
}
