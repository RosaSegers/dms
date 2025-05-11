using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing.Api.Domain.Entities
{
    public class Log
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Message { get; set; }
        public string RequestName { get; set; }
        public string RequestId { get; set; }
        public string Severity { get; set; }
        public string Metadata { get; set; }
    }
}
