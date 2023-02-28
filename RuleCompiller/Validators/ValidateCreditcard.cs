using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Validators
{
    class ValidateCreditcard : ValidateRegex
    {
        public ValidateCreditcard(CodeTreeHandler codeTreeHandler, string propertyName, string errorHeader, string errorMessage) :
            base(codeTreeHandler, propertyName, @"^((\d{4}[- ]?){3}\d{4})$", errorHeader, errorMessage)
        {
            ErrorMessage = errorMessage;
            ErrorHeader = errorHeader;
        }
        public ValidateCreditcard(CodeTreeHandler codeTreeHandler, string propertyName) :
            this(codeTreeHandler, propertyName, null, "Значение свойства %FIELD_NAME% не является корректным значением банковской карты.")
        {
            Pattern = @"^((\d{4}[- ]?){3}\d{4})$";
        }
    }
}
