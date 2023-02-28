//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;
//using System.Xml.XPath;

//namespace RuleCompiller
//{
//    /*
//     * Базовый валидатор
//     * Методы BeforeValidator и AfterValidator возвращают пустую строку в случае упеха либо строку с текстом ошибки в случае возникновения ошибки в процессе валидации.
//     * Не переопределенные методы по умолчанию возвращают пустую строку (успех)
//    */
//    public abstract class BaseValidator
//    {
//        public static string connectionString = "Data Source=TE-DB;Initial Catalog=UTP_TEST;User ID=utpProtoUser;Password=w1v4MdYK8z;";
//        protected Dictionary<string, FieldInfo> BeforeFieldsInfo = new Dictionary<string, FieldInfo>();
//        protected Dictionary<string, FieldInfo> AfterFieldsInfo = new Dictionary<string, FieldInfo>();
//        protected long tradeSectionId;
//        protected long modelId;
//        protected XElement rootElement;

//        public BaseValidator(string[] beforeValidateFields, string[] afterValidateFields, long tradeSectionId, long modelId)
//        {
//            //Получение изменяемых данных полей
//            this.tradeSectionId = tradeSectionId;
//            this.modelId = modelId;
//            using (SqlConnection connection = new SqlConnection(connectionString))
//            {
//                connection.Open();
//                using (SqlCommand command = new SqlCommand("GetSimpleDocumentObjects", connection))
//                {
//                    command.CommandType = System.Data.CommandType.StoredProcedure;
//                    command.Parameters.AddWithValue("@tradeSectionId", tradeSectionId);
//                    command.Parameters.AddWithValue("@modelId", modelId);

//                    var reader = command.ExecuteReader();

//                    while (reader.Read())
//                    {
//                        string currentFieldName = (string)reader["Name"];
//                        if (beforeValidateFields.Contains(currentFieldName))
//                        {
//                            var name = currentFieldName;
//                            var path = (string)(reader["ValuePath"] is not string ? "" : reader["ValuePath"]);
//                            var isComputed = (bool)(reader["isComputed"] is not bool ? false : reader["isComputed"]);
//                            var value = (string)((reader["DefaultValue"] is not string) ? "" : reader["DefaultValue"]);


//                            BeforeFieldsInfo.Add(
//                                currentFieldName,
//                                new FieldInfo()
//                                {
//                                    name = currentFieldName,
//                                    path = path,
//                                    isComputed = isComputed,
//                                    value = value,
//                                }
//                            );
//                        }
//                        //TODO: значения должны браться из xml а не из бд
//                        if (afterValidateFields.Contains(currentFieldName))
//                        {
//                            var name = currentFieldName;
//                            var path = (string)(reader["ValuePath"] is not string ? "" : reader["ValuePath"]);
//                            var isComputed = (bool)(reader["isComputed"] is not bool ? false : reader["isComputed"]);
//                            var value = (string)((reader["DefaultValue"] is not string) ? "" : reader["DefaultValue"]);

//                            AfterFieldsInfo.Add(
//                                currentFieldName,
//                                new FieldInfo()
//                                {
//                                    name = currentFieldName,
//                                    path = path,
//                                    isComputed = isComputed,
//                                    value = value,
//                                }
//                            );
//                        }
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// Метод предпроверки
//        /// </summary>
//        /// <returns>Список нарушений</returns>
//        virtual public List<string> BeforeValidation()
//        {
//            var validationData = new ValidationData(ValidationType.before, BeforeFieldsInfo);
//            return new List<string>();
//        }

//        /// <summary>
//        /// Метод постпроверки
//        /// </summary>
//        /// <returns>Список нарушений</returns>
//        virtual public List<string> AfterValidation(string validationXmlData)
//        {
//            var validationData = new ValidationData(ValidationType.after, AfterFieldsInfo, validationXmlData);
//            return new List<string>();
//        }
//    }

//    public class FieldInfo
//    {
//        public string name;
//        public string path;
//        public bool isComputed;
//        public string value;
//    }

//    /// Перечисление типов данных, используемых при проверке значений свойств объектов.
//    public enum ValidationDataType
//    {
//        String,
//        Integer,
//        Double,
//        Decimal,
//        Date,
//        Bit
//    }

//    public enum ValidationOperator
//    {
//        Equal,
//        NotEqual,
//        GreaterThan,
//        GreaterThanEqual,
//        LessThan,
//        LessThanEqual
//    }

//    public static class ValidationOperatorExtern
//    {
//        public static string GetName(this ValidationOperator value)
//        {
//            switch (value)
//            {
//                case ValidationOperator.Equal: return "равно";
//                case ValidationOperator.NotEqual: return "не равно";
//                case ValidationOperator.LessThanEqual: return "меньше или равно";
//                case ValidationOperator.GreaterThanEqual: return "больше или равно";
//                case ValidationOperator.LessThan: return "меньше, чем";
//                case ValidationOperator.GreaterThan: return "больше, чем";
//            }
//            return "";
//        }
//    }
//}
