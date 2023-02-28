using RuleCompiller;
using RuleCompiller.Plugs;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace GOS_2._0.Process.Actions 
{
    public class GeneratedValidator :
     BaseDocumentProcess 
    {
        public override async Task<object> GetAsync(string xmlData, string parm, ApiHandlerModel handler, PersonModel person, BaseUnitOfWork unitOfWork, string filter = "", string language = "ru-RU",  Dictionary<string, object> handlerParams = null)
        {
            var validationData = new ValidationData(ValidationType.before, new string [] {"Code","Amount"}, 11, 9549);
            List<string> violations = new List<string>();
            if(string.IsNullOrEmpty(validationData["Code"]) && string.IsNullOrWhiteSpace(validationData["Code"]))
            {
                violations.Add("Поле \"Code\" должно быть заполнено!");
            }
            if(!string.IsNullOrEmpty(validationData["Amount"]))
            {
                violations.Add("Поле \"Amount\" должно оставаться пустым!");
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
        public override async Task<object> ProcessAsync(string xmlData, string parm, ApiHandlerModel handler, PersonModel person, BaseUnitOfWork unitOfWork, string sign = "", FilesContainerModel filesContainerModel = null, Dictionary<string, object> handlerParams = null)
        {
            var validationData = new ValidationData(ValidationType.after, new string [] {}, 11, 9549, xmlData);
            List<string> violations = new List<string>();
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