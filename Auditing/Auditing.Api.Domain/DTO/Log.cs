using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing.Api.Domain.DTO
{
    public class Log
    {
        public Guid? UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string RequestName { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string LogSeverity { get; set; } = "Information"; // string instead of enum
        public string LogType { get; set; } = "System";          // string instead of enum
        public string Metadata { get; set; } = string.Empty;
    }
}
