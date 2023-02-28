//using Norbit.NBT.ActionService.Tender.Rules;
//using Norbit.ValidationFramework.DataTransform;
using RuleCompiller.Validators;
using ServiceStack.OrmLite.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Validators
{
    public class ValidateType : ValidationRule
    {
        private static string[] _availableTypes = { "integer", "long", "numeric", "time", "date", "datetime", "currency" };

        public static bool CheckIfTypeValid(string type)
        {
            if (_availableTypes.Contains(type))
            {
                return true;
            }
            return false;
        }

        public string validateType { get; set; }

        public ValidateType(CodeTreeHandler codeTreeHandler, string propertyName, string validateType, string errorHeader, string errorMessage)
            : base(codeTreeHandler, propertyName, errorHeader, errorMessage)
        {
            validateType = validateType;
        }

        public ValidateType(CodeTreeHandler codeTreeHandler, string propertyName, string validateType)
            : this(codeTreeHandler, propertyName, validateType, null, "Поле %FIELD_NAME% имеет неверный формат")
        {
        }

        //TODO: адаптировать ValueTypeConverter.Convert 
        public override bool Match(ValidationData data)
        {
            ErrorMessage = ErrorMessage.Replace("%FIELD_NAME%", '"' + PropertyName + '"');
            string value = data[PropertyName];

            if (string.IsNullOrWhiteSpace(value)) return true;

            //Дабы сошлось в классе ValueTypeConverter
            switch (validateType)
            {
                //case "datetime":
                //    _validateType = "date";
                //    break;
                case "date":
                    validateType = "dateonly";
                    break;
            }

            //try
            //{
            //    ValueTypeConverter.Convert(value, validateType);
            //    return true;
            //}
            //catch (InvalidCastException)
            //{
            //    return false;
            //}
            return true;
        }


        protected override string CompileValidatorCode(string errorExpression = null, string successExpression = null)
        {
            if (string.IsNullOrEmpty(errorExpression)) errorExpression = codeTreeHandler.violationsVariableName + ".Add(" + CompileTool.BuildString(FormatErrors()) + ");";

            StringBuilder codeText = new StringBuilder();

            string outVariableName = codeTreeHandler.GetUniqueVariableName("outVariableName");

            codeText.Append("{if(!");

            switch (validateType)
            {
                case "integer":
                    codeText.Append("int.TryParse("+ codeTreeHandler.validationDataVariableName +"[" + CompileTool.BuildString(PropertyName) + "], out int "+ outVariableName + ")");
                    break;
                case "long":
                    codeText.Append("Int64.TryParse(" + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) + "], out long " + outVariableName + ")");
                    break;
                case "numeric":
                    codeText.Append("int.TryParse(" + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) + "], out int " + outVariableName + ")");
                    break;
                case "time":
                    codeText.Append("DataTime.TryParse(" + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) + "], out DataTime " + outVariableName + ")");
                    break;
                case "datetime":
                    codeText.Append("DataTime.TryParse(" + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) + "], out DataTime " + outVariableName + ")");
                    break;
                //TODO: неизвестно какой тип данных имеет валюта (currency), предположительно double
                case "currency":
                    codeText.Append("double.TryParse(" + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) + "], out double " + outVariableName + ")");
                    break;
            }

            codeText.Append(")" + errorExpression);
            codeText.Append("}");
            if (!CompileTool.IsUnuselessExpression(successExpression)) codeText.Append("else " + successExpression);

            codeTreeHandler.AddProperty(PropertyName);

            return codeText.ToString();
        }



    }
}
