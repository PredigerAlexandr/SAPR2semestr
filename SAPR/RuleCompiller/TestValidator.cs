//using RuleCompiller.Validators;
//using RuleCompiller.Models;
//using RuleCompiller.Validators;
//using System.Collections.Generic;
//using System.Text;

//namespace RuleCompiller
//{
//    public class GeneratedValidator
//    {
//        public string BeforeValidation()
//        {
//            List<string> violations = new List<string>(3);
//            var data = new ValidationData(ValidationType.before, new string[] { "name" });
//            if (string.IsNullOrEmpty(data["name"]) == true) violations.Add("Поле \"name\" должно быть заполнено");

//            if (violations.Count > 0)
//            {
//                StringBuilder result = new StringBuilder();
//                result.Append("<Errors>");
//                foreach (var errorText in violations)
//                {
//                    result.Append("<Error>");
//                    result.Append(errorText);
//                    result.Append("</Error>");
//                }
//                result.Append("</Errors>");
//                return result.ToString();
//            }

//            return "";
//        }

//        public List<string> AfterValidation(string validationXml)
//        {
//            var validationData = new ValidationData(ValidationType.after, fieldsInfo, validationXml);
//            List<string> violations = new List<string>();
//            Rule[] validators = new Rule[]
//            {
//                new ValidateRequired("Amount",null,"Поле %FIELD_NAME% должно быть заполнено"),new ValidateRequired("Code",null,"Поле %FIELD_NAME% должно быть заполнено"),
//            };
//            foreach (var rule in validators)
//            {
//                if (!rule.Match(validationData))
//                {
//                    violations.Add(rule.ErrorMessage);
//                }
//            }
//            return violations;
//        }
//        public GeneratedValidator() : base(new string[] { "Amount", "Code" }, 11, 9549)
//        {
//        }
//    }
//}