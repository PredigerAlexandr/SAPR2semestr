using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller
{
    public class GeneratedCodeHandler
    {
        public static string  GeneratedCode(object purchaseIdObject)
        {
            int purchaseId = int.Parse(purchaseIdObject.ToString());
            var fields = RuleCompiller.GetSimpleDocumentShema(purchaseId);
       
            if (fields.Count == 0) return "";

            var beforeRulesXML = RuleCompiller.GetRules(purchaseId, ValidationType.before);
            var afterRulesXML = RuleCompiller.GetRules(purchaseId, ValidationType.after);

            CodeTreeHandler codeTreeHandlerAfterRules = new CodeTreeHandler();
            CodeTreeHandler codeTreeHandlerBeforeRules = new CodeTreeHandler();

            var beforeRulesTree = RuleCompiller.BuildRulesTree(beforeRulesXML, fields, codeTreeHandlerBeforeRules);
            var afterRulesTree = RuleCompiller.BuildRulesTree(afterRulesXML, fields, codeTreeHandlerAfterRules);

            string code = RulesCodeBuilder.BuildValidationClass(beforeRulesTree, afterRulesTree, "GeneratedValidator", purchaseId);

            return code;
        }


    }
}
