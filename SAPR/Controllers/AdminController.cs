using Microsoft.AspNetCore.Mvc;
using SAPR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAPR.Controllers
{
    public class AdminController : Controller
    {

        DataBaseContext db;
        public AdminController(DataBaseContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult AdminRules()
        {
            ViewBag.Purchases = db.Purchases;
            return View();
        }
    }
}
