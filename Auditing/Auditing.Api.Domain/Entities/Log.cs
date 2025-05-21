using Auditing.Api.Common.Enums;
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
        public Guid UserId { get; set; }
        public string Message { get; set; }
        public string RequestName { get; set; }
        public string RequestId { get; set; }
        public LogSeverity Severity { get; set; }
        public LogType Type { get; set; }
        public string Metadata { get; set; }
    }
}
