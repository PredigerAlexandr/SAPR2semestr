using Norbit.NBT.ActionService.Tender.Rules;
using RuleCompiller.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Validators
{
    public class ValidateValuesIn : ValidationRule
    {
        public string[] values;
        public ValidateValuesIn(CodeTreeHandler codeTreeHandler, string propertyName, string[] values)
            : base(codeTreeHandler, propertyName, null, "Свойство %FIELD_NAME% должно быть определенным значением.")
        {
            this.values = values;
        }

        public ValidateValuesIn(CodeTreeHandler codeTreeHandler, string propertyName, string errorHeader, string errorMessage, string[] values)
            : base(codeTreeHandler, propertyName, errorHeader, errorMessage)
        {
            this.values = values;
        }

        public override bool Match(ValidationData data)
        {
            string propValue = data[PropertyName];

            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i];
                if (value.StartsWith("field:"))
                {
                    string[] tempParts = value.Split(':');
                    if (tempParts.Length > 1) value = data[tempParts[1]];
                }
                if (propValue == value) return true;
            }

            ErrorMessage = ErrorMessage.Replace("%FIELD_NAME%", '"' + PropertyName + '"');
            return false;
        }

        const string XML_FIELD_PREFIX = "field:";

        protected override string CompileValidatorCode(string errorExpression = null, string successExpression = null)
        {
            if (string.IsNullOrEmpty(errorExpression)) errorExpression ="{" + codeTreeHandler.violationsVariableName + ".Add(" + CompileTool.BuildString(FormatErrors()) + ");}";

            StringBuilder codeText = new StringBuilder();
            string variableName = codeTreeHandler.GetUniqueVariableName(PropertyName+"String");

            codeText.Append("string " + variableName + " = " + codeTreeHandler.validationDataVariableName + '[' + CompileTool.BuildString(PropertyName) + "];");

            codeText.Append("if(");
            for (int j = 0; j < values.Length; j++)
            {
                if (j > 0) codeText.Append(" && ");
                codeText.Append(variableName+" != ");
                if (values[j].Contains(XML_FIELD_PREFIX))
                {
                    string tmpPropertyName = values[j].Replace(XML_FIELD_PREFIX, "");
                    codeText.Append(codeTreeHandler.validationDataVariableName+'[' + CompileTool.BuildString(tmpPropertyName) + "]");
                    codeTreeHandler.AddProperty(tmpPropertyName);
                }
                else
                {
                    codeText.Append(CompileTool.BuildString(values[j]));
                }
            }
            codeText.Append(") "+ errorExpression);


            if (!CompileTool.IsUnuselessExpression(successExpression)) codeText.Append("else " + successExpression);

            codeTreeHandler.AddProperty(PropertyName);

            return codeText.ToString();
        }
    }
}
