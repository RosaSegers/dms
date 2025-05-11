using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing.Api.Common.Interfaces
{
    public interface ICacheService
    {
        void SetCache(string key, object value);
        bool TryGetCache(string key, out object value);
        void InvalidateCaches();
    }
}
