using RuleCompiller.Validators;
using RuleCompiller.Validators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller
{
    public class RulesCodeBuilder
    {

        public static void AddUsingProperty(StringBuilder stringBuilder, string propertyName)
        {
            stringBuilder.Append('"' + propertyName + "\",");
        }

        public static string BuildValidationClass(RuleComposite beforeValidationRoot, RuleComposite afterValidationRoot,
            string className, int tradeSectionId, long modelId)
        {
            List<string> beforeUsingProperties = new List<string>();
            List<string> afterUsingProperties = new List<string>();
            StringBuilder codeText = new StringBuilder();
            codeText.Append(
                "using RuleCompiller;" +
                "using System;" +
                "using System.Threading.Tasks;" +
                "using System.Collections.Generic;" +


                "namespace SAPR.RuleCompiller {" +
                "public class " + className + " {"
            );

            //Build before validation
            if (beforeValidationRoot != null)
            {
                codeText.Append("public static string BeforeCheck");
                codeText.Append(BuildValidationFunctionBodyWithArgsFromRulesTree(ValidationType.before, beforeValidationRoot, beforeUsingProperties, tradeSectionId, modelId));
            }

            if (afterValidationRoot != null)
            {
                //BuildAfterValidation
                codeText.Append("public static string AfterChack");
                codeText.Append(BuildValidationFunctionBodyWithArgsFromRulesTree(ValidationType.after, afterValidationRoot, afterUsingProperties, tradeSectionId, modelId));

            }

            codeText.Append("}");


            codeText.Append("}");

            string result = codeText.ToString();
            result = SimpleCodeFormatter.Format(result, "    ");

            return result;
        }

        public static string BuildString(string stringParameter)
        {
            return '"' + stringParameter.Replace("\\", "\\\\").Replace("\"", "\\\"") + '"';
        }

        const string XML_VALUE_PREFIX = "value:";
        const string XML_FIELD_PREFIX = "field:";

        public static StringBuilder BuildValidationFunctionBodyWithArgsFromRulesTree(ValidationType type, RuleComposite ruleComposite, List<string> usingProperties,
            long tradeSectionId, long modelId)
        {
            CodeTreeHandler codeTreeHandler = ruleComposite.codeTreeHandler;
            StringBuilder codeText = new StringBuilder("");

            //прописываем список для хранения ошибок
            codeText.Append($"List<string> {codeTreeHandler.violationsVariableName} = new List<string>();");

            codeText.Append(ruleComposite.BuildValidatorCode());

            codeText.Append("if(" + codeTreeHandler.violationsVariableName + ".Count > 0){");
            codeText.Append("string errors = \"<Errors>\";");

            string index = codeTreeHandler.GetUniqueVariableName("i");
            codeText.Append("for(int " + index + " = 0; " + index + "<" + codeTreeHandler.violationsVariableName + ".Count;" + index + "++){");
            codeText.Append("errors += \"<Error>\"+" + codeTreeHandler.violationsVariableName + "[" + index + "]+\"</Error>\";");
            codeText.Append("}");

            codeText.Append("errors += \"</Errors>\";");
            codeText.Append(" return errors;");
            codeText.Append("} return \"\";");

            codeText.Append("}");

            StringBuilder properties = new StringBuilder();
            foreach (var property in codeTreeHandler.usingPropertys)
            {
                properties.Append('"' + property + '"' + ',');
            }
            if(properties.Length > 0) properties.Remove(properties.Length - 1, 1);

            switch (type)
            {
                case ValidationType.before:
                    codeText.Insert(0,
                        "(){" +
                        $"var " + codeTreeHandler.validationDataVariableName + $" = new ValidationData(ValidationType.before, new string [] {{{properties.ToString()}}}, " +
                        $"{tradeSectionId}, {modelId});"
                    );
                    break;
                case ValidationType.after:
                    codeText.Insert(0,
                        "(string " + codeTreeHandler.xmlDataParameterName + "){" +
                        $"var " + codeTreeHandler.validationDataVariableName + $" = new ValidationData(ValidationType.after, new string [] {{{properties.ToString()}}}, " +
                        $"{tradeSectionId}, {modelId}, " + codeTreeHandler.xmlDataParameterName + ");"
                    );
                    break;
            }

            return codeText;
        }

        protected static string GetOperatorForCompare(ValidationDataType type, ValidationOperator comapreOperator)
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

        public static string GetValue(ValidationDataType type, string value)
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
                        return "DateTime.Parse(" + BuildString(value) + ")";
                    case ValidationDataType.Bit:
                        return Boolean.Parse(value) ? "true" : "false";
                    case ValidationDataType.String:
                        return BuildString(value);
                }
            }
            catch (Exception)
            {
                throw new ApplicationException("Некорректно указан формат значения");
            }
            return "";
        }

        //private static List<Rule> GetRules(RuleComposite ruleComposite)
        //{
        //    var rulesStack = new List<Rule>();
        //    GetRules(ruleComposite, ref rulesStack);
        //    return rulesStack;
        //}
    }
}
