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
	/// <summary>
	/// Бизнес-правило, представляющее собой вычисление значения поля по заданной формуле.
	/// </summary>
	/// <remarks>
	/// Шаблон XML используемый для объявления правила:
	/// <![CDATA[
	///<!-- Запись вычисления выражения, используя лямбда-выражения. -->
	///<target>
	///    <!-- Наименование вычислимого поля. Данный узел может отсутствовать, если в ходе вычисления значения, оно присваивается целевому полю. -->
	///    <name>FieldName</name>
	///    <!-- Описание вычислимого поля. Данный узел может отсутствовать. -->
	///    <description>FieldDescription</description>
	///    <!-- Лямбда-выражение для вычисления значения поля. -->
	///    <expression>Lambda Expression</expression>
	///</target>
	/// ]]>
	/// </remarks>
	public class LambdaBusinessRule : BusinessRule
	{
		/// <summary>
		/// Лямбда-выражение для вычисления.
		/// </summary>
		public string ExpressionString { get; private set; }

		/// <summary>
		/// Конструктор класса.
		/// </summary>
		/// <param name="xFieldDefinition"></param>
		/// <param name="persistValue"></param>
		public LambdaBusinessRule( CodeTreeHandler codeTreeHandler, System.Xml.Linq.XElement xFieldDefinition, bool persistValue)
			: base(codeTreeHandler, xFieldDefinition, persistValue)
		{
			int lineNumber = ((System.Xml.IXmlLineInfo)xFieldDefinition).LineNumber;

			try
			{
				var expression = xFieldDefinition.XPathSelectElement("./expression");

				if (expression == null || string.IsNullOrEmpty(expression.Value))
					throw new Exception(lineNumber + "Для бизнес-правила должно быть указано выражение для вычисления значения поля.");

				ExpressionString = expression.Value;
			}
			catch (Exception ex)
			{
				throw new Exception(lineNumber + "Ошибка разбора бизнес-правила на основе лямбда выражений из xml.", ex);
			}
		}

		/// <summary>
		/// Осуществляет запуск вычисления значения поля объекта.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public override bool Match(ValidationData target)
		{
			//// Вычисляем выражение над объектом.
			//var result = Helpers.DynamicExpressionHelper.Run(target, ExpressionString);
			//// Сохраняем вычисленное значение в целевом поле объекта.
			//SetDynamicFieldValue(target, result);

			return true;
		}

        public override string FormatErrors()
        {
           return base.FormatErrors();
        }
    }
}
