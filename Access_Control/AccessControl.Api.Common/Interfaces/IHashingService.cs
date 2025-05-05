using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessControl.Api.Common.Interfaces
{
    public interface IHashingService
    {
        public string Hash(string key);
        public bool Validate(string key, string hash);
    }
}
