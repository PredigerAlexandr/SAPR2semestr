using Norbit.NBT.ActionService.Tender.Rules;
using RuleCompiller.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Validators
{
    public class ValidateCompare : ValidationRule
    {
        public string OtherPropertyName { get; set; }
        public ValidationDataType DataType { get; set; }
        public ValidationOperator Operator { get; set; }
        private const string XML_VALUE_PREFIX = "value:";

        public ValidateCompare(CodeTreeHandler codeTreeHandler, string propertyName, string otherPropertyName, ValidationOperator validationOperator, ValidationDataType dataType)
            : this(codeTreeHandler, propertyName, otherPropertyName, validationOperator, dataType, null, string.Format("Значение свойства %FIELD_NAME% должно быть {0} %OTHER_FIELD_NAME%!", validationOperator.GetName()))
        {

            OtherPropertyName = otherPropertyName;
            Operator = validationOperator;
            DataType = dataType;
        }

        public ValidateCompare(CodeTreeHandler codeTreeHandler, string propertyName, string otherPropertyName,
            ValidationOperator @operator, ValidationDataType dataType, string errorHeader, string errorMessage)
            : base(codeTreeHandler, propertyName, errorHeader, errorMessage)
        {
            OtherPropertyName = otherPropertyName;
            Operator = @operator;
            DataType = dataType;
        }

        public override bool Match(ValidationData data)
        {
            string firstValue = data[PropertyName];
            string secondValue = OtherPropertyName.Contains(XML_VALUE_PREFIX) ? OtherPropertyName.Substring(XML_VALUE_PREFIX.Length) : data[OtherPropertyName];
            ErrorMessage = ErrorMessage.Replace("%FIELD_NAME%", PropertyName);
            ErrorMessage = ErrorMessage.Replace("%OTHER_FIELD_NAME%", OtherPropertyName.Contains(XML_VALUE_PREFIX) ? OtherPropertyName.Substring(XML_VALUE_PREFIX.Length) : OtherPropertyName);

            try
            {
                if (string.IsNullOrEmpty(firstValue) && string.IsNullOrEmpty(secondValue)) return true;

                switch (DataType)
                {
                    case ValidationDataType.Integer:

                        int intValue1 = int.Parse(firstValue.ToString());
                        int intValue2 = int.Parse(secondValue.ToString());

                        switch (Operator)
                        {
                            case ValidationOperator.Equal: return intValue1 == intValue2;
                            case ValidationOperator.NotEqual: return intValue1 != intValue2;
                            case ValidationOperator.GreaterThan: return intValue1 > intValue2;
                            case ValidationOperator.GreaterThanEqual: return intValue1 >= intValue2;
                            case ValidationOperator.LessThan: return intValue1 < intValue2;
                            case ValidationOperator.LessThanEqual: return intValue1 <= intValue2;
                        }
                        break;

                    case ValidationDataType.Double:

                        double doubleValue1 = double.Parse(firstValue.ToString());
                        double doubleValue2 = double.Parse(secondValue.ToString());

                        switch (Operator)
                        {
                            case ValidationOperator.Equal: return doubleValue1 == doubleValue2;
                            case ValidationOperator.NotEqual: return doubleValue1 != doubleValue2;
                            case ValidationOperator.GreaterThan: return doubleValue1 > doubleValue2;
                            case ValidationOperator.GreaterThanEqual: return doubleValue1 >= doubleValue2;
                            case ValidationOperator.LessThan: return doubleValue1 < doubleValue2;
                            case ValidationOperator.LessThanEqual: return doubleValue1 <= doubleValue2;
                        }
                        break;

                    case ValidationDataType.Decimal:

                        decimal decimalValue1 = decimal.Parse(firstValue.ToString());
                        decimal decimalValue2 = decimal.Parse(secondValue.ToString());

                        switch (Operator)
                        {
                            case ValidationOperator.Equal: return decimalValue1 == decimalValue2;
                            case ValidationOperator.NotEqual: return decimalValue1 != decimalValue2;
                            case ValidationOperator.GreaterThan: return decimalValue1 > decimalValue2;
                            case ValidationOperator.GreaterThanEqual: return decimalValue1 >= decimalValue2;
                            case ValidationOperator.LessThan: return decimalValue1 < decimalValue2;
                            case ValidationOperator.LessThanEqual: return decimalValue1 <= decimalValue2;
                        }
                        break;

                    case ValidationDataType.Date:

                        DateTime dateTimeValue1 = DateTime.Parse(firstValue.ToString());
                        DateTime dateTimeValue2 = DateTime.Parse(secondValue.ToString());

                        switch (Operator)
                        {
                            case ValidationOperator.Equal: return dateTimeValue1 == dateTimeValue2;
                            case ValidationOperator.NotEqual: return dateTimeValue1 != dateTimeValue2;
                            case ValidationOperator.GreaterThan: return dateTimeValue1 > dateTimeValue2;
                            case ValidationOperator.GreaterThanEqual: return dateTimeValue1 >= dateTimeValue2;
                            case ValidationOperator.LessThan: return dateTimeValue1 < dateTimeValue2;
                            case ValidationOperator.LessThanEqual: return dateTimeValue1 <= dateTimeValue2;
                        }
                        break;

                    case ValidationDataType.String:

                        int result = string.Compare(firstValue.ToString(), secondValue.ToString(), StringComparison.CurrentCulture);

                        switch (Operator)
                        {
                            case ValidationOperator.Equal: return result == 0;
                            case ValidationOperator.NotEqual: return result != 0;
                            case ValidationOperator.GreaterThan: return result > 0;
                            case ValidationOperator.GreaterThanEqual: return result >= 0;
                            case ValidationOperator.LessThan: return result < 0;
                            case ValidationOperator.LessThanEqual: return result <= 0;
                        }
                        break;

                    case ValidationDataType.Bit:

                        Boolean booleanValue1 = Boolean.Parse(firstValue.ToString());
                        Boolean booleanValue2 = Boolean.Parse(secondValue.ToString());

                        switch (Operator)
                        {
                            case ValidationOperator.Equal: return booleanValue1 == booleanValue2;
                            case ValidationOperator.NotEqual: return booleanValue1 != booleanValue2;
                            case ValidationOperator.GreaterThan: return booleanValue1 != booleanValue2;
                            case ValidationOperator.GreaterThanEqual: return booleanValue1 == booleanValue2;
                            case ValidationOperator.LessThan: return booleanValue1 != booleanValue2;
                            case ValidationOperator.LessThanEqual: return booleanValue1 == booleanValue2;
                        }
                        break;

                }
            }
            catch { }

            return false;
        }

        protected override string CompileValidatorCode(string errorExpression = null, string successExpression = null)
        {
            StringBuilder codeText = new StringBuilder();
            if (string.IsNullOrEmpty(errorExpression)) errorExpression = "{" + codeTreeHandler.violationsVariableName + ".Add(" + CompileTool.BuildString(FormatErrors()) + ");}";

            //init base values
            string firstValueStringVariableName = codeTreeHandler.GetUniqueVariableName("firstValueString");
            string secondValueStringVariableName = codeTreeHandler.GetUniqueVariableName("secondValueString");
            codeText.Append("string " + firstValueStringVariableName + " = " + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) + "];");
            codeText.Append("string " + secondValueStringVariableName + " = ");

            OtherPropertyName = OtherPropertyName.Trim();

            if (OtherPropertyName.Contains(XML_VALUE_PREFIX))
            {
                codeText.Append(CompileTool.BuildString(OtherPropertyName.Replace(XML_VALUE_PREFIX, "")) + ';');
            }
            else
            {
                codeText.Append(codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(OtherPropertyName) + "];");
            }

            codeText.Append("if (!(string.IsNullOrEmpty(" + firstValueStringVariableName + ") && string.IsNullOrEmpty(" + secondValueStringVariableName + ")) && (");

            //codeText.Append("if(");

            string outFirstValueVariableName = codeTreeHandler.GetUniqueVariableName("firstValue");
            string outSecondValueVariableName = codeTreeHandler.GetUniqueVariableName("secondValue");

           
            switch (DataType)
            {
                case ValidationDataType.Integer:
                    codeText.Append(
                        "!(int.TryParse(" + firstValueStringVariableName + ", out int "+ outFirstValueVariableName + ") && int.TryParse(" + secondValueStringVariableName + ", out int " + outSecondValueVariableName + ")) || !(" + outFirstValueVariableName + " " +
                        GetOperatorForCompare(DataType, Operator) +
                        " " + outSecondValueVariableName + ")"
                    );
                    break;
                case ValidationDataType.Double:
                    codeText.Append(
                        "(!double.TryParse(" + firstValueStringVariableName + ", out double " + outFirstValueVariableName + ") || !double.TryParse(" + secondValueStringVariableName + ", out double " + outSecondValueVariableName + ") || !( " + outFirstValueVariableName + " " +
                        GetOperatorForCompare(DataType, Operator) +
                        " " + outSecondValueVariableName + ")"
                    );
                    break;
                case ValidationDataType.Decimal:
                    codeText.Append(
                        "(!decimal.TryParse(" + firstValueStringVariableName + ", out decimal " + outFirstValueVariableName + ") || !decimal.TryParse(" + secondValueStringVariableName + ", out decimal " + outSecondValueVariableName + ") || !( " + outFirstValueVariableName + " " +
                        GetOperatorForCompare(DataType, Operator) +
                        " " + outSecondValueVariableName + ")"
                    );
                    break;
                case ValidationDataType.Date:
                    codeText.Append(
                        "(!DateTime.TryParse(" + firstValueStringVariableName + ", out DateTime " + outFirstValueVariableName + ") || !DateTime.TryParse(" + secondValueStringVariableName + ", out DateTime " + outSecondValueVariableName + ") || !(" + outFirstValueVariableName + " " +
                        GetOperatorForCompare(DataType, Operator) +
                        " " + outSecondValueVariableName + ")"
                    );
                    break;
                case ValidationDataType.Bit:
                    codeText.Append(
                        "(!Boolean.TryParse(" + firstValueStringVariableName + ", out bool " + outFirstValueVariableName + ") || !Boolean.TryParse(" + secondValueStringVariableName + ", out bool " + outSecondValueVariableName + ") || !( " + outFirstValueVariableName + " " +
                        GetOperatorForCompare(DataType, Operator) +
                        " " + outFirstValueVariableName + ")"
                    );
                    break;
                case ValidationDataType.String:
                    codeText.Append(
                        "!(string.Compare(" + firstValueStringVariableName + ", " + secondValueStringVariableName + ", StringComparison.CurrentCulture) " +
                        GetOperatorForCompare(DataType, Operator) +
                        " 0)"
                    );
                    break;
                default:
                    throw new ApplicationException("Неверно указан тип данных поля.");
            }

            codeText.Append("))" + errorExpression);
            if (!CompileTool.IsUnuselessExpression(successExpression)) codeText.Append("else " + successExpression);
            
            codeTreeHandler.AddProperty(PropertyName);
            if (!OtherPropertyName.Contains(XML_VALUE_PREFIX)) codeTreeHandler.AddProperty(OtherPropertyName.Replace(XML_VALUE_PREFIX, ""));
            
            return codeText.ToString();
        }

        public override string FormatErrors()
        {
            return base.FormatErrors().Replace("%OTHER_FIELD_NAME%", OtherPropertyName.Replace(XML_VALUE_PREFIX, ""));
        }

        protected string GetOperatorForCompare(ValidationDataType type, ValidationOperator comapreOperator)
        {
            switch (comapreOperator)
            {
                case ValidationOperator.Equal:
                    return "==";
                case ValidationOperator.GreaterThan:
                    return (type == ValidationDataType.Bit) ? "!=" : ">";
                case ValidationOperator.GreaterThanEqual:
                    return (type == ValidationDataType.Bit) ? "==" : ">=";
                case ValidationOperator.LessThan:
                    return (type == ValidationDataType.Bit) ? "!=" : "<";
                case ValidationOperator.LessThanEqual:
                    return (type == ValidationDataType.Bit) ? "==" : "<=";
            }
            throw new ApplicationException("Некорректный оператор");
        }
    }
}
