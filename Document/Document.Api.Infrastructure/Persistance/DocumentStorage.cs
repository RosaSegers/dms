﻿using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Microsoft.Extensions.Caching.Memory;

namespace Document.Api.Infrastructure.Persistance
{
    public class DocumentStorage(ICacheService cache) : IDocumentStorage
    {
        private List<IDocumentEvent> documentList = new List<IDocumentEvent>();
        private readonly ICacheService _cache = cache;

        public async Task<bool> AddDocument(IDocumentEvent document)
        {
            documentList.Add(document);
            _cache.InvalidateCaches();

            return true;
        }

        public async Task<List<IDocumentEvent>> GetDocumentList()
        {
            return documentList;
        }

        public async Task<List<IDocumentEvent>> GetDocumentById(Guid id)
        {
            return documentList.Where(x => x.Id == id).ToList();
        }
    }
}
