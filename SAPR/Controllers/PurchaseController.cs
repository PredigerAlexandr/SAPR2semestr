using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SAPR.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SAPR.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly ILogger<PurchaseController> _logger;

        DataBaseContext db;
        public PurchaseController(DataBaseContext context)
        {
            db = context;
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
            if (id == null) return RedirectToAction("Index");
            var zalupa = db.Fields.ToList();
            var hui = db.Purchases.Where(p => p.PurchaseId == id).FirstOrDefault();
            ViewBag.Purchase = hui;
            return View();
        }

        [HttpPost]
        [Route("Purchase/CreatePurchase/{id?}")]
        public IActionResult CreatePurchase(Field[] fields)
        {
            XDocument fieldsXml = new XDocument();
            XElement fieldsInfo = new XElement("root");
            foreach (Field field in fields)
            {
                fieldsInfo.Add(new XElement(field.Alias, field.DefaultValue));
            }
            fieldsXml.Add(fieldsInfo);

            return View();
        }

        [IgnoreAntiforgeryToken]
        public class CreatePurchaseModel : PageModel
        {
            public Field[] Fields { get; private set; } = Array.Empty<Field>();

            public void OnPost(Field[] fields)
            {
                Fields = fields;
            }
        }
    }
}
