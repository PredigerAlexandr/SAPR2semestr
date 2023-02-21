using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAPR.Models
{
    public class Rule
    {
        public long Id { get; set; }
        public long PurchaseId { get; set; }
        public bool Stage { get; set; }
        public string RuleText { get; set; }
    }
}
