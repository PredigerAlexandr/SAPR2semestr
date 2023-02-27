using RuleCompiller.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Validators
{
    public class ValidateEmpty : ValidationRule
    {
        public ValidateEmpty(CodeTreeHandler codeTreeHandler, string propertyName, string errorHeader = null, string errorMessage = "Значение свойства %FIELD_NAME% должно быть пустым.")
            : base(codeTreeHandler, propertyName, errorHeader, errorMessage)
        {
        }

        public override bool Match(ValidationData data)
        {
            string value = data[PropertyName];

            if (string.IsNullOrEmpty(value) && string.IsNullOrWhiteSpace(value)) return true;
            ErrorMessage = ErrorMessage.Replace("%FIELD_NAME%", PropertyName);
            return false;

        }

        protected override string CompileValidatorCode(string errorExpression = null, string successExpression = null)
        {
            StringBuilder codeText = new StringBuilder();
            if (string.IsNullOrEmpty(errorExpression)) errorExpression = "{" + codeTreeHandler.violationsVariableName + ".Add(" + CompileTool.BuildString(FormatErrors()) + ");}";

            codeText.Append(
                "if(!string.IsNullOrEmpty(" + 
                codeTreeHandler.validationDataVariableName + 
                "[" + CompileTool.BuildString(PropertyName) + "]))" + 
                errorExpression
            );

            if (!CompileTool.IsUnuselessExpression(successExpression)) codeText.Append("else " + successExpression);

            codeTreeHandler.AddProperty(PropertyName);
            return codeText.ToString();
        }

    }
}
