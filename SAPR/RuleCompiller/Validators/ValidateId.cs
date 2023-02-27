using RuleCompiller.Validators;
using RuleCompiller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Validators
{
	/// <summary>
	/// Правило для проверки автоинкрементного поля (идентификатора). 
	/// Значение поля должно быть больше или рано нулю.
	/// </summary>
	public class ValidateId : ValidationRule
	{
		public ValidateId(CodeTreeHandler codeTreeHandler, string propertyName)
			: this(codeTreeHandler, propertyName, null, "Значение свойства %FIELD_NAME% не является корректным идентификатором.")
		{
		}

		public ValidateId(CodeTreeHandler codeTreeHandler, string propertyName, string errorHeader, string errorMessage)
			: base(codeTreeHandler, propertyName, errorHeader, errorMessage)
		{
		}

		public override bool Match(ValidationData data)
		{
			string propValue = data[PropertyName];

			// Дополнительно условие по требованию: "если поле не заполнено, то не поднимать сообщение об ошибке".
			if (string.IsNullOrEmpty(propValue)) return true;

			if(long.TryParse(propValue, out long id) && id >= 0) return true;

            ErrorMessage = ErrorMessage.Replace("%FIELD_NAME%", '"' + PropertyName + '"');
			return false;
        }

		protected override string CompileValidatorCode(string errorExpression = null, string successExpression = null)
		{
			if (string.IsNullOrEmpty(errorExpression)) errorExpression = codeTreeHandler.violationsVariableName + ".Add(" + CompileTool.BuildString(FormatErrors()) + ");";

			StringBuilder codeText = new StringBuilder();
			string variableName = codeTreeHandler.GetUniqueVariableName(PropertyName);
			string outVariableName = codeTreeHandler.GetUniqueVariableName(PropertyName+"OutValue");
			codeText.Append("if(");
			codeText.Append("!string.IsNullOrEmpty("+ variableName + ") && (!long.TryParse("+ variableName + ", out long "+ outVariableName + ") || "+ outVariableName + " < 0)");
			codeText.Append(")" + errorExpression);

			if (!CompileTool.IsUnuselessExpression(successExpression)) codeText.Append("else " + successExpression);

			codeTreeHandler.AddProperty(PropertyName);

			return codeText.ToString();
		}
	}
}
