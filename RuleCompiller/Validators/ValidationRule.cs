using RuleCompiller.Validators;
using RuleCompiller.Helpers;
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

namespace RuleCompiller.Validators
{
    public abstract class ValidationRule : Rule
    {
        protected ValidationRule(CodeTreeHandler codeTreeHandler, string propertyName, string errorHeader, string errorMessage) :
            this(codeTreeHandler, propertyName, RuleViolationType.Error, errorHeader, errorMessage)
        {
        }

        protected ValidationRule(CodeTreeHandler codeTreeHandler, string propertyName, RuleViolationType violationType, string errorHeader, string errorMessage) :
            base(violationType, codeTreeHandler)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
            ErrorHeader = errorHeader;
        }

        public override string FormatErrors()
        {
            return ErrorMessage.Replace("%FIELD_NAME%", '"'+PropertyName+'"');
        }

        public string PropertyName { get; private set; }
        public bool LastResult { get; protected set; }

        /// <summary>
        /// Получает значение заданного свойства объекта, используя отражение (reflection), если заданный объект не является реализацией <see cref="IValidatingObject"/>.
        /// </summary>
        /// <param name="businessObject">Ссылка на объект, значение свойства которого необходимо получить.</param>
        /// <returns></returns>
        //protected object GetPropertyValue(object businessObject)
        //{
        //    return GetPropertyValue(businessObject, PropertyName);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xRule"></param>
        /// <returns></returns>
        /// <example><![CDATA[Пример xml на вход:
        ///		<field name="" isId="" required="">
        ///			<compare with="otherFieldName" 
        ///						operator="equal|greater|lesser|greaterOrEqual|lesserOrEqual" 
        ///						type="integer|double|decimal|date|string" />
        ///			<regex>creditcard|email|ipaddress|pattern</regex>
        ///			<length min="" max="" />
        ///			<range min="" max="" type="integer|double|decimal|date|string" />
        ///		</field>]]>
        /// </example>
        public static Rule Parse(XElement xRule, List<DocumentSchema> fields, CodeTreeHandler codeTreeHandler)
        {
            var result = new RuleComposite(codeTreeHandler);

            foreach (var rule in xRule.XPathSelectElements("field"))
                ParseFieldFromXml(rule, result, fields);

            return result;
        }

