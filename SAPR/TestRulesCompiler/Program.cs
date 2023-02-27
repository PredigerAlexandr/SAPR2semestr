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
            string xmlText = "<rootpanel><vtable1><edit1>9</edit1><field12><file><fileid></fileid><filename></filename><signinfo></signinfo><hash></hash><sign></sign><combine_sign></combine_sign></file></field12><field23><file><signtype></signtype><fileid></fileid><filename></filename><signinfo></signinfo><filecreatedt></filecreatedt><hash></hash><sign></sign><combine_sign></combine_sign><hash2012></hash2012><sourcefileid></sourcefileid><filecomment></filecomment></file></field23><field34><combine_sign></combine_sign><file><signtype></signtype><fileid></fileid><filename></filename><signinfo></signinfo><filecreatedt></filecreatedt><hash></hash><sign></sign><hash2012></hash2012><hash2012></hash2012></file></field34><field67><file><signtype></signtype><fileid></fileid><filename></filename><signinfo></signinfo><filecreatedt></filecreatedt><hash></hash><sign></sign><combine_sign></combine_sign><hash2012></hash2012><sourcefileid></sourcefileid><filecomment></filecomment></file></field67><area1>зонтик</area1><sum1>1234</sum1><datetime1></datetime1><datetime2></datetime2><datetime3></datetime3><date1></date1><purid></purid><purstatus>Статус по умолчанию</purstatus><test>RUB</test><testcode>RUB</testcode><testname>Российский рубль</testname><nodess><nodes><nodes>Branch.002</nodes><nodescode>Branch.002</nodescode><nodesname>Горюче-смазочные материалы, энергоносители</nodesname></nodes><nodes><nodes>Branch.003</nodes><nodescode>Branch.003</nodescode><nodesname>Древесина и продукция деревообработки</nodesname></nodes><nodes><nodes>Branch.006</nodes><nodescode>Branch.006</nodescode><nodesname>Медикаменты, медицинские материалы, оборудование, инструмент</nodesname></nodes></nodess></vtable1><gtable3><gtrablerow3><isdeleted></isdeleted><testinput10>Значение по умолчанию</testinput10><test1>Значение по умолчанию</test1></gtrablerow3></gtable3><signtype></signtype><fileid></fileid><filename></filename><signinfo></signinfo><filecreatedt></filecreatedt><hash></hash><sign></sign><combine_sign></combine_sign><hash2012></hash2012><filecomment></filecomment><gtable2><htablevtable1><htablevtableinput1>Значение по умолчанию</htablevtableinput1><vtablehtable1><vtablehtable1input1>Значение по умолчанию</vtablehtable1input1></vtablehtable1></htablevtable1></gtable2><bids><bid><isdeleted></isdeleted><bidinfo><bidno></bidno><bidiscanceled></bidiscanceled><bidname></bidname><bidprice></bidprice></bidinfo></bid></bids><gtable><gtablerow><isdeleted></isdeleted><vtabletest><inputtest>Значение по умолчанию</inputtest></vtabletest><vtabletest2><inputtest2>Значение по умолчанию</inputtest2><testinput300>Значение по умолчанию</testinput300></vtabletest2></gtablerow></gtable></rootpanel>";


            if (true)//IsBuild
            {
                var fields = RuleCompiller.RuleCompiller.GetSimpleDocumentShema(11, 9549);
                var postFields = RuleCompiller.RuleCompiller.GetSimpleDocumentShema(11, 9549, xmlText);
                if (fields.Count == 0) return;
                var beforeRulesXML = RuleCompiller.RuleCompiller.GetRules(11, 9549, fields[0].Object, ValidationType.before);
                var afterRulesXML = RuleCompiller.RuleCompiller.GetRules(11, 9549, fields[0].Object, ValidationType.after);

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
