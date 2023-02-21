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
            return View(db.Purchases.ToList());
        }

        [HttpGet]
        public IActionResult CreatePurchase(int? id)
        {
            if (id == null) return RedirectToAction("Index");
            var zalupa = db.Fields;
            var hui = db.Purchases.Where(p => p.PurchaseId == id).FirstOrDefault();
            List<Field> fields = hui.Fields;
            ViewBag.Fields = fields;
            return View();
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return ;
        //}
    }
}
