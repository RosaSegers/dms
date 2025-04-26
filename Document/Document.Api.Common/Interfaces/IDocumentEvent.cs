using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Common.Interfaces
{
    public interface IDocumentEvent
    {
        Guid Id { get; }
        DateTime OccurredAt { get; }
        float? Version { get; }
    }
}
