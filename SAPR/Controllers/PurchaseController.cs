using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
//using RuleCompiller;
using SAPR.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        DataBaseContext db;
        public PurchaseController(DataBaseContext context)
        {
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

            System.IO.File.WriteAllText(@"C:\Users\predi\OneDrive\Рабочий стол\бекап от 28.08.2023\_\6 семестр УлГТУ ИВТ\САПР\RuleCompillerGeneratedValidator.cs", purchase.GeneratedCode.Code, Encoding.Default);

            string errors;

            //var domain = AppDomain.CreateDomain(nameof(RuleCompiller.GeneratedValidator));
            //try
            //{
            //    var loader = (Loader)domain.CreateInstanceAndUnwrap(typeof(RuleCompiller.GeneratedValidator).Assembly.FullName, typeof(Loader).FullName);
            //    loader.Load(myFile);
            //}
            //finally
            //{
            //    AppDomain.Unload(domain);
            //}

            Assembly asm = Assembly.LoadFrom(@"C:\Users\predi\OneDrive\Рабочий стол\бекап от 28.08.2023\_\6 семестр УлГТУ ИВТ\САПР\SAPR\bin\Debug\net5.0\RuleCompiller.dll");
            Type? t = asm.GetType("RuleCompiller.GeneratedValidator");
            MethodInfo? square = t.GetMethod("BeforeCheck");
            object? result = square?.Invoke(null, new object[] { });
            errors = result.ToString();


            //string errors = GeneratedValidator.BeforeCheck();

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

            System.IO.File.WriteAllText(@"C:\Users\predi\OneDrive\Рабочий стол\бекап от 28.08.2023\_\6 семестр УлГТУ ИВТ\САПР\RuleCompillerGeneratedValidator.cs", purchase.GeneratedCode.Code, Encoding.Default);

            Assembly asm = Assembly.LoadFrom(@"C:\Users\predi\OneDrive\Рабочий стол\бекап от 28.08.2023\_\6 семестр УлГТУ ИВТ\САПР\RuleCompiller\bin\Debug\net5.0\RuleCompiller.dll");
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
