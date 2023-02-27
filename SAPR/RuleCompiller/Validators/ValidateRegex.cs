using Norbit.NBT.ActionService.Tender.Rules;
using RuleCompiller.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RuleCompiller.Validators
{
    public class ValidateRegex : ValidationRule
    {
        public string Pattern { get; set; }

        public ValidateRegex(CodeTreeHandler codeTreeHandler, string propertyName, string pattern, string errorHeader, string errorMessage)
            : base(codeTreeHandler, propertyName, errorHeader, errorMessage)
        {
            Pattern = pattern;
        }

        public ValidateRegex(CodeTreeHandler codeTreeHandler, string propertyName, string pattern)
            : this(codeTreeHandler, propertyName, pattern, null, string.Format("Значение поля %FIELD_NAME% имеет неверный формат."))
        {
        }

        public override bool Match(ValidationData data)
        {
            string propValue = data[PropertyName];

            if (string.IsNullOrEmpty(propValue) || Regex.Match(propValue, Pattern).Success) return true;
            ErrorMessage = ErrorMessage.Replace("%FIELD_NAME%", '"' + PropertyName + '"');
            return false;
        }

        protected override string CompileValidatorCode(string errorExpression = null, string successExpression = null)
        {
            if (string.IsNullOrEmpty(errorExpression)) errorExpression = "{" + codeTreeHandler.violationsVariableName + ".Add(" + CompileTool.BuildString(FormatErrors()) + ");}";

            StringBuilder codeText = new StringBuilder();
            string variableName = codeTreeHandler.GetUniqueVariableName(PropertyName);
            codeText.Append("string " + variableName + " = " + codeTreeHandler.validationDataVariableName + '[' + CompileTool.BuildString(PropertyName) + "];" );
            codeText.Append("if(!string.IsNullOrEmpty("+ variableName + ") && Regex.Match(" + variableName + ',' + CompileTool.BuildString(Pattern)+ ").Success == false"); 
            codeText.Append(")" + errorExpression);

            if (!CompileTool.IsUnuselessExpression(successExpression)) codeText.Append("else " + successExpression);

            codeTreeHandler.AddProperty(PropertyName);

            return codeText.ToString();
        }
    }
}
