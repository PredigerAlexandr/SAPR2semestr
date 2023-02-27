using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Plugs
{
    public abstract class BaseDocumentProcess
    {
        public abstract Task<object> GetAsync(string validationXml, string parm, ApiHandlerModel handler, PersonModel person, BaseUnitOfWork unitOfWork, string filter = "", string language = "ru-RU", Dictionary<string, object> handlerParams = null);

        public abstract Task<object> ProcessAsync(string validationXml, string parm, ApiHandlerModel handler, PersonModel person, BaseUnitOfWork unitOfWork, string sign = "", FilesContainerModel filesContainerModel = null, Dictionary<string, object> handlerParams = null);
    }
}
