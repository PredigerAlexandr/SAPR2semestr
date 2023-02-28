//using Norbit.NBT.Dao;
using RuleCompiller.Helpers;
//using StructureMap;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
//using System.Linq.Dynamic.ValidationException;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RuleCompiller.Rules
{
    /// <summary>
    /// Правила для расчета полей на основе SQL-функций.
    /// </summary>
    /// <remarks>
    /// Шаблон XML используемый для объявления правила:
    /// <![CDATA[
    ///<!-- Запись вычисления, используя вызов SQL-функций с передачей указанных параметров. -->
    ///<target>
    ///    <!-- Наименование вычислимого поля. -->
    ///    <name>FieldName</name>
    ///    <!-- Описание вычислимого поля. Данный узел может отсутствовать. -->
    ///    <description>FieldDescription</description>
    ///    <!-- SQL-функция для вычисления значения поля. -->
    ///    <function>
    ///        <!-- Название функции с указанием схемы. Пример: dbo.MyFunction. -->
    ///        <name>FunctionName</name>
    ///        <!-- Параметры функции.
    ///             Паметры функции могут быть как константами, так и текущими значениями полей объектов.
    ///             Для передачи текущего значения поля в функцию используется синтаксис: field:FieldName.
    ///             Где FieldName - название поля объекта.
    ///             Параметры передаются в вызываемую функцию в порядке их следования в объявлении.
    ///             Так же параметр может быть предварительно вычислен с использованием лямбда-выражений.
    ///             Систаксис записи такого параметра: expression:LambdaExpression.
    ///             Где LambdaExpression - лямбда-выражение для вычисления значения. -->
    ///        <params>
    ///            <param value="" />
    ///            <param value="123" />
    ///            <param value="adc" />
    ///            <param value="field:FieldName" />
    ///            <param value="expression:LambdaExpression"/>
    ///        </params>
    ///    </function>
    ///</target>
    /// ]]>
    /// </remarks>
    public class SqlBusinessRule : BusinessRule
    {
        private const string XML_FIELD_PREFIX = "field:";
        private const string XML_EXPRESSION_PREFIX = "exression:";

        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="xFieldDefinition"></param>
        /// <param name="persistValue"></param>
        public SqlBusinessRule(CodeTreeHandler codeTrteeHandler,XElement xFieldDefinition, bool persistValue)
            : base(codeTrteeHandler, xFieldDefinition, persistValue)
        {
            int lineNumber = ((IXmlLineInfo)xFieldDefinition).LineNumber;

            try
            {
                // Получаем параметры конфигурации правила.
                var function = XFieldDefinition.XPathSelectElement("./function");

                if (function == null)
                    throw new ApplicationException(lineNumber +
                                                 "Не заданы параметры настройки вызова SQL-функции для расчета значения правила.");

                // Достаем название SQL-функции.
                var xFunctionName = function.XPathSelectElement("./name");

                if (xFunctionName == null || string.IsNullOrEmpty(xFunctionName.Value))
                    throw new ApplicationException(lineNumber +
                                                 "Не задано имя SQL-фунции, вызываемой для расчета значения правила.");

                FunctionName = xFunctionName.Value;

                // Получаем список параметров вызываемой функции.
                var functionParams = function.XPathSelectElements("./params/param");

                // Переходим к формированию списка параметров.
                FunctionParams = new Dictionary<string, string>();
                int paramNumber = 0;

                // Формируем список параметров с генерацией их идентификаторов.
                functionParams.ToList().ForEach(
                    p =>
                    FunctionParams.Add(string.Format("@p{0}", paramNumber++),
                                       p.Attributes().Where(
                                           a => a.Name.LocalName.Equals("value")).Select(
                                               a => a.Value).FirstOrDefault()));
            }
            catch (Exception ex)
            {
                throw new ApplicationException(lineNumber + "Ошибка разбора бизнес-правила на основе SQL скрипта из xml.",
                                             ex);
            }

        }

        /// <summary>
        /// Название SQL-функции для запуска вычислений.
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// Параметры SQL-функции для вычислений.
        /// </summary>
        public Dictionary<string, string> FunctionParams { get; set; }

        public override string FormatErrors()
        {
            return base.FormatErrors();
        }

        /// <summary>
        /// Осуществляет запуск проверки правила над указанным объектом.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override bool Match(ValidationData target)
        {
            // Задаем шаблон скрипта для вычисления значения SQL-функции.
            string query = "select %FUNCTION_NAME%(%PARAM_LIST%)";

            // Подставляем в шаблон имя функции.
            query = query.Replace("%FUNCTION_NAME%", FunctionName);
            // Подставляем в шаблон список параметров.
            query = query.Replace("%PARAM_LIST%", string.Join(", ", FunctionParams.Keys));

            // Далее формируем команду на выполнение, используя контекст доступа к данным, и осуществляем ее запуск.
            //var contextFactory = ObjectFactory.GetInstance<IDaoContextFactory>();
            //using (IDaoContext context = contextFactory.CreateContext())
            //{
            //    using (var command = context.CreateCommand())
            //    {
            //        command.CommandText = query;
            //        command.CommandType = CommandType.Text;

            //        // Добавляем к команде все параметры, использованные при формировании скрипта на запуск функции.
            //        var parms = FunctionParams.Select(p => command.AddParameterWithValue(p.Key, FormParamValue(target, p.Value)))
            //                      .ToArray();

            //        // Осуществляем запуск сформированной команды.
            //        Object result;
            //        try
            //        {
            //            result = command.ExecuteScalar();
            //        }
            //        catch (SqlException ex)
            //        {
            //            List<string> parmsStr = new List<string>();
            //            if (parms.Length > 0)
            //            {
            //                parmsStr = parms.Select(p => string.Format("{0}: {1}", p.ParameterName, p.Value.ToString())).ToList();
            //            }
            //            string msg = string.Format("Не удалось вызвать функцию '{0}'.\r\n Ошибка: {1}\r\n {2}.\r\n", query, ex.Message, (parmsStr.Count > 0 ? ("Параметры >> " + string.Join(", ", parmsStr)) : ""));
            //            throw new ParamNotFoundException(query, target, msg);
            //        }

            //        // Сохраняем полученное значение в целевое поле.
            //        SetDynamicFieldValue(target, result);
            //    }
            //}
            return true;
        }

        private object FormParamValue(object target, string paramDefinition)
        {
            // Если определение значения параметра не задано, то
            if (paramDefinition == null)
                // возвращаем пустое значение.
                return DBNull.Value;

            // Если определение значения параметра состоит из определения поля объекта, то
            if (paramDefinition.Contains(XML_FIELD_PREFIX))
            {
                // возвращаем значение указанного поля из объекта.
                var propVal = GetPropertyValue(target, paramDefinition.Replace(XML_FIELD_PREFIX, ""));
                if (propVal == null)
                {
                    //string msg = string.Format("Не удалось определить значение параметра {0}", paramDefinition);
                    //throw new ParamNotFoundException(paramDefinition, target, msg);
                    //return DBNull.Value;
                    return string.Empty;
                }
                return propVal;
            }

            // Если определение значения параметра состоит из определения вычислимого выражения, то
            //if (paramDefinition.Contains(XML_EXPRESSION_PREFIX))
            //{
            //    // осуществляем вычисление выражения над объектом и возвращаем полученный результат.
            //    var exprVal = DynamicExpressionHelper.Run(target, paramDefinition.Replace(XML_EXPRESSION_PREFIX, ""));
            //    if (exprVal == null)
            //    {
            //        string msg = string.Format("Не удалось определить значение параметра {0}", paramDefinition);
            //        throw new ParamNotFoundException(paramDefinition, target, msg);
            //    }

            //    return exprVal;
            //}
            // Во всех остальных случаях просто возвращаем указанное значение.
            return paramDefinition;
        }
    }
}
