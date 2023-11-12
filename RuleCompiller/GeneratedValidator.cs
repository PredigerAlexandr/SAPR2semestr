using RuleCompiller;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace RuleCompiller
{
    public class GeneratedValidator
    {
        public static string BeforeCheck()
        {
            var validationData = new ValidationData(ValidationType.before, new string[] { }, 1);
            List<string> violations = new List<string>();
            if (violations.Count > 0)
            {
                string errors = "<Errors>";
                for (int i = 0; i < violations.Count; i++)
                {
                    errors += "<Error>" + violations[i] + "</Error>";
                }
                errors += "</Errors>";
                return errors;
            }
            return "";
        }
        public static string AfterCheck(object xmlObject)
        {
            string xmlData = xmlObject.ToString();
            var validationData = new ValidationData(ValidationType.after, new string[] { "Name" }, 1, xmlData);
            List<string> violations = new List<string>();
            if (string.IsNullOrEmpty(validationData["Name"]) && string.IsNullOrWhiteSpace(validationData["Name"]))
            {
                violations.Add("�������� \"Name\" ����������� ��� ����������.");
            }
            if (violations.Count > 0)
            {
                string errors = "<Errors>";
                for (int i = 0; i < violations.Count; i++)
                {
                    errors += "<Error>" + violations[i] + "</Error>";
                }
                errors += "</Errors>";
                return errors;
            }
            return "";
        }
    }
}