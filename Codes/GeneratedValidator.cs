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
            var validationData = new ValidationData(ValidationType.before, new string [] {"empty"}, 1005);
            List<string> violations = new List<string>();
            if(!string.IsNullOrEmpty(validationData["empty"]))
            {
                violations.Add("Значение свойства \"empty\" должно быть пустым.");
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
        public static string AfterCheck(string xmlData)
        {
            var validationData = new ValidationData(ValidationType.after, new string [] {"SNILS","Surname","Sport"}, 1005, xmlData);
            List<string> violations = new List<string>();
            if(string.IsNullOrEmpty(validationData["SNILS"]) && string.IsNullOrWhiteSpace(validationData["SNILS"]))
            {
                violations.Add("Свойство \"SNILS\" обязательно для заполнения.");
            }
            if(!string.IsNullOrEmpty(validationData["Surname"]))
            {
                violations.Add("Значение свойства \"Surname\" должно быть пустым.");
            }
            string sportString = validationData["Sport"];
            if(sportString != "Бокс" && sportString != "Волейбол" && sportString != "Баскетбол") 
            {
                violations.Add("Свойство \"Sport\" должно быть определенным значением.");
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