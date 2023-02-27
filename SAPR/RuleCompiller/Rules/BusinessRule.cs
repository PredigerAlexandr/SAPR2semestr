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

namespace RuleCompiller.Rules
{
	/// <summary>
	/// Обобщенное бизнес-правило вычисления значения поля.
	/// </summary>
	/// <remarks>
	/// Шаблон XML, использумый при объявлении правил данного типа:
	/// <![CDATA[
	///<!-- Общая схема описания простого бизнес-правила. Обычно используется перед запуском указанных проверок. -->
	///<rule type="business" mode="persistence|temporary">
	///    <target>
	///			...
	///    </target>
	///</rule>
	/// ]]>
	/// </remarks>
	public abstract class BusinessRule : Rule
	{
		/// <summary>
		/// Название целевого поля (если необходимо).
		/// </summary>
		public string FieldName { get; private set; }
		/// <summary>
		/// Описание целевого поля (если необходимо).
		/// </summary>
		public string FieldDescription { get; private set; }
		/// <summary>
		/// Признак того является ли целевое поле постоянным или временным.
		/// </summary>
		public bool PersistValue { get; private set; }
		/// <summary>
		/// Тип данных поля. Используется для временных полей.
		/// </summary>
		public string DataType { get; private set; }

		/// <summary>
		/// XML-узел с параметрами бизнес-правила.
		/// </summary>
		protected XElement XFieldDefinition { get; set; }

		/// <summary>
		/// Конструктор класса.
		/// </summary>
		/// <param name="xFieldDefinition"></param>
		/// <param name="persistValue"></param>
		protected BusinessRule(CodeTreeHandler codeTreeHandler, XElement xFieldDefinition, bool persistValue)
			: base(RuleViolationType.Error, codeTreeHandler)
		{
			// ReSharper disable AssignNullToNotNullAttribute
			var targetName = xFieldDefinition.XPathSelectElement("./name");
			var targetDescription = xFieldDefinition.XPathSelectElement("./description");
			var targetDataType = xFieldDefinition.XPathSelectElement("./type");
			// ReSharper restore AssignNullToNotNullAttribute

			XFieldDefinition = xFieldDefinition;
			FieldName = targetName == null ? string.Empty : targetName.Value;
			FieldDescription = targetDescription == null ? string.Empty : targetDescription.Value;
			DataType = targetDataType == null ? "text" : targetDataType.Value;
			PersistValue = persistValue;
		}

		public static Rule Parse(XElement xRule, CodeTreeHandler codeTreeHandler)
		{
			int lineNumber = ((IXmlLineInfo)xRule).LineNumber;

			try
			{
				var result = new RuleComposite(codeTreeHandler);

				var mode = xRule.Attribute("mode");

				if (mode == null)
					throw new RuleParseException(lineNumber, "Не указан тип бизнес-правила (временное значение или сохраняемое).");

				// ReSharper disable PossibleNullReferenceException
				var isPersistenceMode = mode.Value.Equals("persistence");
				// ReSharper restore PossibleNullReferenceException

				// Тут тегов target может быть несколько, т.к. бизнес-правила можно группировать по типу хранения значений полей.
				foreach (var target in xRule.XPathSelectElements("./target"))
				{
					// Получаем соответствующие настройки бизнес-правила.
					var function = target.XPathSelectElement("./function");
					var expression = target.XPathSelectElement("./expression");

					// Проверяем, что указан хотя бы один параметр конфигурации вычисления значения поля, но не оба сразу.
					if (function == null && expression == null)
						throw new RuleParseException(lineNumber, "Не указано объявление поля с параметрами вычисления значения и его хранения.");

					if (function != null && expression != null)
						throw new RuleParseException(lineNumber, "Указаны неверные параметры бизнес-правила: поле не может одновременно быть вычислимым через SQL-функции и через Lambda-выражения.");

					// Если задана настройка вычисления через SQL-функции, то создаем правило вычисления через SQL.
					if (function != null)
						result.Add(new SqlBusinessRule(codeTreeHandler, target, isPersistenceMode));

					// Если задана настройка вычисления через динамическое выражение, то создаем правило вычисления через Lambda-выражения.
					if (expression != null)
						result.Add(new LambdaBusinessRule(codeTreeHandler, target, isPersistenceMode));
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new RuleParseException(lineNumber, "Ошибка разбора бизнес-правила из xml.", ex);
			}
		}

		protected void SetDynamicFieldValue(object target, object result)
		{
			// Если указано целевое поле, то
			if (!string.IsNullOrEmpty(FieldName))
			{
				// Сохраняем вычисленное значение в указанное поле.
				SetPropertyValue(target, FieldName, result, PersistValue, DataType);

				// Если для указанного поля задано описание, то
				if (!string.IsNullOrEmpty(FieldDescription))
					// Устанавливаем описание вычислимого поля.
					SetPropertyName(target, FieldName, FieldDescription);
			}
		}

        public override string FormatErrors()
        {
			return ErrorMessage;
        }
    }
}
