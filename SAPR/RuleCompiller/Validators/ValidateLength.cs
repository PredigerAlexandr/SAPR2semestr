using Norbit.NBT.ActionService.Tender.Rules;
using RuleCompiller.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Validators
{
    public class ValidateLength : ValidationRule
    {
        public readonly int min;
        public readonly int max;

        public ValidateLength(CodeTreeHandler codeTreeHandler, string propertyName, int min, int max, string errorHeader, string errorMessage)
            : base(codeTreeHandler, propertyName, errorHeader, errorMessage)
        {
            this.min = min;
            this.max = max;
        }

        public ValidateLength(CodeTreeHandler codeTreeHandler, string propertyName, int min, int max)
            : this(codeTreeHandler, propertyName, min, max, null, string.Format("Значение свойства %FIELD_NAME% должно быть от {0} до {1} символов.", min, max))
        {
        }

        public override bool Match(ValidationData data)
        {
            string value = data[PropertyName];
            int length = value.Length;

            if (length >= this.min && length <= this.max) return true;
            ErrorMessage = ErrorMessage.Replace("%FIELD_NAME%", '"' + PropertyName + '"');
            return false;
        }

        protected override string CompileValidatorCode(string errorExpression = null, string successExpression = null)
        {
            if (string.IsNullOrEmpty(errorExpression)) errorExpression ="{" + codeTreeHandler.violationsVariableName + ".Add(" + CompileTool.BuildString(FormatErrors()) + ");}";
            
            StringBuilder codeText = new StringBuilder();
            codeText.Append("if(" + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) + "].Length <= " +
                min + " || " + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) +
                "].Length >= " + max);
            codeText.Append(")" + errorExpression);

            if (!CompileTool.IsUnuselessExpression(successExpression)) codeText.Append("else " + successExpression);

            codeTreeHandler.AddProperty(PropertyName);

            return codeText.ToString();
        }

    }
}
