using AST.Core.Exceptions;
using AST.Core.Helpers;
using Norbit.Dsr.BusinessTrade.DocumentShema;
using Norbit.NBT.ActionService.Client;
using Norbit.NBT.Dao.Helpers;
using Norbit.NBT.Extensions;
using Norbit.NBT.Plugins;
using Norbit.ValidationFramework;
using Norbit.ValidationFramework.DataTransform;
using Norbit.ValidationFramework.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RuleCompiller.Models
{
    [Serializable]
    public class ShemaObject : MarshalByRefObject, ISchemaObject, IValidatingObject
    {
        #region константы

        private const string ID_CONST = "id";
        private const string UID_CONST = "uid";

        #endregion

        /// <summary>
        /// Наименование поля - внешнего ключа. Например для объекта Bid связанного с закупкой это может быть PurchaseId
        /// </summary>
        public string NavigationFieldCode { set; get; }
        /// <summary>
        /// Ссылка на родительский объект. Например для объекта Bid это будет Purchase
        /// </summary>
        public ShemaObject ParentObject { set; get; }
        /// <summary>
        /// Список дочерних объектов. Например для лота это могут быть позиции лота,периоды(если они заполняются для каждого лота)
        /// </summary>
        public List<ShemaObject> ChildObjects { set; get; }
        /// <summary>
        /// Идентификатор объекта(Значение поля Id  в таблице)
        /// </summary>
        public Int64 Id { set; get; }
        /// <summary>
        /// Глобальный Уникальный Идентификатор объекта(Значение поля Guid  в таблице)
        /// </summary>
        public Guid UID { set; get; }
        /// <summary>
		/// Список всех полей объекта
		/// </summary>
		public List<DocumentField> DocumentFields { set; get; }
        /// <summary>
        /// Название таблицы которой соответствует объект
        /// </summary>
        public string EntityName { set; get; }
        /// <summary>
        /// Xml верхним тегом которого является название таблицы, а все вложенные элементы - все поля объекта с их значениями
        /// </summary>
        public string XmlView { set; get; }
        /// <summary>
        ///  Тег во входящем на обработку xml по которому лежит объект(для случае нескольких объектов на одном уровне и для объектов являющихся дочерними)
        /// </summary>
        public string XmlPath { set; get; }
        /// <summary>
        /// Идентификатор обрабатываемого документа (Необходимо для сохранения идентификатора в таблице с объектом, чтобы там была ссылка на документ из которого появился или обновился объект)
        /// </summary>
        public Int64 DocumentId { set; get; }
        /// <summary>
        /// Признак того что объект был удален
        /// </summary>
        public bool IsDeleted { set; get; }

        /// <summary>
        /// Признак того, что объект не может быть удален, если истина
        /// </summary>
        public bool isNotRemove { set; get; }

        /// <summary>
        /// Признак того что объект используется только для чтения(без сохранения в БД). Но этот объект зхаполняется отдельным запросом из БД
        /// </summary>
        public bool IsReadOnly { set; get; }
        /// <summary>
        ///  Признак того что данный объект является главным, и его идентификатор сохраниться в документе (ObjectId).
        /// </summary>
        public bool IsBaseObject { get; set; }

        /// <summary>
        ///  Признак того что данный объект является сервисным, и по нему ведется отсчет использования услуг 
        /// </summary>
        public bool IsServiceObject { get; set; }

        /// <summary>
        ///  Признак того что данный объект является кросс-объектом. Его описание хранится в ТС "Main", изменение его свойств из других секций запрещено.
        /// </summary>
        public bool IsCrossObject { get; set; }

        /// <summary>
        ///  Признак того что если объект не был заполнен на этапе парсинга, то его следует удалить.
        /// </summary>
        public bool DeleteIfEmpty { get; set; }

        /// <summary>
        /// Признак того, что объект заполнен данными из xml.
        /// </summary>
        public bool IsParsedFromXml { get; set; }

        /// <summary>
        /// Тип схемы (основная/приложенный документ)
        /// </summary>
        public int SchemaType { get; set; }
        /// <summary>
        /// Уникальные ключи, идентифицирующие объект
        /// </summary>
        public string UniqueKey { set; get; }

        /// <summary>
        /// Порядковый номер объекта (для однотипных объектов)
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Признак несохранения данных в DocumentObject
        /// </summary>
        public bool DoNotSaveToDocumentObject { get; set; }

        /// <summary>
        /// Генерировать исключение при обращении к полю, отсутствующему у объекта
        /// </summary>
        public bool RiseFieldNotFoundException { get; set; }

        /// <summary>
        /// IssueID #144854
        /// 2022-07-14 - Снегирь М.С.
        /// Дополнительное логирование данных 
        /// </summary>
        private StringBuilder DataSaveExtendedLog { get; set; }

        public void AppendDataSaveExtendedLog(string text)
        {
            if (DataSaveExtendedLog == null)
            {
                DataSaveExtendedLog = new StringBuilder();
                AppendDataSaveExtendedLog("Scheme object extended log #144854");
            }

            DataSaveExtendedLog.AppendLine(string.Format("&&&&&&&&&& {0}", text));
        }

        public bool HasDataSaveExtendedLogMessage
        {
            get
            {
                return DataSaveExtendedLog?.Length > 0;
            }
        }

        public string DataSaveExtendedLogMessage
        {
            get
            {
                return DataSaveExtendedLog?.ToString();
            }
        }

        public void ClearDataSaveExtendedLog()
        {
            if (DataSaveExtendedLog != null)
                DataSaveExtendedLog.Clear();
        }


        private List<ShemaObject> _descendingObjects;

        public List<ShemaObject> DescendingObjects
        {
            get
            {
                if (_descendingObjects == null)
                    _descendingObjects = GetDescendingObjects();
                return _descendingObjects;
            }
        }

        public int GetSchemaType()
        {
            return SchemaType;
        }

        /// <summary>
		/// Клонирование объекта(Реализовать позднее стандартный интерфейс)
		/// </summary>
		/// <returns></returns>
		public ShemaObject CloneShemaObject()
		{
			var returnShemaObject = new ShemaObject
										{
											NavigationFieldCode = NavigationFieldCode,
											IsDeleted = IsDeleted,
                                            isNotRemove = isNotRemove,
											IsReadOnly = IsReadOnly,
											IsBaseObject = IsBaseObject,
                                            IsCrossObject = IsCrossObject,
                                            DeleteIfEmpty = DeleteIfEmpty,
											EntityName = EntityName,
											XmlPath = XmlPath,
											XmlView = XmlView,
											Id = Id,
											DocumentId = DocumentId,
											ParentObject = ParentObject,
											DocumentFields =
												DocumentFields.Select(documentField => documentField.CloneField()).
												ToList(),
                                            SortOrder = 0,
                                            SchemaType = SchemaType,
                                            UniqueKey = UniqueKey,
											ChildObjects =
											!ChildObjects.Any()
												? new List<ShemaObject>()
												: ChildObjects.Select(shemaObject => shemaObject.CloneShemaObject()).ToList(),
                                            DoNotSaveToDocumentObject = DoNotSaveToDocumentObject
            };
			return returnShemaObject;
		}

        /// <summary>
        /// Возвращает поле из набора полей
        /// </summary>
        /// <param name="fieldName">имя поля/переменной</param>
        /// <returns></returns>
        private DocumentField GetField(string fieldName)
        {
            string[] strings = fieldName.ToLower().Split('.');

            //сюда пойдем если поле участвует в проверке(своя специфика наименования, с точками. Например Purchase.Name)
            if (strings.Length > 1)
                return
                    ChildObjects.Where(
                        c => c.EntityName.ToLower() == strings[0]).Select(
                            c => c.GetField(string.Join(".", strings.Skip(1)))).First();
            string s = "";
            foreach (var item in DocumentFields)
            {
                
            }
            return DocumentFields.Where(f => f.Name.ToLower() == strings[0]).FirstOrDefault();
        }

        /// <summary>
        /// Возвращает символьное значение поля  схемы (Field.Value) по его наименованию(Field)
        /// </summary>
        /// <param name="fieldName">Наименование поля</param>
        /// <returns></returns>
        public object GetFieldValue(string fieldName)
        {
            return GetFieldValue(fieldName, false);
        }

        /// <summary>
        /// Возвращает символьное значение поля  схемы (Field.Value) по его наименованию(Field)
        /// </summary>
        /// <param name="fieldName">Наименование поля</param>
        /// <param name="allowNullValue">Разрешить значения null</param>
        /// <returns></returns>
        private object GetFieldValue(string fieldName, bool allowNullValue)
		{
            try
            {
                //IssueID #92803
                //2020-07-02 - Снегирь М.С.
                //Получаем поле из списка полей объекта
                var field = GetField(fieldName);

                if (field == null)
                {
                    if (RiseFieldNotFoundException)
                        throw new AstFieldNotFoundException(fieldName);
                    else
                    {
                        string errorMessage = string.Format("Проверяемый объект: {0}. Объект не содержит поле \"{1}\".", this, fieldName);

                        //LogHelper.Logger.DebugAndDB(errorMessage);
                        return null;
                    }
                }

                //Проверка на заполненность поля
                if (!allowNullValue && string.IsNullOrWhiteSpace(field.Value) && field.Type != "bit")
                    throw new AstFieldValueNullException(fieldName);

                return ValueTypeConverter.Convert(field.Value, field.Type);
            }
            catch (Exception ex) {

                if (ex is AstFieldNotFoundException || ex is AstFieldValueNullException)
                    throw;

                else
                    throw new AstInvalidCastException(string.Format("Поле \"{0}\". {1}", fieldName, ex.Message));

            }
		}

        /// <summary>
        /// Возвращает предыдущее символьное значение поля  схемы (Field.Value) по его наименованию(Field)
        /// </summary>
        /// <param name="fieldName">Наименование поля</param>
        /// <returns></returns>
        public object GetPreviousFieldValue(string fieldName)
        {
            return GetPreviousFieldValue(fieldName, false);
        }

        /// <summary>
        /// Возвращает предыдущее символьное значение поля  схемы (Field.Value) по его наименованию(Field)
        /// </summary>
        /// <param name="fieldName">Наименование поля</param>
        /// <param name="allowNullValue">Разрешить значения null</param>
        /// <returns></returns>
        private object GetPreviousFieldValue(string fieldName, bool allowNullValue)
        {
            try
            {
                //IssueID #92803
                //2020-07-02 - Снегирь М.С.
                //Получаем поле из списка полей объекта
                var field = GetField(fieldName);

                if (field == null)
                {
                    if (RiseFieldNotFoundException)
                        throw new AstFieldNotFoundException(fieldName);
                    else
                    {
                        string errorMessage = string.Format("Проверяемый объект: {0}. Объект не содержит поле \"{1}\".", this, fieldName);

                        //LogHelper.Logger.DebugAndDB(errorMessage);
                        return null;
                    }
                }

                //Проверка на заполненность поля
                if (!allowNullValue && string.IsNullOrWhiteSpace(field.PrevValue) && field.Type != "bit")
                    throw new AstFieldValueNullException(fieldName, previous: true);

                return ValueTypeConverter.Convert(field.PrevValue, field.Type);
            }
            catch (Exception ex)
            {
                if (ex is AstFieldNotFoundException || ex is AstFieldValueNullException)
                    throw;

                else
                    throw new AstInvalidCastException(string.Format("Поле \"{0}\". {1}", fieldName, ex.Message));
            }
        }

        public object GetPreviousNullableFieldValue(string fieldName)
        {
            //IssueID #92803
            //2020-07-02 - Снегирь М.С.
            //Получаем поле из списка полей объекта
            var field = GetField(fieldName);

            if (field == null)
            {
                if (RiseFieldNotFoundException)
                    throw new AstFieldNotFoundException(fieldName);
                else
                {
                    string errorMessage = string.Format("Проверяемый объект: {0}. Объект не содержит поле \"{1}\".", this, fieldName);

                    //LogHelper.Logger.DebugAndDB(errorMessage);
                    return null;
                }
            }

            return (string.IsNullOrEmpty(field.PrevValue) && field.Type != "bit")
                ? null
                : ValueTypeConverter.Convert(field.PrevValue, field.Type);
            
        }

        public object GetNullableFieldValue(string fieldName)
        {
            //IssueID #92803
            //2020-07-02 - Снегирь М.С.
            //Получаем поле из списка полей объекта
            var field = GetField(fieldName);

            return field == null || (string.IsNullOrEmpty(field.Value) && field.Type != "bit")
                ? null
                : ValueTypeConverter.Convert(field.Value, field.Type);
        }

        public object GetNullablePreviousFieldValue(string fieldName)
        {
            //IssueID #92803
            //2020-07-02 - Снегирь М.С.
            //Получаем поле из списка полей объекта
            var field = GetField(fieldName);

            if (field == null)
            {
                if (RiseFieldNotFoundException)
                    throw new AstFieldNotFoundException(fieldName);
                else
                {
                    string errorMessage = string.Format("Проверяемый объект: {0}. Объект не содержит поле \"{1}\".", this, fieldName);

                    //LogHelper.Logger.DebugAndDB(errorMessage);
                    return null;
                }
            }

            return (string.IsNullOrEmpty(field.PrevValue) && field.Type != "bit")
                ? null
                : ValueTypeConverter.Convert(field.PrevValue, field.Type);
        }

        /// <summary>
        /// Возвращает описание поля
        /// </summary>
        /// <param name="fieldName">Имя поля/переменной</param>
        /// <returns></returns>
		public string GetFieldName(string fieldName)
		{
            //IssueID #92803
            //2020-07-02 - Снегирь М.С.
            //Получаем поле из списка полей объекта
            var field = GetField(fieldName);
            /*
            if (field == null)
            {
                if (RiseFieldNotFoundException)
                    throw new AstFieldNotFoundException(fieldName);
                else
                {
                    string errorMessage = string.Format("Проверяемый объект: {0}. Объект не содержит поле \"{1}\".", this, fieldName);

                    ErrorLogHelper.WriteDebug(Logger, errorMessage);
                    return null;
                }
            }
            */
            return field?.Description;

        }
        
        /// <summary>
        /// Устанавливает новое значение полю по его наименованию
        /// </summary>
        /// <param name="fieldName">Наименование поля</param>
        /// <param name="newValue">Новое значение поля</param>
        /// <param name="persistence">Признак необходимости сохрянять поле в БД(Для вычислимых полей)</param>
        /// <param name="dataType">Тип динамического поля, по-умолчанию, равный 'text'</param>
        public object SetFieldValue(string fieldName, object newValue, bool persistence, string dataType)
		{
            //IssueID #92803
            //2020-07-02 - Снегирь М.С.
            //Получаем поле из списка полей объекта
            var field = GetField(fieldName);

            try
            {
                if (persistence)
                {
                    if (field == null)
                    {
                        if (RiseFieldNotFoundException)
                            throw new AstFieldNotFoundException(fieldName);
                        else
                        {
                            string errorMessage = string.Format("Проверяемый объект: {0}. Объект не содержит поле \"{1}\".", this, fieldName);

                            //LogHelper.Logger.DebugAndDB(errorMessage);
                            return null;
                        }
                    }

                    field.Value = Convert.ToString(newValue);
                }
                else
                {

                    if (field == null)
                        DocumentFields.Add(new DocumentField { Name = fieldName, Value = Convert.ToString(newValue), IsTemporaty = true, IsRequired = false, IsStatic = true, Type = dataType, Code = fieldName });
                    else
                        field.Value = Convert.ToString(newValue);
                }

                if (fieldName.ToLower() == ID_CONST.ToLower())
                {
                    var strValue = Convert.ToString(newValue);
                    this.Id = string.IsNullOrEmpty(strValue) ? 0 : long.Parse(strValue);
                }

                if (fieldName.ToLower() == UID_CONST.ToLower())
                {
                    var strValue = Convert.ToString(newValue);
                    this.UID = string.IsNullOrEmpty(strValue) ? Guid.Empty : Guid.Parse(strValue);
                }

            }
            catch(Exception ex)
            {
                if (ex is AstFieldNotFoundException)
                    throw;

                var msg = string.Format("Не удалось установить значение '{0}' для поля '{1}'", newValue, fieldName);
                throw new AstInvalidOperationException(msg);
            }

            return newValue;
        }

        public object SetFieldValue(string fieldName, object newValue, bool persistence)
        {
            return SetFieldValue(fieldName, newValue, persistence, "text");
        }

        /// <summary>
        /// Устанавливает новое значение полю по его наименованию
        /// </summary>
        /// <param name="fieldName">Наименование поля</param>
        /// <param name="newDescription">Новое название (описание) поля</param>
        public void SetFieldName(string fieldName, string newDescription)
		{
            //IssueID #92803
            //2020-07-02 - Снегирь М.С.
            //Получаем поле из списка полей объекта
            var field = GetField(fieldName);


            if (field == null)
            {
                if (RiseFieldNotFoundException)
                    throw new AstFieldNotFoundException(fieldName, "Для задания описания поля, оно должно существовать среди полей объекта. Для создания временных полей испульзуйте сначала SetFieldValue");
                else
                {
                    string errorMessage = string.Format("Проверяемый объект: {0}. Объект не содержит поле \"{1}\".", this, fieldName);

                    //LogHelper.Logger.DebugAndDB(errorMessage);
                    return;
                }
            }

            field.Description = newDescription;
		}

        public DateTime GetDateTime(string fieldName)
        {
            var value = GetFieldValue(fieldName);
            return ((DateTime)ValueTypeConverter.Convert(value.ToString(), "date"));
        }

        public int GetDayOfWeek()
        {
            var day = (int)DateTime.Now.DayOfWeek;
            return day == 0 ? 7 : day;
        }

        public string GetString(string fieldName)
        {
            var value = GetNullableFieldValue(fieldName);

            if (value == null)
                return String.Empty;
            else return value.ToString();
        }

        public DateTime? GetNullableDateTime(string fieldName)
        {
            var value = GetNullableFieldValue(fieldName);
            if (value == null) return null;

            var strValue = value.ToString();
            if (string.IsNullOrEmpty(strValue)) return null;

            //return DateTime.Parse(value.ToString());
            return ((DateTime)ValueTypeConverter.Convert(value.ToString(), "date"));
        }

        public int GetInt(string fieldName)
        {
            var value = GetFieldValue(fieldName);
            return int.Parse(value.ToString());
        }

        public int? GetNullableInt(string fieldName)
        {
            var value = GetNullableFieldValue(fieldName);
            if (value == null) return null;

            return int.Parse(value.ToString());
        }

        public long GetLong(string fieldName)
        {
            var value = GetFieldValue(fieldName);
            return long.Parse(value.ToString());
        }

        public long? GetNullableLong(string fieldName)
        {
            var value = GetNullableFieldValue(fieldName);
            if (value == null) return null;

            return long.Parse(value.ToString());
        }

        public decimal GetDecimal(string fieldName)
        {
            var value = GetFieldValue(fieldName);
            return ((decimal)ValueTypeConverter.Convert(value.ToString(), "numeric"));
        }

        public decimal? GetNullableDecimal(string fieldName)
        {
            var value = GetNullableFieldValue(fieldName);
            if (value == null) return null;

            var strValue = value.ToString();
            if (string.IsNullOrEmpty(strValue)) return null;

            return ((decimal)ValueTypeConverter.Convert(strValue, "numeric"));
        }

        public bool CheckByteMask(string fieldName, int valueToCheck)
        {
            return (GetNullableInt(fieldName).GetValueOrDefault(0) & valueToCheck) > 0;
        }

        public decimal? Round(string fieldName, int precision)
        {
            var value = GetNullableDecimal(fieldName);

            if (value.HasValue)
                value = Math.Round(value.Value, precision, MidpointRounding.AwayFromZero);

            return value;
        }

        public bool GetBoolean(string fieldName)
        {
            var value = GetFieldValue(fieldName);
            return bool.Parse(value.ToString());
        }

        public bool? GetNullableBoolean(string fieldName)
        {
            var value = GetNullableFieldValue(fieldName);
            if (value == null) return null;

            return bool.Parse(value.ToString());
        }

		public string XPathEval(string fieldName, string xPath)
		{
			var value = GetFieldValue(fieldName, true);
			var xValue = XElement.Parse(string.Format("<root>{0}</root>", value));
            var result = xValue.XPathSelectElement(xPath);
            if (result != null)
                return result.Value;
            else
                return string.Empty;
		}

        public object XPathEvaluate(string fieldName, string xPath)
        {
            var value = GetFieldValue(fieldName, true);
            var xValue = XElement.Parse(string.Format("<root>{0}</root>", value));
            return xValue.XPathEvaluate(xPath, null);
        }


		public string XPathEvalPrevious(string fieldName, string xPath)
		{
            var value = GetPreviousFieldValue(fieldName, true);
			var xValue = XElement.Parse(string.Format("<root>{0}</root>", value));
            var result = xValue.XPathSelectElement(xPath);
            if (result != null)
                return result.Value;
            else
                return string.Empty;
        }

		public override string ToString()
		{
			return string.Format("EntityName: {0}; Fields Count: {1}; Children Count: {2}; IsDeleted: {3}; IsReadOnly: {4}", EntityName, DocumentFields.Count, ChildObjects.Count, IsDeleted, IsReadOnly);
		}

        /// <summary>
        /// Наличие этого класса предотвращает ошибки кроссдоменного доступа.
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            return (null);
        }

        ISchemaObject ISchemaObject.ParentObject 
        {
            get { return ParentObject; }
        }

        ISchemaObjectCollection ISchemaObject.ChildObjects
        {
            get { return new SchemaObjectCollection(ChildObjects.OfType<ISchemaObject>()); }
        }

        IDocumentFieldCollection ISchemaObject.DocumentFields
        {
            get { return new DocumentFieldCollection(DocumentFields.OfType<IDocumentField>()); }
        }

        /// <summary>
        /// Возвращает список полей в виде Dictionary.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToDictionary() 
        {
            Dictionary<string, object> items = new Dictionary<string, object>();

            foreach (var field in DocumentFields)
            {
                try
                {
                    items[field.Name] = GetFieldValue(field.Name);
                }
                catch 
                {
                }
            }

            return items;
        }

        private List<ShemaObject> GetDescendingObjects()
        {
            List<ShemaObject> descObjects = new List<ShemaObject>();

            foreach (var curObject in this.ChildObjects)
            {
                descObjects.Add(curObject);
                if (curObject.ChildObjects.Count > 0)
                    descObjects.AddRange(curObject.DescendingObjects);
            }

            return descObjects;
        }
        /// <summary>
        /// Возвращает строковое представление данного объекта, используя заданную формат строку. 
        /// Для вставки значений полей в представление добавьте в формат строку выражения 
        /// вида {FieldName} или {FieldName:Format}, например: {AuctionStartDate}, {AuctionStopDate:MM/dd/yyyy} или {StartPrice:0.00}.
        /// </summary>
        /// <param name="formatString"></param>
        /// <returns></returns>
        public string ToString(string formatString) 
        {
            var fieldValues = ToDictionary();

            return formatString.Inject(fieldValues);
        }

        public string GetState(string prefix, bool includeChild)
        {
            if (prefix == null)
            {
                prefix = string.Empty;
            }

            StringBuilder builder = new StringBuilder();

            builder.Append(prefix);
            builder.AppendFormat("{0} [object] (IsDeleted:{1}, IsReadOnly:{2}) \n", this.EntityName, this.IsDeleted, this.IsReadOnly);

            prefix += "-->  ";

            builder.Append(string.Join("\n", this.DocumentFields.Select(field => string.Format("{0}{1}: '{2}';", prefix, field.Code, field.Value))));

            builder.Append("\n");

            if (includeChild)
                foreach (var child in this.ChildObjects)
                {
                    builder.Append(child.GetState(prefix, true));
                }

            return builder.ToString();

        }
    }
}