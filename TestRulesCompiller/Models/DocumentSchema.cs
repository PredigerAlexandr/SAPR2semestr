using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRulesCompiler.Models
{
    class DocumentSchema
    {
        public class ObjectListTable
        {
            public bool IsNotRemove { set; get; }//
            public bool IsReadOnly { set; get; }//
            public string Name { set; get; }//
            public string Code { set; get; }//
            public string Description { set; get; }//
            public string Object { set; get; }//
            public string DataTypeId { set; get; }//
            public string DefaultValue { set; get; }//
            public bool IsRequired { set; get; }//
            public bool IsBaseObject { set; get; }//
            public bool IsServiceObject { set; get; }//
            public bool IsCrossObject { set; get; }//
            public bool DeleteIfEmpty { set; get; }//
            public bool IsTemporary { set; get; }//
            public bool IsUnique { set; get; }//
            public string UniqueKey { set; get; }//
            public bool DoNotSaveToDocumentObject { set; get; }//
        }
    }
}
