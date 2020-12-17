using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionWithLinq
{
    class JobSearchCriteria
    {
        public string AssignedTo { get; set; }
        public Status? Status { get; set; }
    }
}
