using Norbit.ValidationFramework.DataTransferObjects;
using RuleCompiller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Validators
{
	/// <summary>
	/// Интерфейс компоновщика правил в одно правило.
	/// </summary>
	public interface IRuleComposite
	{
		/// <summary>
		/// Признак того, что при последней проверке правил были сообщения с нарушениями уровня Error.
		/// </summary>
		bool HasViolations { get; }

		/// <summary>
		/// Список нарушений, выявленных при последней проверке правил.
		/// </summary>
		IEnumerable<Models.ValidationMessageData> ViolationMessages { get; }

		/// <summary>
		/// Количество правил, содержащихся в компоновщике.
		/// </summary>
		int RuleCount { get; }
	}
}
