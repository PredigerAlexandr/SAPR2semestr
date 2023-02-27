using RuleCompiller.Validators;
using RuleCompiller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Validators
{
    /// <summary>
    /// Правило для проверки заполнения указанного свойства объекта.
    /// </summary>
    public class ValidateRequired : ValidationRule
    {
        public ValidateRequired(CodeTreeHandler codeTreeHandler, string propertyName, string errorHeader = null, string errorMessage = "Свойство %FIELD_NAME% обязательно для заполнения.")
            : base(codeTreeHandler, propertyName, errorHeader, errorMessage)
        {
        }

        public override bool Match(ValidationData data)
        {
            string value = data[PropertyName];
            
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value)) return true;
            ErrorMessage = ErrorMessage.Replace("%FIELD_NAME%",  PropertyName);
            return false;
        }

        protected override string CompileValidatorCode(string errorExpression = null, string successExpression = null)
        {
            StringBuilder codeText = new StringBuilder();
            if (string.IsNullOrEmpty(errorExpression)) errorExpression ="{" + codeTreeHandler.violationsVariableName + ".Add(" + CompileTool.BuildString(FormatErrors()) + ");}";

            codeText.Append(
                "if(string.IsNullOrEmpty(" +
                codeTreeHandler.validationDataVariableName +
                "[" + CompileTool.BuildString(PropertyName) + "]) && string.IsNullOrWhiteSpace(" + codeTreeHandler.validationDataVariableName +"["+ CompileTool.BuildString(PropertyName) + "]))" +
                errorExpression
            );

            if (!CompileTool.IsUnuselessExpression(successExpression)) codeText.Append("else " + successExpression);

            codeTreeHandler.AddProperty(PropertyName);
            return codeText.ToString();
        }
    }
}
