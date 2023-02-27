using RuleCompiller.Helpers;
using RuleCompiller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.ValidationException;
using System.Reflection;

namespace RuleCompiller.Validators
{
    public abstract class Rule
    {
        private const BindingFlags propertyBindingFlags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.SetProperty;
        public CodeTreeHandler codeTreeHandler;
        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="violationType"></param>
        protected Rule(RuleViolationType violationType, CodeTreeHandler codeThreeHandler)
        {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            this.codeTreeHandler = codeThreeHandler;
            ViolationType = violationType;
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        public string BuildValidatorCode(string errorExpression = "", string successExpression = "")
        {
            codeTreeHandler.validatorsStack.Push(this.GetType().Name);

            //Optimization part
            int counterTerminateDelimiter = 0;
            if (!string.IsNullOrEmpty(successExpression))
            {
                if (!CompileTool.IsExpressionInsideBlock(successExpression)) successExpression = "{" + successExpression + "}";
                //string temp = successExpression.Replace(" ", "").Replace(";;", ";");
                //counterTerminateDelimiter = 0;
                //for (int i = 0; i < temp.Length; i++)
                //{
                //    if (temp[i] == ';' && ++counterTerminateDelimiter > 1) break;
                //}
                //if (counterTerminateDelimiter == 0) successExpression += ';';
                //if (
                //    counterTerminateDelimiter > 1 && 
                //    successExpression[0] != '{' && 
                //    successExpression[successExpression.Length-1] != '}'
                //)

            }
            else successExpression = "";
            if (!string.IsNullOrEmpty(errorExpression))
            {
                if (!CompileTool.IsExpressionInsideBlock(errorExpression)) errorExpression = "{" + errorExpression + "}";
                //string temp = errorExpression.Replace(" ", "").Replace(";;", ";");
                //counterTerminateDelimiter = 0;
                //for (int i = 0; i < temp.Length; i++)
                //{
                //    if (temp[i] == ';' && ++counterTerminateDelimiter > 1) break;
                //}
                //if (counterTerminateDelimiter == 0) errorExpression += ';';
                //if (
                //    counterTerminateDelimiter > 1 &&
                //    errorExpression[0] != '{' &&
                //    errorExpression[successExpression.Length - 1] != '}'
                //) 
            }
            else errorExpression = "";

            string code = CompileValidatorCode(errorExpression, successExpression);
            codeTreeHandler.validatorsStack.Pop();
            return code;
        }

        protected virtual string CompileValidatorCode(string errorExpression, string successExpression = "")
        {
            return "";
        }

        protected virtual string FormatErrors(string propertyName)
        {
            return ErrorMessage.Replace("%FIELD_NAME%", propertyName);
        }

        public abstract string FormatErrors();
        /// <summary>
        /// Осуществляет запуск проверки правила над указанным объектом.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        //public abstract bool Match(object target);
        public abstract bool Match(ValidationData target);

        public string Name { get; set; }

        public string VerificationCode { get; set; }

        /// <summary>
        /// Необходимость выполнения правил для приложенных документов.
        /// </summary>
        public RuleSchemaType SchemaType { get; set; }

        /// <summary>
        /// Получает название указанного свойства объекта (обычно используется для формирования удобочитаемого сообщения об ошибках).
        /// </summary>
        /// <param name="businessObject"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected string GetPropertyName(List<DocumentSchema> businessObject, string propertyName)
        {
            try
            {
                string[] strings = propertyName.Split('.');
                object obj = businessObject;

                //if (obj is IValidatingObject)
                //    return WrapStringWithQuotationMarks(((IValidatingObject)obj).GetFieldName(propertyName));

                //foreach (var prop in strings.Take(strings.Length - 1))
                //    obj = obj.GetType().GetProperty(prop, propertyBindingFlags).GetValue(obj, null);

                return businessObject.Where(p => p.Name == propertyName).ToArray()[0].Name;

                //return WrapStringWithQuotationMarks(obj.GetType().GetProperty(strings.Reverse().First()).Name);
            }
            catch (Exception)
            {
                var msg = string.Format("Не удалось получить свойство '{0}' объекта '{1}'", propertyName, businessObject.ToString());
                throw new ParamNotFoundException(propertyName, businessObject, msg);
            }
        }

        private string WrapStringWithQuotationMarks(string value)
        {
            if (!value.StartsWith("\""))
                value = "\"" + value;
            if (!value.EndsWith("\""))
                value = value + "\"";
            return value;
        }

        /// <summary>
        /// Получает значение указанного свойства объекта.
        /// </summary>
        /// <param name="businessObject"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected object GetPropertyValue(object businessObject, string propertyName)
        {
            try
            {
                string[] strings = propertyName.Split('.');
                //достаём из поля объекта дефолтное значение, т.к. приори там не может быть иных данных для "before проверки"
                var obj = ((List<DocumentSchema>)businessObject).Where(p => p.Name==propertyName).ToArray()[0].DefaultValue;



                //if (obj is IValidatingObject)
                //    //    return ((IValidatingObject)obj).GetNullableFieldValue(propertyName);

                //foreach (var prop in strings)
                //    var obj1 = businessObject.GetType().GetProperty(prop, propertyBindingFlags).GetValue(obj, null);



                return obj;
            }
            catch (Exception)
            {
                var msg = string.Format("Не удалось получить значение свойства '{0}' объекта '{1}'", propertyName, businessObject.ToString());
                throw new ParamNotFoundException(propertyName, businessObject, msg);
            }
        }

        /// <summary>
        /// Получает тип схемы объекта
        /// </summary>
        /// <param name="businessObject"></param>
        /// <returns></returns>
        protected int GetSchemaType(object businessObject)
        {
            try
            {
                object obj = businessObject;

                if (obj is IValidatingObject)
                    return ((IValidatingObject)obj).GetSchemaType();
                else
                    return 0;
            }
            catch (Exception)
            {
                var msg = string.Format("Не удалось получить тип схемы объекта '{0}'", businessObject.ToString());
                throw new ValidationException(msg);
            }
        }

        /// <summary>
        /// Устанавливает значение указанного свойства объекта.
        /// </summary>
        /// <param name = "businessObject" ></ param >
        /// < param name="propertyName"></param>
        /// <param name = "value" ></ param >
        /// < param name="persistence"></param>
        /// <param name = "dataType" > Тип данных поля.Используется для временных полей.</param>
        protected void SetPropertyValue(object businessObject, string propertyName, object value, bool persistence, string dataType)
        {
            string[] strings = propertyName.Split('.');
            object obj = businessObject;

            if (obj is IValidatingObject)
                ((IValidatingObject)obj).SetFieldValue(propertyName, value, persistence, dataType);
            else
            {
                foreach (var prop in strings.Take(strings.Count() - 1))
                    obj = obj.GetType().GetProperty(prop, propertyBindingFlags).GetValue(obj, null);

                var property = strings.Reverse().First();
                obj.GetType().GetProperty(property, propertyBindingFlags).SetValue(obj, value, null);
            }
        }

        /// <summary>
        /// Устанавливает название указанного свойства объекта (обычно используется для указания описания для временных расчетных полей).
        /// </summary>
        /// <param name="businessObject"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        protected void SetPropertyName(object businessObject, string propertyName, string value)
        {
            object obj = businessObject;

            if (obj is IValidatingObject)
                ((IValidatingObject)obj).SetFieldName(propertyName, value);
            else
                throw new ApplicationException("Объект не поддерживает установку описания поля");
        }

        /// <summary>
        /// Заголовок сообщения об ошибке.
        /// </summary>
        public string ErrorHeader { get; protected set; }

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string ErrorMessage { get; protected set; }

        /// <summary>
        /// Тип ошибки правила.
		/// </summary>
		public virtual RuleViolationType ViolationType { get; set; }

    }

    /// Перечисление типов данных, используемых при проверке значений свойств объектов.
    public enum ValidationDataType
    {
        String,
        Integer,
        Double,
        Decimal,
        Date,
        Bit
    }

    public enum ValidationOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanEqual,
        LessThan,
        LessThanEqual
    }

    public static class ValidationOperatorExtern
    {
        public static string GetName(this ValidationOperator value)
        {
            switch (value)
            {
                case ValidationOperator.Equal: return "равно";
                case ValidationOperator.NotEqual: return "не равно";
                case ValidationOperator.LessThanEqual: return "меньше или равно";
                case ValidationOperator.GreaterThanEqual: return "больше или равно";
                case ValidationOperator.LessThan: return "меньше, чем";
                case ValidationOperator.GreaterThan: return "больше, чем";
            }
            return "";
        }
    }
}
