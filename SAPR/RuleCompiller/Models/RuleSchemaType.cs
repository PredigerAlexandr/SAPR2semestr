using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Models
{
	/// <summary>
	/// Тип сообщения правила.
	/// </summary>
	public enum RuleSchemaType
	{
		/// <summary>
		/// Все схемы документа
		/// </summary>
		All = 0,
		/// <summary>
		/// Только главная схема документа.
		/// </summary>
		MainOnly = 1,
		/// <summary>
		///  Только схемы приложенных документов
		/// </summary>
		AttachOnly = 2
	}
}
