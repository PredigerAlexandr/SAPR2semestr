using RuleCompiller.Validators;
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
using RuleCompiller.Conditions;
using RuleCompiller.Rules;
using Microsoft.Data.SqlClient;

namespace RuleCompiller
{
    public class RuleCompiller
    {
        public static string connectionString = "Server=(localdb)\\mssqllocaldb;Database=mobilestoredb;Trusted_Connection=True;MultipleActiveResultSets=true;";
        public static string GetRules(int purchaseId, ValidationType validationType = ValidationType.after)
        {
            string rules = "";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (
                    SqlCommand command = new SqlCommand(
                        "Select * from Rules " +
                        "Where PurchaseId = @purchaseId AND Stage = @stage",
                        connection
                    )
                )
                {
                    command.CommandType = System.Data.CommandType.Text;
                    command.Parameters.AddWithValue("@purchaseId", purchaseId);
                    if (validationType == ValidationType.before)
                    {
                        command.Parameters.AddWithValue("@stage", "before");
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@stage", "after");
                    }

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        if (reader["RuleText"] is string) rules = (string)reader["RuleText"];
                    }
                }
            }
            return rules;
        }

        public static List<DocumentSchema> GetSimpleDocumentShema(int purchaseId)
        {
            List<DocumentSchema> listFields = new List<DocumentSchema>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (
                    SqlCommand command = new SqlCommand(
                        "Select * from Fields " +
                        "Where PurchaseId = @purchaseId",
                        connection
                    )
                )
                {
                    command.CommandType = System.Data.CommandType.Text;
                    command.Parameters.AddWithValue("@purchaseId", purchaseId);

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        listFields.Add(new DocumentSchema
                        {
                            Alias = (string)reader["Alias"],
                            Name = (string)reader["Name"],
                            DefaultValue = (string)(reader["DefaultValue"] is not string ? "" : reader["DefaultValue"]),
                        });
                    }
                }

                return listFields;
            }
        }

        public static RuleComposite BuildRulesTree(string ruleExpression, List<DocumentSchema> fields, CodeTreeHandler codeTreeHandler, ICustomRuleFactory ruleFactory = null)
        {
            //LogHelper.Logger.DebugFormat("RuleComposite ParseRulesFromXml {0}", ruleFactory == null ? "Null" : ruleFactory.GetType().ToString());
            var ruleXml = XElement.Parse(ruleExpression);
            if (ruleXml == null) return null;

            var result = new RuleComposite(codeTreeHandler);
            foreach (var xRule in ruleXml.XPathSelectElements("rule"))
            {
                var ruleType = xRule.Attribute("type");

                if (ruleType == null)
                    throw new ApplicationException("Не указан тип правила.");


                switch (ruleType.Value)
                {
                    case "condition": //if else then
                        {
                            var rule = ConditionRule.Parse(xRule, fields, codeTreeHandler);
                            result.Add(rule);
                            break;
                        }
                    case "business": //sql и лямбда выражения
                        {
                            var rule = BusinessRule.Parse(xRule, codeTreeHandler);
                            result.Add(rule);
                            var tmp = rule.Match(new ValidationData(ValidationType.before, new string[] { "Amount" }, 11, 9549));
                            break;
                        }
                    case "validation":
                        {
                            var rule = ValidationRule.Parse(xRule, fields, codeTreeHandler);
                            result.Add(rule);
                            break;
                        }
                    case "select": //case, default(else), isnull rules
                        {
                            var rule = SelectCaseRule.Parse(xRule, fields, codeTreeHandler);
                            result.Add(rule);
                            break;
                        }
                    default:
                        throw new Exception("Задан неизвестный тип правила.");
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
                        //listFields.Add(new DocumentSchema
                        //{
                        //    Code = (string)reader["Code"],
                        //    DataTypeId = (string)reader["DataTypeId"],
                        //    DefaultValue = (string)(reader["DefaultValue"] is not string ? "" : reader["DefaultValue"]),
                        //    Description = (string)reader["Description"],
                        //    Object = (string)reader["Object"],
                        //    IsRequired = (bool)reader["isRequired"],
                        //    IsUnique = (bool)reader["isUnique"],
                        //    IsNotRemove = (bool)reader["isNotRemove"],
                        //    IsReadOnly = (bool)reader["ReadOnly"],
                        //    IsBaseObject = (bool)reader["IsBaseObject"],
                        //    IsServiceObject = (bool)reader["IsServiceObject"],
                        //    IsCrossObject = (bool)reader["IsCrossObject"],
                        //    DeleteIfEmpty = (bool)(reader["DeleteIfEmpty"] is not bool ? false : reader["DeleteIfEmpty"]),
                        //    Name = (string)reader["Name"],
                        //    IsTemporary = (bool)(reader["IsTemporary"] is not bool ? false : reader["IsTemporary"]),
                        //    UniqueKey = (string)(reader["UniqueKey"] is not string ? "" : reader["UniqueKey"]),
                        //    DoNotSaveToDocumentObject = (bool)(reader["DoNotSaveToDocumentObject"] is not bool ? false : reader["DoNotSaveToDocumentObject"]),
                        //    ValuePath = (string)(reader["ValuePath"] is not string ? "" : reader["ValuePath"]),
                        //    DoNotLowerCase = (bool)(reader["DoNotLowerCase"] is not bool ? false : reader["DoNotLowerCase"])
                        //}
                        //);
                    }
                }
            }

            return new List<DocumentSchema>();
        }
    }
}
