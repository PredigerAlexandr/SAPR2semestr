using RuleCompiller.Models;
using RuleCompiller.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RuleCompiller.Rules
{
    public class SelectCaseRule : Rule, IRuleComposite
    {
        public string FieldName { get; set; }

        public RuleComposite ElseRule { get; set; }

        public RuleComposite IsNullRule { get; set; }

        public readonly Dictionary<string, RuleComposite> Cases = new Dictionary<string, RuleComposite>();

        private RuleComposite _selectedRule = null;


        public SelectCaseRule(CodeTreeHandler codeTreeHandler) :
            this(new RuleComposite(codeTreeHandler))
        {
        }

        public SelectCaseRule(RuleComposite rule)
            : base(RuleViolationType.Error, rule.codeTreeHandler)
        {
        }

        public override bool Match(ValidationData validationData)//Получаем propertyValue по fieldName проверяем есть ли значение в CASE и берем правило если такового нету то проверяем по IsNull
        {
            string propertyName = FieldName;
            string propertyValue = validationData[FieldName];
            //Выбираем из всех Cases case['propertyValue']
            if (Cases.ContainsKey(propertyValue))
            {
                _selectedRule = Cases[propertyValue];
            }
            else
            {
                if (string.IsNullOrEmpty(propertyValue) && IsNullRule != null)
                {
                    _selectedRule = IsNullRule;
                }
                else
                {
                    _selectedRule = ElseRule;
                }
            }

            return _selectedRule == null || _selectedRule.Match(validationData);
        }

        public static Rule Parse(XElement xRule, List<DocumentSchema> fields, CodeTreeHandler codeTreeHandler)
        {
            int lineNumber = ((IXmlLineInfo)xRule).LineNumber;

            SelectCaseRule rule = new SelectCaseRule(codeTreeHandler);

            try
            {
                rule.FieldName = xRule.XPathSelectElement("./field").Attribute("name").Value;//парсим fieldname

                foreach (var element in xRule.XPathSelectElements("./field/case"))//Перебор case элементов
                {
                    string value = element.Attribute("value")?.Value;

                    if (value == null) //Если атрибут value пустой то смотрим содержимое isEmpty атрибута если не пустой то парсим содержисое тега ./rules и создаем rule composite в isnullable
                    {
                        value = element.Attribute("isEmpty").Value;
                        var ruleElement = element.XPathSelectElement("./rules");

                        var reader = ruleElement.CreateReader();
                        reader.MoveToContent();
                        string rulesXml = reader.ReadInnerXml();


                        if (bool.TryParse(value, out bool val) && val)
                        {
                            rule.IsNullRule = RuleCompiller.BuildRulesTree(rulesXml, fields, codeTreeHandler);
                        }
                    }
                    else //создаем rulecomposite правило и засовываем в кейсы
                    {
                        var ruleElement = element.XPathSelectElement("./rules");

                        var reader = ruleElement.CreateReader();
                        reader.MoveToContent();
                        string rulesXml = reader.ReadOuterXml();

                        rule.Cases.Add(value, RuleCompiller.BuildRulesTree(rulesXml, fields, codeTreeHandler));
                    }

                }

                //else rule 
                var elseRuleElement = xRule.XPathSelectElement("./field/else/rules");

                if (elseRuleElement != null)
                {
                    var reader = elseRuleElement.CreateReader();
                    reader.MoveToContent();
                    string elseRuleXml = reader.ReadOuterXml();
                    rule.ElseRule = RuleCompiller.BuildRulesTree(elseRuleXml, fields, codeTreeHandler);
                }

                return rule;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка разбора case-правила из xml. Строка: "+lineNumber + ". Причина: " + ex.Message);
            }
        }

        protected override string CompileValidatorCode(string errorExpression = "", string successExpression = "")
        {
            StringBuilder codeText = new StringBuilder();

            string valueVariableName = codeTreeHandler.GetUniqueVariableName("value");
            codeText.Append("string "+valueVariableName+" = "+ codeTreeHandler.validationDataVariableName + '[' + CompileTool.BuildString(FieldName) + "];");

            bool firstConditionFlag = true;
            //IsNullCheck
            if (IsNullRule != null)
            {
                codeText.Append("if(string.IsNullOrEmpty(" + valueVariableName+")){");
                codeText.Append(IsNullRule.BuildValidatorCode(errorExpression, successExpression));
                codeText.Append("}");
                firstConditionFlag = false;
            }

            
            foreach (var caseItem in Cases)
            {
                //build else for if
                if (firstConditionFlag != true)
                {
                    codeText.Append(" else ");
                }
                //Build if
                codeText.Append(" if("+ valueVariableName + " == " + CompileTool.BuildString(caseItem.Key)+"){");
                codeText.Append(caseItem.Value.BuildValidatorCode(errorExpression, successExpression)); 
                codeText.Append("}");

                firstConditionFlag = false;
            }

            if (ElseRule != null)
            {
                string elseExpression = ElseRule.BuildValidatorCode(errorExpression, successExpression);
                if (!CompileTool.IsUnuselessExpression(elseExpression))
                {
                    codeText.Append(" else { " + elseExpression + "}");
                }
            }

            return codeText.ToString();
        }

        /// <summary>
        /// Признак того, что при последней проверке правил были сообщения с нарушениями уровня Error.
        /// </summary>
        public bool HasViolations
        {
            get
            {
                if (_selectedRule != null)
                {
                    return _selectedRule.HasViolations;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Список нарушений, выявленных при последней проверке правил.
        /// </summary>
        public IEnumerable<ValidationMessageData> ViolationMessages
        {
            get
            {
                if (_selectedRule != null)
                {
                    return _selectedRule.ViolationMessages;
                }
                else
                {
                    return new List<ValidationMessageData>();
                }
            }
        }

        /// <summary>
        /// Количество правил, содержащихся в компоновщике.
        /// </summary>
        public int RuleCount
        {
            get
            {
                int ruleCount = Cases.Values.Select(v => v.RuleCount).Sum();

                if (ElseRule != null)
                {
                    ruleCount += ElseRule.RuleCount;
                }

                if (IsNullRule != null)
                {
                    ruleCount += IsNullRule.RuleCount;
                }

                return ruleCount;
            }
        }

        public override string FormatErrors()
        {
            return ErrorMessage;
        }
    }
}
