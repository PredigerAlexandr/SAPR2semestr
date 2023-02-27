using AST.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Models
{
	/// <summary>
	/// Исключение валидации
	/// </summary>
	[Serializable]
	public class ValidationException : AstValidationException
	{
		#region Properties
		/// <summary>
		/// Сообщения
		/// </summary>
		public IEnumerable<string> ValidationMessages { get; private set; }

		/// <summary>
		/// Сообщение
		/// </summary>
		public string OriginalMessage { get; private set; }

		/// <summary>
		/// Идентификатор закупки
		/// </summary>
		public long? PurchaseId { get; private set; }

		/// <summary>
		/// Идентификатор лота
		/// </summary>
		public long? BidId { get; private set; }

		/// <summary>
		/// Идентификатор договора
		/// </summary>
		public long? ContractId { get; private set; }

		/// <summary>
		/// Идентификатор схемы
		/// </summary>
		public long? MainSchemaObjectId { get; private set; }

		/// <summary>
		/// Тип схемы 
		/// </summary>
		public string MainSchemaObjectType { get; private set; }
		#endregion

		#region Ctor

		/// <summary>
		/// Конструктор инициализации исключения
		/// </summary>
		/// <param name="message">сообщение</param>
		public ValidationException(string message)
			: base(message)
		{
			OriginalMessage = message;
			ValidationMessages = new List<string>();
		}

		/// <summary>
		/// Конструктор инициализации исключения
		/// </summary>
		/// <param name="message">сообщение</param>
		/// <param name="innerException">внутреннее исключение</param>
		public ValidationException(string message, Exception innerException)
			: base(message, innerException)
		{
			OriginalMessage = message;
			ValidationMessages = new List<string>();
		}

		/// <summary>
		/// Конструктор инициализации исключения
		/// </summary>
		/// <param name="message">сообщение</param>
		/// <param name="validationMessages">сообщения</param>
		/// <param name="innerException">внутреннее исключение</param>
		public ValidationException(string message, IEnumerable<string> validationMessages, Exception innerException)
			: base(GetMessage(message, validationMessages), innerException)
		{
			OriginalMessage = message;
			ValidationMessages = validationMessages;
		}

		/// <summary>
		/// Конструктор инициализации исключения
		/// </summary>
		/// <param name="info">контейнер данных</param>
		/// <param name="context">поток для сериализации</param>
		protected ValidationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info != null)
			{
				ValidationMessages = (IEnumerable<string>)info.GetValue("ValidationMessages", typeof(IEnumerable<string>));
				OriginalMessage = info.GetString("OriginalMessage");
				PurchaseId = info.GetInt64("PurchaseId");
				BidId = info.GetInt64("BidId");
				ContractId = info.GetInt64("ContractId");
				MainSchemaObjectId = info.GetInt64("MainSchemaObjectId");
				MainSchemaObjectType = info.GetString("MainSchemaObjectType");
			}
		}
		/// <summary>
		/// Конструктор инициализации исключения
		/// </summary>
		/// <param name="message">сообщение</param>
		/// <param name="purchaseId">идентификатор закупки</param>
		/// <param name="bidId">идентификатор лота</param>
		/// <param name="mainSchemaObjectId">идентификатор схемы</param>
		/// <param name="objectType">тип схемы</param>
		public ValidationException(string message, long purchaseId, long bidId, long? mainSchemaObjectId = null, string objectType = null, long? сontractId = null)
			: this(message, new List<string>(), mainSchemaObjectId, purchaseId, bidId, objectType, сontractId) { }

		public ValidationException(string message, IEnumerable<string> validationMessages, long? mainSchemaObjectId = null, long? purchaseId = null, long? bidId = null, string objectType = null, long? сontractId = null) :
			base(GetMessage(message, validationMessages))
		{
			OriginalMessage = message;
			ValidationMessages = validationMessages;
			MainSchemaObjectId = mainSchemaObjectId;
			PurchaseId = purchaseId;
			ContractId = сontractId;
			BidId = bidId;
			MainSchemaObjectType = objectType;
		}



		#endregion

		#region Func

		/// <summary>
		/// Функция вернет сообщение
		/// </summary>
		/// <param name="onlyMessage">флаг - только сообщение</param>
		/// <param name="generateHtml">флаг - генерировать html</param>
		/// <returns></returns>
		public string ToString(bool onlyMessage, bool generateHtml)
		{
			if (generateHtml)
			{
				var result = new StringBuilder();
				result.Append("<ValidationResult>");
				result.Append("<ValidationHeader>");
				result.Append(OriginalMessage);
				result.Append("</ValidationHeader>");

				result.Append("<ValidationMessages>");

				if (ValidationMessages != null)
				{
					foreach (var validationMessage in ValidationMessages)
						result.AppendFormat("<ValidationMessage>{0}</ValidationMessage>", validationMessage);
				}

				result.Append("</ValidationMessages>");

				if (!onlyMessage)
				{
					var stacks = new List<string>();

					Exception ex = this;
					while (ex != null)
					{
						stacks.Add(ex.StackTrace);
						ex = ex.InnerException;
					}

					result.AppendFormat("<StackTrace>{0}</StackTrace>", string.Join("\n", stacks));
				}
				result.Append("</ValidationResult>");
				return result.ToString();
			}
			return onlyMessage ? Message : ToString();
		}

		/// <summary>
		/// Метод заполнит данные для сериализации исключения
		/// </summary>
		/// <param name="info">контейнер данных</param>
		/// <param name="context">поток для сериализации</param>
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("ValidationMessages", ValidationMessages);
			info.AddValue("OriginalMessage", OriginalMessage);
			info.AddValue("PurchaseId", PurchaseId);
			info.AddValue("BidId", BidId);
			info.AddValue("ContractId", ContractId);
			info.AddValue("MainSchemaObjectId", MainSchemaObjectId);
			info.AddValue("MainSchemaObjectType", MainSchemaObjectType);
		}

		static string GetMessage(string message, IEnumerable<string> validationMessages)
		{
			return string.Format("{0} Список ошибок: {1}{2}", message, Environment.NewLine, validationMessages != null ? string.Join(Environment.NewLine, validationMessages) : string.Empty);
		}
		#endregion
	}
}
