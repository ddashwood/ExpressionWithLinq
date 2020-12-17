using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionWithLinq
{
    enum Status
    {
        Received,
        Paid,
        Processing,
        Cancelled,
        Complete
    }

    class Job
    {
        public string JobNumber { get; set; }
        public string Description { get; set; }
        public string AssignedTo { get; set; }
        public Status Status { get; set; }
    }
}
