using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Infrastructure.Services.Interface
{
    public interface IDocumentStatusService
    {
        Task<string> GetStatusAsync(Guid documentId);
        Task SetStatusAsync(Guid documentId, string status);
    }

}
