using RuleCompiller.Validators;
using RuleCompiller.Helpers;
using RuleCompiller.Models;
using RuleCompiller.Validators;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using RuleCompiller.Conditions;
using RuleCompiller.Rules;

namespace RuleCompiller
{
    public class RuleCompiller
    {
        public static string connectionString = "Data Source=TE-DB;Initial Catalog=UTP_TEST;User ID=utpProtoUser;Password=w1v4MdYK8z;";
        public static string GetRules(int tradeSectionId, long modelId, string targetObjectName, ValidationType validationType = ValidationType.after)
        {
            string rules = "";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (
                    SqlCommand command = new SqlCommand(
                        "DECLARE @modelHistoryid INT = (Select ActiveModelHistoryId from Model WHERE TradeSectionId = @tradeSectionId  AND Id = @modelId);" +
                        "EXEC[dbo].[ModelVerificationGetRules] @TradeSectionId = @tradeSectionId, @ModelHistoryId = @modelHistoryid, @object = @targetObjectName, @CheckStage = @CheckStage",
                        connection
                    )
                )
                {
                    command.CommandType = System.Data.CommandType.Text;
                    command.Parameters.AddWithValue("@tradeSectionId", tradeSectionId);
                    command.Parameters.AddWithValue("@modelId", modelId);
                    command.Parameters.AddWithValue("@targetObjectName", targetObjectName);
                    if (validationType == ValidationType.before)
                    {
                        command.Parameters.AddWithValue("@CheckStage", "before");
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@CheckStage", "after");
                    }

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        if (reader["Expression"] is string) rules = (string)reader["Expression"];
                    }
                }
            }
            return rules;
        }

        public static List<DocumentSchema> GetSimpleDocumentShema(int tradeSectionId, long modelId)
        {
            List<DocumentSchema> listFields = new List<DocumentSchema>();

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
                        listFields.Add(new DocumentSchema
                        {
                            Code = (string)reader["Code"],
                            DataTypeId = (string)reader["DataTypeId"],
                            DefaultValue = (string)(reader["DefaultValue"] is not string ? "" : reader["DefaultValue"]),
                            Description = (string)reader["Description"],
                            Object = (string)reader["Object"],
                            IsRequired = (bool)reader["isRequired"],
                            IsUnique = (bool)reader["isUnique"],
                            IsNotRemove = (bool)reader["isNotRemove"],
                            IsReadOnly = (bool)reader["ReadOnly"],
                            IsBaseObject = (bool)reader["IsBaseObject"],
                            IsServiceObject = (bool)reader["IsServiceObject"],
                            IsCrossObject = (bool)reader["IsCrossObject"],
                            DeleteIfEmpty = (bool)(reader["DeleteIfEmpty"] is not bool ? false : reader["DeleteIfEmpty"]),
                            Name = (string)reader["Name"],
                            IsTemporary = (bool)(reader["IsTemporary"] is not bool ? false : reader["IsTemporary"]),
                            UniqueKey = (string)(reader["UniqueKey"] is not string ? "" : reader["UniqueKey"]),
                            DoNotSaveToDocumentObject = (bool)(reader["DoNotSaveToDocumentObject"] is not bool ? false : reader["DoNotSaveToDocumentObject"])
                        }
                        );
                    }
                }
            }

            return listFields;
        }

        public static RuleComposite BuildRulesTree(string ruleExpression, List<DocumentSchema> fields, CodeTreeHandler codeTreeHandler, ICustomRuleFactory ruleFactory = null)
        {
            //LogHelper.Logger.DebugFormat("RuleComposite ParseRulesFromXml {0}", ruleFactory == null ? "Null" : ruleFactory.GetType().ToString());
            var ruleXml = XElement.Parse(ruleExpression);
            if (ruleXml == null) return null;

            var result = new RuleComposite(codeTreeHandler);
            foreach (var xRule in ruleXml.XPathSelectElements("rule"))
            {
                int lineNumber = ((IXmlLineInfo)xRule).LineNumber;

                var ruleType = xRule.Attribute("type");
                var ruleName = xRule.Attribute("ruleName");
                var ruleSchemaType = xRule.Attribute("schemaType");

                if (ruleType == null)
                    throw new RuleParseException(lineNumber, "Не указан тип правила.");

                RuleSchemaType ruleScemaType = RuleSchemaType.All;
                //if (ruleSchemaType != null && _violationSchemaTypeMap.ContainsKey(ruleSchemaType.Value.ToLower()))
                //    ruleScemaType = _violationSchemaTypeMap[ruleSchemaType.Value.ToLower()];

                switch (ruleType.Value)
                {
                    case "condition": //if else then
                        {
                            var rule = ConditionRule.Parse(xRule, fields, codeTreeHandler);
                            rule.Name = ruleName == null ? string.Empty : ruleName.Value;
                            rule.SchemaType = ruleScemaType;
                            result.Add(rule);
                            break;
                        }
                    case "business": //sql и лямбда выражения
                        {
                            var rule = BusinessRule.Parse(xRule, codeTreeHandler);
                            rule.Name = ruleName == null ? string.Empty : ruleName.Value;
                            rule.SchemaType = ruleScemaType;
                            result.Add(rule);
                            var tmp = rule.Match(new ValidationData(ValidationType.before, new string[] { "Amount" }, 11, 9549));
                            break;
                        }
                    case "validation":
                        {
                            var rule = ValidationRule.Parse(xRule, fields, codeTreeHandler);
                            rule.Name = ruleName == null ? string.Empty : ruleName.Value;
                            rule.SchemaType = ruleScemaType;
                            result.Add(rule);
                            break;
                        }
                    case "select": //case, default(else), isnull rules
                        {
                            var rule = SelectCaseRule.Parse(xRule, fields, codeTreeHandler);
                            rule.Name = ruleName == null ? string.Empty : ruleName.Value;
                            rule.SchemaType = ruleScemaType;
                            result.Add(rule);
                            break;
                        }
                    case "comment":
                        //IssueId #95756
                        //2020-06-23 - Снегирь М.С.
                        //Введен новый тип правила - "Комментарий"
                        continue;
                    default:
                        throw new RuleParseException(lineNumber, "Задан неизвестный тип правила.");
                }

                if (ruleFactory == null) continue;
                result.Add(ruleFactory.Create(xRule));
            }
            return result;
        }

        public void DecompileRule()
        {

        }

        public void DecompileComposite(RuleComposite composite)
        {
            //foreach (var rule in new )
            //{
            //    if (rule is RuleComposite)
            //    {
            //        DecompileComposite(rule as RuleComposite);
            //    }
            //}
        }

        //для отработки пост-правил, после подгрузки схемы заполняет поля данными из пришедшего xml
        public static List<DocumentSchema> GetSimpleDocumentShema(int tradeSectionId, long modelId, string xmlText)
        {

            XElement root = XElement.Parse(xmlText);

            List<DocumentSchema> listFields = new List<DocumentSchema>();

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
                        listFields.Add(new DocumentSchema
                        {
                            Code = (string)reader["Code"],
                            DataTypeId = (string)reader["DataTypeId"],
                            DefaultValue = (string)(reader["DefaultValue"] is not string ? "" : reader["DefaultValue"]),
                            Description = (string)reader["Description"],
                            Object = (string)reader["Object"],
                            IsRequired = (bool)reader["isRequired"],
                            IsUnique = (bool)reader["isUnique"],
                            IsNotRemove = (bool)reader["isNotRemove"],
                            IsReadOnly = (bool)reader["ReadOnly"],
                            IsBaseObject = (bool)reader["IsBaseObject"],
                            IsServiceObject = (bool)reader["IsServiceObject"],
                            IsCrossObject = (bool)reader["IsCrossObject"],
                            DeleteIfEmpty = (bool)(reader["DeleteIfEmpty"] is not bool ? false : reader["DeleteIfEmpty"]),
                            Name = (string)reader["Name"],
                            IsTemporary = (bool)(reader["IsTemporary"] is not bool ? false : reader["IsTemporary"]),
                            UniqueKey = (string)(reader["UniqueKey"] is not string ? "" : reader["UniqueKey"]),
                            DoNotSaveToDocumentObject = (bool)(reader["DoNotSaveToDocumentObject"] is not bool ? false : reader["DoNotSaveToDocumentObject"]),
                            ValuePath = (string)(reader["ValuePath"] is not string ? "" : reader["ValuePath"]),
                            DoNotLowerCase = (bool)(reader["DoNotLowerCase"] is not bool ? false : reader["DoNotLowerCase"])
                        }
                        );
                    }
                }
            }

            return new List<DocumentSchema>();
        }
    }
}
