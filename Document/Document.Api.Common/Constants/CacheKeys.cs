using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Common.Constants
{
    public static class CacheKeys
    {
        public static string GetDocumentsCacheKey(int pageNumber, int pageSize, bool isDeleted) =>
            $"documents_page_{pageNumber}_size_{pageSize}_deleted_{isDeleted}";

        public static string GetDocumentCacheKey(Guid documentId) =>
            $"document_{documentId}";
    }
}
