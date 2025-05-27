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
        public Guid? UserId { get; set; } = Guid.Empty;
        public string Message { get; set; } = string.Empty;
        public string RequestName { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public LogSeverity Severity { get; set; } = LogSeverity.Information;
        public LogType Type { get; set; } = LogType.System;
        public string Metadata { get; set; } = string.Empty;
    }
}
