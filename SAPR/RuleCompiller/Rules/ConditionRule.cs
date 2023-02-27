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

namespace RuleCompiller.Conditions
{
    public class ConditionRule : Rule, IRuleComposite
    {
        // NOTE: Возможно данный набор компоновщиков следует заменить на словарь (Dictionary) компоновщиков (Dictionary<ConditionType, RuleComposite>).
        // И проводить инициализацию словаря в конструкторе "условного" компоновщика.
        // Очень важно: данное изменение может ухудшить "наглядность" кода, но позволит избавиться от избыточных "переключателей" (switch).
        public readonly RuleComposite _if;
        public readonly RuleComposite _then;
        public readonly RuleComposite _else;

        protected List<DocumentSchema> documentSchemas;

        private bool _lastConditionResult;

        public ConditionRule(List<DocumentSchema> documentSchemas, CodeTreeHandler codeTreeHandler) :
            this(new RuleComposite(codeTreeHandler), documentSchemas)
        {
        }

        public ConditionRule(RuleComposite rule, List<DocumentSchema> documentSchemas) : base(RuleViolationType.Error, rule.codeTreeHandler)
        {
            this.documentSchemas = documentSchemas;
            _if = rule;
            _then = new RuleComposite(rule.codeTreeHandler);
            _else = new RuleComposite(rule.codeTreeHandler);

            _if.compileSuccessMode = _then.compileSuccessMode = _else.compileSuccessMode = true;

            _lastConditionResult = true;
        }

        public void Add(ConditionType conditionType, Rule rule)
        {
            if (rule == null) return;

            // Если добавляемое правило является компоновщиком и не содержит в себе вложенных правил, то ничего не делаем.
            IRuleComposite composite = rule as IRuleComposite;
            if (composite != null && composite.RuleCount == 0) return;

            switch (conditionType)
            {
                case ConditionType.If:
                    _if.Add(rule);
                    break;
                case ConditionType.Then:
                    _then.Add(rule);
                    break;
                case ConditionType.Else:
                    _else.Add(rule);
                    break;
            }
        }

        public void Remove(ConditionType conditionType, Rule rule)
        {
            switch (conditionType)
            {
                case ConditionType.If:
                    _if.Remove(rule);
                    break;
                case ConditionType.Then:
                    _then.Remove(rule);
                    break;
                case ConditionType.Else:
                    _else.Remove(rule);
                    break;
            }
        }

        public override bool Match(ValidationData data)
        {
            _lastConditionResult = _if.Match(data);
            return _lastConditionResult ? _then.Match(data) : _else.Match(data);
        }

        /// <summary>
        /// Признак того, что при последней проверке правил были сообщения с нарушениями.
        /// </summary>
        public bool HasViolations
        {
            get { return _lastConditionResult ? _then.HasViolations : _else.HasViolations; }
        }

        /// <summary>
        /// Список нарушений с уровнем Error, выявленные при последней проверке правил.
        /// </summary>
        public IEnumerable<ValidationMessageData> ViolationMessages
        {
            get { return _lastConditionResult ? _then.ViolationMessages : _else.ViolationMessages; }
        }

        /// <summary>
        /// Количество правил, содержащихся в компоновщике.
        /// </summary>
        public int RuleCount
        {
            // При расчете количества правил в "условном" компоновщике не учитываем правила входящие в компонивщик условия,
            // т.к. это никак не повлияет на общую проверку правил.
            // Пояснения: в случае отсутствия правил в компоновщике выполнение проверки пойдет по основно (Then) ветке.
            get { return _then.RuleCount + _else.RuleCount; }
        }

        public static Rule Parse(XElement xRule, List<DocumentSchema> fields, CodeTreeHandler codeTreeHandler)
        {
            int lineNumber = ((IXmlLineInfo)xRule).LineNumber;

            try
            {
                var xIf = xRule.XPathSelectElement("if");
                var xThen = xRule.XPathSelectElement("then");
                var xElse = xRule.XPathSelectElement("else");

                var result = new ConditionRule(fields, codeTreeHandler);

                var reader = xIf.CreateReader();
                reader.MoveToContent();
                string ifRuleXML = reader.ReadInnerXml();

                reader = xThen.CreateReader();
                reader.MoveToContent();
                string thenRuleXML = reader.ReadInnerXml();

                reader = xElse.CreateReader();
                reader.MoveToContent();
                string elseRuleXML = reader.ReadInnerXml();

                RuleComposite ifRuleComposite = RuleCompiller.BuildRulesTree(ifRuleXML, fields, codeTreeHandler);
                RuleComposite thenRuleComposite = RuleCompiller.BuildRulesTree(thenRuleXML, fields, codeTreeHandler);
                RuleComposite elseRuleComposite = RuleCompiller.BuildRulesTree(elseRuleXML, fields, codeTreeHandler);
                ifRuleComposite.compileSuccessMode = thenRuleComposite.compileSuccessMode = elseRuleComposite.compileSuccessMode = true;
                
                result.Add(ConditionType.If, ifRuleComposite);
                result.Add(ConditionType.Then, thenRuleComposite);
                result.Add(ConditionType.Else, elseRuleComposite);
                
                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка разбора условного правила из xml. Строка: "+ lineNumber,  ex);
            }
        }

        protected override string CompileValidatorCode(string errorExpression = "", string successExpression = "")
        {
            StringBuilder codeText = new StringBuilder();
            string successFlagVariableName = codeTreeHandler.GetUniqueVariableName("successFlag");
            codeText.Append("bool "+ successFlagVariableName + " = true;");
            bool blockErrors = codeTreeHandler.CheckBlockErrors();
            //if expression build
            codeTreeHandler.validatorsStack.Push("if");

            string ifErrorExpression = successFlagVariableName + "=false;";
            //string ifSuccessExpression = successFlagVariableName + "=true;";
            codeText.Append(_if.BuildValidatorCode(ifErrorExpression));
            codeTreeHandler.validatorsStack.Pop();//Pop if block from stack

            if (blockErrors)//Для вложенностей ошибки выводится не будут
            {
                if (string.IsNullOrEmpty(errorExpression)) errorExpression = "{}";
                if (string.IsNullOrEmpty(successExpression)) successExpression = "{}";
            }

            //then and else expression build
            codeText.Append("if(" + successFlagVariableName + "){");
            codeText.Append(_then.BuildValidatorCode(errorExpression, successExpression));
            codeText.Append("}else{");
            codeText.Append(_else.BuildValidatorCode(errorExpression, successExpression));
            codeText.Append("}");

            return codeText.ToString();
        }

        public override string FormatErrors()
        {
            return ErrorMessage;
        }

        public override RuleViolationType ViolationType
        {
            get
            {
                return base.ViolationType;
            }
            set
            {
                base.ViolationType = value;
                if (_then != null) _then.ViolationType = value;
                if (_else != null) _else.ViolationType = value;
            }
        }
    }
}
