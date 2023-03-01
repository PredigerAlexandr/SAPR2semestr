using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RuleCompiller;

namespace TestRulesCompiler
{
    public class Program
    {
        static void Main(string[] args)
        {
            string xmlText = "<NumberPhone>8902<NumberPhone><NumberPassport><NumberPassport>";

            if (true)//IsBuild
            {
                var fields = RuleCompiller.RuleCompiller.GetSimpleDocumentShema(1);
                //var postFields = RuleCompiller.RuleCompiller.GetSimpleDocumentShema(11, 9549, xmlText);
                if (fields.Count == 0) return;
                var beforeRulesXML = RuleCompiller.RuleCompiller.GetRules(1, ValidationType.before);
                var afterRulesXML = RuleCompiller.RuleCompiller.GetRules(1, ValidationType.after);

                CodeTreeHandler codeTreeHandlerAfterRules = new CodeTreeHandler();
                CodeTreeHandler codeTreeHandlerBeforeRules = new CodeTreeHandler();
                var beforeRulesTree = RuleCompiller.RuleCompiller.BuildRulesTree(beforeRulesXML, fields, codeTreeHandlerBeforeRules);
                var afterRulesTree = RuleCompiller.RuleCompiller.BuildRulesTree(afterRulesXML, fields, codeTreeHandlerAfterRules);

                //bool result = FieldsAnalyser.CheckFields(documentShema);

                //RulesCodeBuilder.BuildCodeFromRulesTree(tree, "GeneratedValidator", 11, 9549);

                string code = RulesCodeBuilder.BuildValidationClass(beforeRulesTree, afterRulesTree, "GeneratedValidator", 11, 9549);
                File.WriteAllText(@"D:\УТП\utp_norbit\RuleCompiller\code.cs", code, Encoding.Default);
            }
            else
            {
                //GeneratedValidator validator = new GeneratedValidator();
                //List<string> resultBefore = validator.BeforeValidation();
                //List<string> resultAfter = validator.AfterValidation(xmlText);
            }
        }
    }
}
