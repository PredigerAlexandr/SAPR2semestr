using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RuleCompiller
{
    public class ValidationData
    {
        protected XElement rootElement;
        private const string connectionString = "Data Source=TE-DB;Initial Catalog=UTP_TEST;User ID=utpProtoUser;Password=w1v4MdYK8z;";
        internal Dictionary<string, FieldInfo> fieldsInfo = new Dictionary<string, FieldInfo>();


        public ValidationData(ValidationType type, string[] checkFields, long tradeSectionId, long modelId, string rootXml = null)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("GetSimpleDocumentObjects", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@tradeSectionId", tradeSectionId);
                    command.Parameters.AddWithValue("@modelId", modelId);



                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string currentFieldName = (string)reader["Name"];
                        if (checkFields.Contains(currentFieldName))
                        {
                            var name = currentFieldName;
                            var path = (string)(reader["ValuePath"] is not string ? "" : reader["ValuePath"]);
                            var isComputed = (bool)(reader["isComputed"] is not bool ? false : reader["isComputed"]);
                            var value = (string)((reader["DefaultValue"] is not string) ? "" : reader["DefaultValue"]);
                            var dataType = (string)reader["DataTypeId"];

                            fieldsInfo.Add(
                            currentFieldName,
                            new FieldInfo()
                            {
                                name = currentFieldName,
                                path = path,
                                isComputed = isComputed,
                                value = value,
                                DataType = dataType
                            }
                        );
                        }
                    }
                }
            }
            //TODO: получение с бд всех полей т.е заменяем fieldsInfo вместо него будет массив необхидимых строк
            this.fieldsInfo = fieldsInfo;
            switch (type)
            {
                case ValidationType.before:
                    GetValue = GetBeforeValue;
                    break;
                case ValidationType.after:
                    GetValue = GetAfterValue;
                    try
                    {
                        rootElement = XElement.Parse(rootXml);
                    }
                    catch (Exception error)
                    {
                        throw new ApplicationException("Некорректный XML. " + error.Message);
                    }
                    break;
                default:
                    throw new ApplicationException("Unknown validation type");
            }
        }

        protected string GetBeforeValue(string name)
        {
            fieldsInfo.TryGetValue(name, out FieldInfo currentFieldInfo);
            if (currentFieldInfo == null) throw new ApplicationException($"Не удалось получить значение свойства '{name}'");
            return currentFieldInfo.value;
        }

        protected string GetAfterValue(string name)
        {
            fieldsInfo.TryGetValue(name, out FieldInfo currentFieldInfo);
            if (currentFieldInfo == null) throw new ApplicationException($"Не удалось получить значение свойства '{name}'"); //TODO: запилить спец исключения?

            if (currentFieldInfo.isComputed)
            {
                var node = rootElement.XPathEvaluate(currentFieldInfo.path) == null ? rootElement.XPathEvaluate(currentFieldInfo.path.ToLower()) : rootElement.XPathEvaluate(currentFieldInfo.path);
                if (node != null) return node.ToString();
            }
            else
            {
                XElement value = rootElement.XPathSelectElement(currentFieldInfo.path) == null ? rootElement.XPathSelectElement(currentFieldInfo.path.ToLower()) : rootElement.XPathSelectElement(currentFieldInfo.path);
                if (value != null)
                {
                    using (var reader = value.CreateReader())
                    {
                        reader.MoveToContent();
                        return reader.ReadInnerXml();
                    }
                }
            }

            return null;
        }

        protected Func<string, string> GetValue;

        public string this[string key]
        {
            get
            {
                return GetValue(key);
            }
        }

        public string GetFieldType(string fieldName)
        {
            return fieldsInfo[fieldName].DataType.ToLower();
        }
    }

    public class FieldInfo
    {
        public string name;
        public string path;
        public bool isComputed;
        public string value;
        private string dataType;

        public string DataType
        {
            get { return value.ToLower(); }
            set { dataType = value; }
        }
    }

    public enum ValidationType
    {
        before,
        after
    }
}
