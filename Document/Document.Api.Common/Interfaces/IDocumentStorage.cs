using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Common.Interfaces
{
    public interface IDocumentStorage
    {
        Task<bool> AddDocument(IDocumentEvent document);
        Task<List<IDocumentEvent>> GetDocumentList();
        Task<List<IDocumentEvent>> GetDocumentById(Guid id);
    }
}
