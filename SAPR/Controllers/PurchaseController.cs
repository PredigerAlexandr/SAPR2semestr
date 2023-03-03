using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SAPR.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
        public IActionResult CreatePurchase( Field[] fields)
        {
            //if (id == null) return RedirectToAction("Index");
            //var zalupa = db.Fields.ToList();
            //var hui = db.Purchases.Where(p => p.PurchaseId == id).FirstOrDefault();
            //List<Field> fields = hui.Fields;
            //ViewBag.Fields = fields;
            return View();
        }
    }
}
