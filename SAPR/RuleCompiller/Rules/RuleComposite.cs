using RuleCompiller.Validators;
using RuleCompiller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.ValidationException;
using RuleCompiller.Conditions;

namespace RuleCompiller.Validators
{
    /// <summary>
    /// Компонивщик правил. Осуществляет управление набором правил.
    /// </summary>
    public class RuleComposite : Rule, IRuleComposite
    {

        public List<Rule> _rules;
        private readonly List<ValidationMessageData> _violations;

        /// <summary>
        /// Конструктор класса. Создает новый экземпляр компоновщика.
        /// </summary>
        public RuleComposite(RuleViolationType violationType, CodeTreeHandler codeTreeHandler) : base(violationType, codeTreeHandler)
        {
            _rules = new List<Rule>();
            _violations = new List<ValidationMessageData>();
        }

        /// <summary>
        /// Конструктор класса. Создает новый экземпляр компоновщика.
        /// </summary>
        public RuleComposite(CodeTreeHandler codeTreeHandler) : this(RuleViolationType.Error, codeTreeHandler)
        {
        }

        public bool compileSuccessMode = false;//Флаг отвечающий за генерацию кода деревом т.е случае возникновения ошибки дальнейшие проверки проводится не будут.

        protected override string CompileValidatorCode(string errorExpression, string successExpression = "")
        {
            StringBuilder codeText = new StringBuilder();
            List<Rule> rules = GetRules(this);
            bool compileSuccessMode = this.compileSuccessMode;//multiti thread fix
            //прописываем список для хранения ошибок
            //codeText.Append($"List<string> {codeTreeHandler.violationsVariableName} = new List<string>({rules.Count});");
            string lastSuccessExpression = successExpression;
            //прописываем каждую проверку в ручную в зависимости от её типа 
            for (int i = 0; i < rules.Count; i++)
            {
                //обработка заголовка ошибки, если отсутствует, то прописываем null в ручную
                //string errorHeader = ((ValidationRule)rules[i]).ErrorHeader != null ? "\"" + ((ValidationRule)rules[i]).ErrorHeader + "\"" : "null";
                //string propertyName;
                //string errorMessage = "";
                string ruleTypeName = rules[i].GetType().Name;
                if(ruleTypeName == "RuleComposite")
                {
                    RuleComposite ruleComposite = rules[i] as RuleComposite;
                    ruleComposite.compileSuccessMode = compileSuccessMode;
                }

                switch (ruleTypeName)
                {
                    case "RuleComposite":
                    case "ValidateRequired":
                    case "ValidateEmpty":
                    case "ValidateCompare":
                    case "ValidateRange":
                    case "ValidateId":
                    case "ValidateValuesIn":
                    case "ValidateLength":
                    case "ValidateRegex":
                    case "ValidateIPAddress":
                    case "ValidateCreditcard":
                    case "ValidateEmail":
                    case "ValidateType":
                    case "ConditionRule":
                    case "SelectCaseRule":
                        //вложенности
                        //ruleComposite.compileSuccessMode = compileSuccessMode;
                        if (compileSuccessMode)
                        {
                            lastSuccessExpression = rules[i].BuildValidatorCode(errorExpression, lastSuccessExpression);
                        }
                        else
                        {
                            codeText.Append(rules[i].BuildValidatorCode(errorExpression, successExpression));
                        }
                        break;
                    //case "ConditionRule":
                    //    ConditionRule conditionRule = rules[i] as ConditionRule;
                    //    codeText.Append(rules[i].BuildValidatorCode(errorExpression, successExpression));
                    //    break;
                }
            }

            return (compileSuccessMode) ? lastSuccessExpression : codeText.ToString();
        }

        private static List<Rule> GetRules(RuleComposite ruleComposite)
        {
            var rulesStack = new List<Rule>();
            GetRules(ruleComposite, ref rulesStack);
            return rulesStack;
        }

        private static void GetRules(RuleComposite ruleComposite, ref List<Rule> listRules)
        {
            foreach (var rule in ruleComposite._rules)
            {
                if (rule is RuleComposite)
                {
                    listRules.AddRange(GetRules((RuleComposite)rule));
                }
                else
                {
                    listRules.Add(rule);
                }
            }
        }

        public int RuleCount
        {
            get { return _rules.Count; }
        }


        /// <summary>
        /// Добавляет указанное правило в компоновщик для последующей обработки.
        /// </summary>
        /// <param name="rule">Правило, которое необходимо добавить в компоновщик.</param>
        public void Add(Rule rule)
        {
            if (rule == null) return;

            // Если добавляемое правило является компоновщиком и не содержит в себе вложенных правил, то ничего не делаем.
            IRuleComposite composite = rule as IRuleComposite;
            if (composite != null && composite.RuleCount == 0) return;

            if (!_rules.Contains(rule))
            {
                _rules.Add(rule);
            }
        }

