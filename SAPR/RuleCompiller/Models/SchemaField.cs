using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Models
{
    public class SchemaField
    {
        public List<ShemaObject> ChildObjects { set; get; }
        /// <summary>
        /// Идентификатор объекта(Значение поля Id  в таблице)
        /// </summary>
        public List<ShemaObject> DescendingObjects
        {
            get            { };
        }
    }
}
