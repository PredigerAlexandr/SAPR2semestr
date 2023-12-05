using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
//using RuleCompiller;
using SAPR.Models;
using SAPR.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SAPR.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly ILogger<PurchaseController> _logger;
        private readonly AppOptions _appOptions;

        DataBaseContext db;
        public PurchaseController(DataBaseContext context, IOptions<AppOptions> appOptions)
        {
            _appOptions = appOptions.Value;
            db = context;
            if (db.Purchases.ToList().Count == 0)
            {
                Purchase purchase1 = new Purchase()
                {
                    Name = "Заявка на получние нового ИНН",
                    Fields = new List<Field>
                    {
                        new Field
                        {
                            Name = "Name",
                            Alias = "Имя",
                            DefaultValue = ""
                        },
                        new Field
                        {
                            Name = "Age",
                            Alias = "Возраст",
                            DefaultValue = ""
                        }
                    },
                    BeforeRule = new Rule()
                    {
                        PurchaseId = 1,
                        Stage = "before",
                        RuleText = ""
                    },
                    AfterRule = new Rule()
                    {
                        PurchaseId = 1,
                        Stage = "after",
                        RuleText = "<rules>" +
                                        "<rule type=\"validation\">" +
                                            "field name=\"Name\" required=\"true\" isRequiredErrorMessage=\"Поле Имя должно быть заполненным\"/>" +
                                        "</rule>" +
                                    "</rules>"
                    },
                    GeneratedCode = new ExecutableСode
                    {
                        Code=""
                    }
                };
                db.Purchases.Add(purchase1);
                db.SaveChanges();
            }
        }

        public IActionResult Index()
        {
            var huipenis = db.Fields.ToList();
            var kek = db.Purchases.FirstOrDefault();
            return View(db.Purchases.ToList());
        }

        [HttpGet]
        [Route("Purchase/CreatePurchase/{id?}")]
        public IActionResult CreatePurchase(int? id)
        {
            var codes = db.ExecutableСodes.ToList();
            var fields = db.Fields.ToList();
            Purchase purchase = db.Purchases.Where(p => p.PurchaseId == id).FirstOrDefault();

            if (id == null) return RedirectToAction("Index");
            ViewBag.Purchase = purchase;

            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}\{_appOptions.Directories.GeneratedCode}"))
            {
                Directory.CreateDirectory(@$"{AppDomain.CurrentDomain.BaseDirectory}\{_appOptions.Directories.GeneratedCode}");
            }

            var pathGenerateCode = @$"{AppDomain.CurrentDomain.BaseDirectory}\{_appOptions.Directories.GeneratedCode}\RuleCompillerGeneratedValidator.cs";
            System.IO.File.WriteAllText(pathGenerateCode, purchase.GeneratedCode.Code, Encoding.Default);

            string errors;

            var pathToDll = @$"{AppDomain.CurrentDomain.BaseDirectory}\RuleCompiller.dll";
            Assembly asm = Assembly.LoadFrom(pathToDll);
            Type? t = asm.GetType("RuleCompiller.GeneratedValidator");
            MethodInfo? square = t.GetMethod("BeforeCheck");
            object? result = square?.Invoke(null, new object[] { });
            errors = result.ToString();

            if (string.IsNullOrEmpty(errors))
            {
                return View();
            }
            else
            {
                List<string> errorsList = new List<string>();
                var errorrsXml = XElement.Parse(errors);
                foreach (var error in errorrsXml.XPathSelectElements("Error"))
                {
                    var reader = error.CreateReader();
                    reader.MoveToContent();
                    string errorText = reader.ReadInnerXml();
                    errorsList.Add(errorText);
                }

                if (errorsList.Count > 0)
                {
                    ViewBag.ErrorsList = errorsList;
                    ViewBag.Purchase = purchase;
                    return View("ErrorPurchase");
                }
                else
                {
                    return View();
                }
            }
        }

        [HttpPost]
        [Route("Purchase/CreatePurchase/{id?}")]
        public IActionResult CreatePurchase(Field[] fields, int purchaseId)
        {
            XDocument fieldsXml = new XDocument();
            XElement fieldsInfo = new XElement("root");
            foreach (Field field in fields)
            {
                fieldsInfo.Add(new XElement(field.Alias, string.IsNullOrEmpty(field.Value) ? " " : field.Value));
            }
            fieldsXml.Add(fieldsInfo);

            var codes = db.ExecutableСodes.ToList();
            var fieldsForView = db.Fields.ToList();
            Purchase purchase = db.Purchases.Where(p => p.PurchaseId == purchaseId).FirstOrDefault();

            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}\{_appOptions.Directories.GeneratedCode}"))
            {
                Directory.CreateDirectory(@$"{AppDomain.CurrentDomain.BaseDirectory}\{_appOptions.Directories.GeneratedCode}");
            }

            var pathGenerateCode = @$"{AppDomain.CurrentDomain.BaseDirectory}\{_appOptions.Directories.GeneratedCode}\RuleCompillerGeneratedValidator.cs";
            System.IO.File.WriteAllText(pathGenerateCode, purchase.GeneratedCode.Code, Encoding.Default);

            var pathToDll = @$"{AppDomain.CurrentDomain.BaseDirectory}\RuleCompiller.dll";
            Assembly asm = Assembly.LoadFrom(pathToDll);
            Type? t = asm.GetType("RuleCompiller.GeneratedValidator");
            MethodInfo? square = t.GetMethod("AfterCheck");
            object? result = square?.Invoke(null, new object[] { fieldsXml });
            string errors = result.ToString();

            //string errors = GeneratedValidator.AfterCheck(fieldsXml.ToString());

            if (string.IsNullOrEmpty(errors))
            {
                return View("AcceptPurchase");
            }
            else
            {
                List<string> errorsList = new List<string>();
                var errorrsXml = XElement.Parse(errors);
                foreach (var error in errorrsXml.XPathSelectElements("Error"))
                {
                    var reader = error.CreateReader();
                    reader.MoveToContent();
                    string errorText = reader.ReadInnerXml();
                    errorsList.Add(errorText);
                }

                if (errorsList.Count > 0)
                {
                    ViewBag.ErrorsList = errorsList;
                    ViewBag.Purchase = purchase;
                    return View("ErrorPurchase");
                }
                else
                {
                    return View("AcceptPurchase");
                }

            }
            return View();
        }




    }
}