        /// <summary>
        /// Удаляет указанное правило из компоновщика.
        /// </summary>
        /// <param name="rule">Правило, которое необходимо удалить из компонивщика.</param>
        public void Remove(Rule rule)
        {
            if (rule == null) return;

            if (_rules.Contains(rule))
                _rules.Remove(rule);
        }

        public override bool Match(ValidationData data)
        {
            //if (this.SchemaType == RuleSchemaType.MainOnly && GetSchemaType(target) > 0 ||
            //    this.SchemaType == RuleSchemaType.AttachOnly && GetSchemaType(target) == 0)
            //    return true;
            return Match(data, true);
        }

        public bool Match(ValidationData data, bool checkAll)
        {
            var result = true;

            _violations.Clear();

            //LogHelper.Logger.Debug("RuleComposite.Match begin");
            foreach (var rule in _rules)
            {
                string header;
                string message;
                bool ruleResult = true;
                RuleViolationType violationType;

                try
                {
                    //if (this.SchemaType == RuleSchemaType.MainOnly && GetSchemaType(target) > 0 ||
                    //    this.SchemaType == RuleSchemaType.AttachOnly && GetSchemaType(target) == 0)
                    //{
                    //    //LogHelper.Logger.DebugFormat("ruleResult = true {0}", this.SchemaType);
                    //    ruleResult = true;
                    //}
                    //else
                    //{
                        //LogHelper.Logger.Debug("rule.Match(target)");
                        ruleResult = rule.Match(data);
                    //}

                    header = rule.ErrorHeader;
                    message = rule.ErrorMessage;
                    violationType = rule.ViolationType;

                    //LogHelper.Logger.DebugFormat("RuleComposite.Match {0} {1} {2} {3} {4} {5}", this.SchemaType, GetSchemaType(target), rule.ErrorMessage ?? "Null", rule.SchemaType, ruleResult, rule.GetType().ToString());
                }
                catch (Exception ex)
                {
                    //LogHelper.Logger.Error(ex);

                    message = ex.Message;
                    violationType = RuleViolationType.Error;
                    if (ex is System.Linq.Dynamic.ValidationException.ParamNotFoundException)
                    {
                        //Не удалось получить какой-то параметр. Прокидываем исключение выше, дополняя по возможности.
                        var exc = ex as ParamNotFoundException;
                        exc.ValidationObject = data;
                        exc.RuleName = Name;
                        exc.VerificationCode = rule.VerificationCode;

                        throw exc;
                    }

                    //if (ex.InnerException is AstBaseException)
                    //{
                    //	message = ex.InnerException.Message;
                    //}

                    //TODO: отрефакторить обработку технических ошибок валидации
                    var errorFormat = string.Format("Техническая ошибка при валидации правил. Проверяемый объект: '{0}'. \n {1}", data, message);
                    if (!string.IsNullOrEmpty(Name))
                        errorFormat = string.Format("{0} Имя правила: '{1}': ", errorFormat, Name);
                    if (message.StartsWith("Техническая ошибка при валидации правил."))
                    {
                        //Не перезатираем месадж, а прокидываем его же наверх. По возможности дополняем кодом правила в библиотеке.
                        errorFormat = message;
                        if (!string.IsNullOrEmpty(rule.VerificationCode))
                            errorFormat = string.Format("{0} Код правила из библиотеки: '{1}'", errorFormat, rule.VerificationCode);

                    }

                    //throw new AstInvalidOperationException(errorFormat, ex);
                }

                result = result && ruleResult;
                if (!ruleResult)
                {
                    //if (rule is IRuleComposite)
                    //	_violations.AddRange(((IRuleComposite)rule).ViolationMessages);
                    //else
                    //	_violations.Add(new ValidationMessageData(violationType, header, message));

                    if (!checkAll)
                        break;
                }
            }

            //LogHelper.Logger.DebugFormat("RuleComposite.Match end: {0}", result);
            return result;
        }

        public override string FormatErrors()
        {
            return "";
        }

        /// <summary>
        /// Признак того, что при последней проверке правил были сообщения с нарушениями уровня Error.
        /// </summary>
        public bool HasViolations
        {
            get { return _violations.Any(); }
        }

        /// <summary>
        /// Список нарушений, выявленных при последней проверке правил.
        /// </summary>
        public IEnumerable<ValidationMessageData> ViolationMessages
        {
            get { return _violations.ToArray(); }
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

                if (_rules != null)
                    foreach (var rule in _rules)
                        rule.ViolationType = value;
            }
        }
    }
}
