using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAPR.Models
{
    public class Field
    {
        public int FieldId { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string DefaultValue { get; set; }
    }
}
