using RuleCompiller;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace SAPR.RuleCompiller 
{
    public class GeneratedValidator 
    {
        public static string BeforeCheck()
        {
            var validationData = new ValidationData(ValidationType.before, new string [] {"SNILS"}, 11, 9549);
            List<string> violations = new List<string>();
            if(!string.IsNullOrEmpty(validationData["SNILS"]))
            {
                violations.Add("Номер процедуры надо          оставить пустым");
            }
            if(violations.Count > 0)
            {
                string errors = "<Errors>";
                for(int i = 0; i<violations.Count;i++)
                {
                    errors += "<Error>"+violations[i]+"</Error>";
                }
                errors += "</Errors>";
                return errors;
            }
            return "";
        }
        public static string AfterChack(string xmlData)
        {
            var validationData = new ValidationData(ValidationType.after, new string [] {"PhoneNumber"}, 11, 9549, xmlData);
            List<string> violations = new List<string>();
            if(string.IsNullOrEmpty(validationData["PhoneNumber"]) && string.IsNullOrWhiteSpace(validationData["PhoneNumber"]))
            {
                violations.Add("Свойство \"PhoneNumber\" обязательно для заполнения.");
            }
            if(violations.Count > 0)
            {
                string errors = "<Errors>";
                for(int i = 0; i<violations.Count;i++)
                {
                    errors += "<Error>"+violations[i]+"</Error>";
                }
                errors += "</Errors>";
                return errors;
            }
            return "";
        }
    }
}