using RuleCompiller.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Linq.Dynamic.ValidationException;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Validators
{
    public class ValidateRange : ValidationRule
    {
        public ValidationDataType DataType { get; set; }
        private const string XML_FIELD_PREFIX = "field:";

        public string Min { get; set; }
        public string Max { get; set; }
        public bool WithLeftBorder { get; set; }
        public bool WithRightBorder { get; set; }

        public ValidateRange(CodeTreeHandler codeTreeHandler, string propertyName, string min, string max, ValidationDataType dataType, string errorHeader, string errorMessage)
            : this(codeTreeHandler, propertyName, min, max, dataType, true, true, errorHeader, errorMessage)
        {
        }

        public ValidateRange(CodeTreeHandler codeTreeHandler, string propertyName, string min, string max, ValidationDataType dataType, bool withLeft, bool withRight, string errorHeader, string errorMessage)
            : base(codeTreeHandler, propertyName, errorHeader, errorMessage)
        {
            Min = min;
            Max = max;
            DataType = dataType;
            WithLeftBorder = withLeft;
            WithRightBorder = withRight;
        }

        public ValidateRange(CodeTreeHandler codeTreeHandler, string propertyName, string min, string max, ValidationDataType dataType, bool withLeft, bool withRight)
            : this(codeTreeHandler, propertyName, min, max, dataType, withLeft, withRight, null, string.Format("Значение свойства %FIELD_NAME% должно быть между %MINVALUE% и %MAXVALUE%."))
        {
        }

        public ValidateRange(CodeTreeHandler codeTreeHandler, string propertyName, string min, string max, ValidationDataType dataType)
            : this(codeTreeHandler, propertyName, min, max, dataType, true, true)
        {
        }


        private string GetFixedValue(ValidationData data, string value)
        {
            if (value.Contains(XML_FIELD_PREFIX))
            {
                // возвращаем значение указанного поля из объекта.
                //return GetPropertyValue(target, value.Replace(XML_FIELD_PREFIX, ""));
                return data[value.Replace(XML_FIELD_PREFIX, "")];
            }
            return value;
        }

        //TODO: переработка если есть XML_FIELD_PREFIX удалить его! + если minString/maxString = null то пустая строка
        //Необходимо избавиться от GetFixedValue?
        //Избавиться от switch case преобразований min max преобразовать в данные dynamic
        public override bool Match(ValidationData data)
        {

            ErrorMessage = ErrorMessage.Replace("%FIELD_NAME%", PropertyName);
            ErrorMessage = ErrorMessage.Replace("%MINVALUE%", Min.Contains(XML_FIELD_PREFIX) ? Min.Substring(XML_FIELD_PREFIX.Length) : Min);
            ErrorMessage = ErrorMessage.Replace("%MAXVALUE%", Max.Contains(XML_FIELD_PREFIX) ? Max.Substring(XML_FIELD_PREFIX.Length) : Max);

            try
            {
                string value = data[PropertyName];

                // Дополнительно условие по требованию: "если поле не заполнено, то не поднимать сообщение об ошибке".
                if (string.IsNullOrEmpty(value))
                    return true;

                switch (DataType)
                {
                    case ValidationDataType.Integer:
                        {
                            int imin = Min == null ? int.MinValue : int.Parse(GetFixedValue(data, Min.ToString()).ToString());
                            int imax = Max == null ? int.MaxValue : int.Parse(GetFixedValue(data, Max.ToString()).ToString());
                            int ival = int.Parse(value);
                            return (ival > imin && ival < imax) ||
                                (WithLeftBorder && ival == imin) ||
                                (WithRightBorder && ival == imax);
                        }
                    case ValidationDataType.Double:
                        {
                            double dmin = Min == null ? double.MinValue : double.Parse(GetFixedValue(data, Min.ToString()).ToString());
                            double dmax = Max == null ? double.MaxValue : double.Parse(GetFixedValue(data, Max.ToString()).ToString());
                            double dval = double.Parse(value);
                            return (dval > dmin && dval < dmax) ||
                                (WithLeftBorder && dval == dmin) ||
                                (WithRightBorder && dval == dmax);
                        }
                    case ValidationDataType.Decimal:
                        {
                            decimal cmin = Min == null ? decimal.MinValue : decimal.Parse(GetFixedValue(data, Min.ToString()).ToString());
                            decimal cmax = Max == null ? decimal.MaxValue : decimal.Parse(GetFixedValue(data, Max.ToString()).ToString());
                            decimal cval = decimal.Parse(value);
                            return (cval > cmin && cval < cmax) ||
                                (WithLeftBorder && cval == cmin) ||
                                (WithRightBorder && cval == cmax);
                        }
                    case ValidationDataType.Date:
                        {
                            DateTime tmin = Min == null ? DateTime.MinValue : DateTime.Parse(GetFixedValue(data, Min.ToString()).ToString());
                            DateTime tmax = Max == null ? DateTime.MaxValue : DateTime.Parse(GetFixedValue(data, Max.ToString()).ToString());
                            DateTime tval = DateTime.Parse(value);
                            return (tval > tmin && tval < tmax) ||
                                (WithLeftBorder && tval == tmin) ||
                                (WithRightBorder && tval == tmax);
                        }
                    case ValidationDataType.String:
                        {
                            string smin = GetFixedValue(data, Min);
                            string smax = GetFixedValue(data, Max);

                            int result1 = string.Compare(smin, value);
                            int result2 = string.Compare(value, smax);

                            return (result1 < 0 && result2 < 0) ||
                                (WithLeftBorder && result1 == 0) ||
                                (WithRightBorder && result2 == 0);
                        }
                }
            }
            catch (Exception ex)
            {
            }

            return false;
        }

        protected override string CompileValidatorCode(string errorExpression = null, string successExpression = null)
        {
            if (string.IsNullOrEmpty(errorExpression)) errorExpression =  "{" + codeTreeHandler.violationsVariableName + ".Add(" + CompileTool.BuildString(FormatErrors()) + ");}";


            StringBuilder codeText = new StringBuilder();

            bool minIsField = false, maxIsField = false;

            if (!string.IsNullOrEmpty(Min))
            {
                Min = Min.Trim();
                if (Min.Contains(XML_FIELD_PREFIX))
                {
                    Min = Min.Replace(XML_FIELD_PREFIX, "");
                    minIsField = true;
                }
            }
            else Min = "";

            if (!string.IsNullOrEmpty(Max))
            {
                Max = Max.Trim();
                if (Max.Contains(XML_FIELD_PREFIX))
                {
                    Max = Max.Replace(XML_FIELD_PREFIX, "");
                    maxIsField = true;
                }
            }
            else Max = "";

            if (Max == "" && Min == "") return "";

            codeText.Append("if(");

            //Build expression
            string compareExpression = "";
            string outValueProp = codeTreeHandler.GetUniqueVariableName(PropertyName);
            switch (DataType)
            {
                case ValidationDataType.Integer:
                    codeText.Append("!int.TryParse(" + codeTreeHandler.validationDataVariableName + '[' + CompileTool.BuildString(PropertyName) + "], out int " + outValueProp + ")");
                    break;
                case ValidationDataType.Double:
                    codeText.Append("!double.TryParse(" + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) + "], out double " + outValueProp + ")");
                    break;
                case ValidationDataType.Decimal:
                    codeText.Append("!decimal.TryParse(" + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) + "], out decimal " + outValueProp + ")");
                    break;
                case ValidationDataType.Date:
                    codeText.Append("!DateTime.TryParse(" + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) + "], out DataTime " + outValueProp + ")");
                    break;
                case ValidationDataType.String:
                    codeText.Append("!String.TryParse(" + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) + "], out string " + outValueProp + ")");
                    break;
                default:
                    throw new ApplicationException("Неверно указан тип данных поля.");
            }
            codeText.Append("||");
            if (DataType != ValidationDataType.String)
            {
                if (Min != "")
                {
                    compareExpression += outValueProp + (WithLeftBorder ? "<=" : "<");
                    compareExpression += (minIsField) ? codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(Min) + "]" : GetValue(DataType, Min);
                }
                if (Max != "")
                {
                    if (compareExpression != "") compareExpression += "||";
                    compareExpression += outValueProp + (WithRightBorder ? ">=" : ">");
                    compareExpression += (maxIsField) ? codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(Max) + "]" : GetValue(DataType, Max);
                }
            }
            else
            {
                if (Min != "")
                {
                    compareExpression += "!(string.Compare(" + CompileTool.BuildString(Min) + ", " + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) + "])";
                    compareExpression += (WithLeftBorder ? "<= 0" : "< 0") + ')';
                }
                if (Max != "")
                {
                    if (compareExpression != "") compareExpression += "||";
                    compareExpression += "!(string.Compare(" + codeTreeHandler.validationDataVariableName + "[" + CompileTool.BuildString(PropertyName) + "], " + CompileTool.BuildString(Max) + ")";
                    compareExpression += (WithRightBorder ? "<= 0" : "< 0") +')';
                }
            }

            codeText.Append(compareExpression);

            codeText.Append(") " + errorExpression);

            if (!CompileTool.IsUnuselessExpression(successExpression)) codeText.Append("else " + successExpression);

            codeTreeHandler.AddProperty(PropertyName);
            if (minIsField) codeTreeHandler.AddProperty(Min);
            if (maxIsField) codeTreeHandler.AddProperty(Max);

            return codeText.ToString();
        }

        public override string FormatErrors()
        {
            return base.FormatErrors().Replace("%MINVALUE%", Min).Replace("%MAXVALUE%", Max);
        }

        private string GetValue(ValidationDataType type, string value)
        {
            try
            {
                switch (type)
                {
                    case ValidationDataType.Integer:
                        return int.Parse(value) + "";
                    case ValidationDataType.Double:
                        return double.Parse(value) + "d";
                    case ValidationDataType.Decimal:
                        return decimal.Parse(value) + "M";
                    case ValidationDataType.Date:
                        DateTime date = DateTime.Parse(value);
                        return "DateTime.Parse(" + CompileTool.BuildString(value) + ")";
                    case ValidationDataType.Bit:
                        return Boolean.Parse(value) ? "true" : "false";
                    case ValidationDataType.String:
                        return CompileTool.BuildString(value);
                }
            }
            catch (Exception)
            {
                throw new ApplicationException("Некорректно указан формат значения");
            }
            return "";
        }
    }
}
