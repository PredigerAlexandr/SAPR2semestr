using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Validators
{
    public class ValidateEmail : ValidateRegex
    {
        public ValidateEmail(CodeTreeHandler codeTreeHandler, string propertyName, string errorHeader, string errorMessage) :
            base(codeTreeHandler, propertyName, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", errorHeader, errorMessage)
        {
        }

        public ValidateEmail(CodeTreeHandler codeTreeHandler, string propertyName) :
            this(codeTreeHandler, propertyName, null, "Значение свойства %FIELD_NAME% не является корректным адресом электронной почты.")
        {
            Pattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        }
    }
}