        private static void ParseFieldFromXml(XElement xRule, RuleComposite result, List<DocumentSchema> fields)
        {
            int lineNumber = ((IXmlLineInfo)xRule).LineNumber;

            try
            {
                var field = xRule.Attribute("name");
                var id = xRule.Attribute("id");
                var required = xRule.Attribute("required");
                var isEmpty = xRule.Attribute("isEmpty");
                var isEmptyErrorMessage = xRule.Attribute("isEmptyErrorMessage");
                var requiredErrorMessage = xRule.Attribute("requiredErrorMessage");

                //объект поля по которому составлено правило
                //var fieldSchema = fields.Where(p => p.Name == field.Value).ToList()[0];

                var compares = xRule.XPathSelectElements(".//compare");
                var regexes = xRule.XPathSelectElements(".//regex");
                var lengths = xRule.XPathSelectElements(".//length");
                var ranges = xRule.XPathSelectElements(".//range");
                var fieldTypes = xRule.XPathSelectElements(".//fieldType");
                var inElem = xRule.XPathSelectElements(".//in");

                if (field == null || string.IsNullOrEmpty(field.Value))
                    throw new Exception(lineNumber +  "Не указано поле, над которым необходимо выполнять текущие проверки.");

                var fieldName = field.Value;

                if (id != null && id.Value.Equals("true"))
                    result.Add(new ValidateId(result.codeTreeHandler, fieldName));

                if (inElem.Any())
                {
                    var errorHeader = ExtractErrorHeader(inElem.FirstOrDefault());
                    var errorMessage = ExtractErrorMessage(inElem.FirstOrDefault());
                    var values = inElem.First().XPathSelectElements(".//value");
                    if (values.Any())
                    {
                        var valArr = values.Select(s => s.Attribute("name").Value).ToArray();

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            result.Add(new ValidateValuesIn(result.codeTreeHandler, fieldName, errorHeader, errorMessage, valArr));
                        }
                        else
                        {
                            result.Add(new ValidateValuesIn(result.codeTreeHandler, fieldName, valArr));
                        }
                    }
                }

                if (required != null && required.Value.Equals("true") && isEmpty != null && isEmpty.Value.Equals("true"))
                    throw new Exception(lineNumber + "Одновременно задана проверка на обязательность и на пустоту поля");

                if (required != null && required.Value.Equals("true"))
                {
                    if (requiredErrorMessage != null)
                    {
                        result.Add(new ValidateRequired(result.codeTreeHandler, fieldName, null, requiredErrorMessage.Value));
                    }
                    else
                    {
                        result.Add(new ValidateRequired(result.codeTreeHandler, fieldName));
                    }
                }

                if (isEmpty != null && isEmpty.Value.Equals("true"))
                {
                    if (isEmptyErrorMessage != null)
                    {
                        result.Add(new ValidateEmpty(result.codeTreeHandler, fieldName, null, isEmptyErrorMessage.Value));
                    }
                    else
                    {
                        result.Add(new ValidateEmpty(result.codeTreeHandler, fieldName));
                    }
                }

                foreach (var compare in compares)
                {
                    var compareField = compare.Attribute("with");
                    var compareOperator = compare.Attribute("operator");
                    var compareType = compare.Attribute("type");
                    var errorHeader = ExtractErrorHeader(compare);
                    var errorMessage = ExtractErrorMessage(compare);

                    if (compareField != null)
                    {
                        if (compareOperator == null)
                            throw new Exception(lineNumber + "Не указан оператор сравнения.");

                        if (compareType == null)
                            throw new Exception(lineNumber + "Не указан тип сравнения.");

                        if (!_operatorsMap.ContainsKey(compareOperator.Value))
                            throw new Exception(lineNumber + "Указан неизвестный оператор сравнения.");

                        if (!_typesMap.ContainsKey(compareType.Value))
                            throw new Exception(lineNumber + "Указан неизвестный тип данных для сравнения.");

                        if (string.IsNullOrEmpty(errorMessage))
                            result.Add(new ValidateCompare(result.codeTreeHandler, fieldName, compareField.Value,
                                                           _operatorsMap[compareOperator.Value],
                                                           _typesMap[compareType.Value]));
                        else
                            result.Add(new ValidateCompare(result.codeTreeHandler, fieldName, compareField.Value,
                                                           _operatorsMap[compareOperator.Value],
                                                           _typesMap[compareType.Value],
                                                           errorHeader,
                                                           errorMessage));
                    }
                }

                foreach (var regex in regexes)
                {
                    // Извлекаем сообщение об ошибке для проверки по регулярному выражению.
                    var errorHeader = ExtractErrorHeader(regex);
                    var errorMessage = ExtractErrorMessage(regex);

                    // Для корректной работы проверки по регулярному выражению в старом формате:
                    // удаляем тег с сообщением об ошибке, если такой существует.
                    if (regex.XPathSelectElement("./errorHeader") != null)
                        regex.XPathSelectElement("./errorHeader").Remove();
                    if (regex.XPathSelectElement("./errorMessage") != null)
                        regex.XPathSelectElement("./errorMessage").Remove();

                    // Для внедрения новой схемы:
                    // извлекаем узел с шаблоном регулярного выражения.
                    var regexPattern = regex.XPathSelectElement("./pattern");
                    // Далее получаем шаблон, не забывая при это про поддержку правил со старой схемой.
                    // (при полном переходе на новую схему данную логику следует удалить).
                    var regexValue = regexPattern == null ? regex.Value : regexPattern.Value;

                    if (regexValue.Equals("creditcard"))
                        result.Add(string.IsNullOrEmpty(errorMessage) ? new ValidateCreditcard(result.codeTreeHandler, fieldName) : new ValidateCreditcard(result.codeTreeHandler, fieldName, errorHeader, errorMessage));
                    if (regexValue.Equals("email"))
                        result.Add(string.IsNullOrEmpty(errorMessage) ? new ValidateEmail(result.codeTreeHandler, fieldName) : new ValidateEmail(result.codeTreeHandler, fieldName, errorHeader, errorMessage));
                    if (regexValue.Equals("ipaddress"))
                        result.Add(string.IsNullOrEmpty(errorMessage) ? new ValidateIPAddress(result.codeTreeHandler, fieldName) : new ValidateIPAddress(result.codeTreeHandler, fieldName, errorHeader, errorMessage));
                    if (!(new[] { "email", "creditcard", "ipaddress" }).Contains(regexValue))
                        result.Add(string.IsNullOrEmpty(errorMessage) ? new ValidateRegex(result.codeTreeHandler, fieldName, regexValue) : new ValidateRegex(result.codeTreeHandler, fieldName, regexValue, errorHeader, errorMessage));
                }

                foreach (var length in lengths)
                {
                    var xlMin = length.Attribute("min");
                    var xlMax = length.Attribute("max");

                    var lMin = xlMin == null ? 0 : int.Parse(xlMin.Value);
                    var lMax = xlMax == null ? int.MaxValue : int.Parse(xlMax.Value);

                    var errorHeader = ExtractErrorHeader(length);
                    var errorMessage = ExtractErrorMessage(length);

                    result.Add(string.IsNullOrEmpty(errorMessage)
                                ? new ValidateLength(result.codeTreeHandler, fieldName, lMin, lMax)
                                : new ValidateLength(result.codeTreeHandler, fieldName, lMin, lMax, errorHeader, errorMessage));
                }

                foreach (var range in ranges)
                {
                    var xrMin = range.Attribute("min");
                    var xrMax = range.Attribute("max");
                    var rType = range.Attribute("type");
                    var withLeft = range.Attribute("withLeft");
                    var withRight = range.Attribute("withRight");

                    var errorHeader = ExtractErrorHeader(range);
                    var errorMessage = ExtractErrorMessage(range);

                    if (rType == null)
                        throw new Exception(lineNumber + "Не указан тип сравнения для проверки диапазона значений.");

                    if (!_typesMap.ContainsKey(rType.Value))
                        throw new Exception(lineNumber + "Указан неизвестный тип сравнения для проверки диапазона значений.");

                    if (string.IsNullOrEmpty(errorMessage))
                        result.Add(new ValidateRange(result.codeTreeHandler, fieldName,
                                                     xrMin == null ? null : xrMin.Value,
                                                     xrMax == null ? null : xrMax.Value,
                                                     _typesMap[rType.Value],
                                                     withLeft == null || withLeft.Value.Equals("true"),
                                                     withRight == null || withRight.Value.Equals("true")));
                    else
                        result.Add(new ValidateRange(result.codeTreeHandler, fieldName,
                                                     xrMin == null ? null : xrMin.Value,
                                                     xrMax == null ? null : xrMax.Value,
                                                     _typesMap[rType.Value],
                                                     withLeft == null || withLeft.Value.Equals("true"),
                                                     withRight == null || withRight.Value.Equals("true"),
                                                     errorHeader,
                                                     errorMessage));
                }

                foreach (var fieldType in fieldTypes)
                {
                    var requiredType = fieldType.Attribute("type");
                    var errorHeader = ExtractErrorHeader(fieldType);
                    var errorMessage = ExtractErrorMessage(fieldType);

                    if (!ValidateType.CheckIfTypeValid(requiredType.Value))
                    {
                        throw new Exception(lineNumber + string.Format("Неизвестный тип {0}", requiredType.Value));
                    }


                    if (string.IsNullOrEmpty(errorMessage))
                    {
                        result.Add(new ValidateType(result.codeTreeHandler, fieldName, requiredType.Value));
                    }
                    else
                    {
                        result.Add(new ValidateType(result.codeTreeHandler, fieldName, requiredType.Value, errorHeader, errorMessage));
                    }


                }

            }
            catch (Exception ex)
            {
                throw new Exception(lineNumber + "Ошибка разбора правила-валидации из xml.", ex);
            }
        }

        protected static string ExtractErrorMessage(XElement source)
        {
            var xError = source.XPathSelectElement("./errorMessage");
            return xError == null ? string.Empty : string.Concat(xError.Nodes());
        }

        protected static string ExtractErrorHeader(XElement source)
        {
            var xError = source.XPathSelectElement("./errorHeader");
            return xError == null ? string.Empty : xError.Value;
        }

        private static readonly Dictionary<string, ValidationOperator> _operatorsMap =
            new Dictionary<string, ValidationOperator>
            {
                { "equal", ValidationOperator.Equal},
                { "notequal", ValidationOperator.NotEqual},
                { "greater", ValidationOperator.GreaterThan },
                { "greaterOrEqual", ValidationOperator.GreaterThanEqual },
                { "less", ValidationOperator.LessThan },
                { "lessOrEqual", ValidationOperator.LessThanEqual },
            };

        private static readonly Dictionary<string, ValidationDataType> _typesMap =
            new Dictionary<string, ValidationDataType>
            {
            { "integer", ValidationDataType.Integer },
            { "double", ValidationDataType.Double },
            { "decimal", ValidationDataType.Decimal },
            { "date", ValidationDataType.Date },
            { "string", ValidationDataType.String },
            { "bit", ValidationDataType.Bit },
            };
    }
}
