using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAPR.Models
{
    public class Rule
    {
        public long RuleId { get; set; }
        public long PurchaseId { get; set; }
        public string Stage { get; set; }
        public string RuleText { get; set; }
    }
}
