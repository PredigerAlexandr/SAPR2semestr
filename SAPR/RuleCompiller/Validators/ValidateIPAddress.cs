using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Validators
{
    public class ValidateIPAddress : ValidateRegex
    {

        public ValidateIPAddress(CodeTreeHandler codeTreeHandler, string propertyName, string errorHeader, string errorMessage) :
            base(codeTreeHandler, propertyName, @"^([0-2]?[0-5]?[0-5]\.){3}[0-2]?[0-5]?[0-5]$", errorHeader, errorMessage)
        {
        }

        public ValidateIPAddress(CodeTreeHandler codeTreeHandler, string propertyName) :
            this(codeTreeHandler, propertyName, null, "Значение свойства %FIELD_NAME% не является корректным IP-адресом.")
        {
            this.Pattern = @"^([0-2]?[0-5]?[0-5]\.){3}[0-2]?[0-5]?[0-5]$";
        }
    }
}
