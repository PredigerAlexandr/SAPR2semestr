using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Helpers
{
	/// <summary>
	/// Интерфейс объекта, предоставляющего доступ к значениям своих свойств.
	/// </summary>
	public interface IValidatingObject
	{
		object GetFieldValue(string fieldName);
		object GetNullableFieldValue(string fieldName);
		string GetFieldName(string fieldName);

		object SetFieldValue(string fieldName, object newValue, bool persistence, string dataType);
		object SetFieldValue(string fieldName, object newValue, bool persistence);
		void SetFieldName(string fieldName, string newDescription);

		DateTime GetDateTime(string fieldName);
		DateTime? GetNullableDateTime(string fieldName);

		int GetInt(string fieldName);
		int? GetNullableInt(string fieldName);

		long GetLong(string fieldName);
		long? GetNullableLong(string fieldName);

		decimal GetDecimal(string fieldName);
		decimal? GetNullableDecimal(string fieldName);

		bool GetBoolean(string fieldName);
		bool? GetNullableBoolean(string fieldName);

		string XPathEval(string fieldName, string xPath);

		int GetSchemaType();
	}
}
