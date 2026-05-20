using System;
using System.Collections.Generic;
using System.Text;

namespace Easrms.Domain.Entities
{
    public class RequestEscalationHistory
    {
        public Guid EscalationId { get; set; }
        public Guid RequestId { get; set; }
        public Guid EscalatedBy { get; set; }
        public DateTime EscalatedOn { get; set; }
        public string EscalationReason { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }

        // Navigation
        public ServiceRequest Request { get; set; } = null!;
        public User EscalatedByUser { get; set; } = null!;
    }
}
