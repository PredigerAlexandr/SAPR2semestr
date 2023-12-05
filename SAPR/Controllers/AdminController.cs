using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
//using RuleCompiller;
using SAPR.Models;
using SAPR.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SAPR.Controllers
{
    public class AdminController : Controller
    {
        private readonly DataBaseContext db;
        private readonly AppOptions _appOptions;
        public AdminController(DataBaseContext context, IOptions<AppOptions> appOptions)
        {
            db = context;
            _appOptions = appOptions.Value;
        }

        [Route("Admin/AdminRules/{id?}")]
        [HttpGet]
        public IActionResult AdminRules(int? id)
        {
            var rules = db.Rules.ToList();
            var purchase = db.Purchases.Where(p => p.PurchaseId == id).FirstOrDefault();
            return View(purchase);
        }

        [HttpPost]
        public IActionResult AdminRules(string beforeRuleText, string afterRuleText, int purchaseId)
        {
            var rules = db.Rules.ToList();
            var generatedCodes = db.ExecutableСodes.ToList();
            var purchase = db.Purchases.Where(p => p.PurchaseId == purchaseId).FirstOrDefault();

            if (purchase.BeforeRule != null)
            {
                purchase.BeforeRule.RuleText = beforeRuleText;
            }
            else
            {
                purchase.BeforeRule = new Rule
                {
                    PurchaseId = purchaseId,
                    Stage = "before",
                    RuleText = beforeRuleText
                };
            }

            if (purchase.AfterRule != null)
            { 
                purchase.AfterRule.RuleText = afterRuleText;
            }
            else
            {
                purchase.AfterRule = new Rule
                {
                    PurchaseId = purchaseId,
                    Stage = "after",
                    RuleText = afterRuleText
                };
            }

            db.SaveChanges();

            var pathToDll = @$"{AppDomain.CurrentDomain.BaseDirectory}\RuleCompiller.dll";
            Assembly asm = Assembly.LoadFrom(pathToDll);
            Type? t = asm.GetType("RuleCompiller.GeneratedCodeHandler");
            MethodInfo? square = t.GetMethod("GeneratedCode");
            object? result = square?.Invoke(null, new object[] { purchase.PurchaseId });

            string code = result.ToString();
            //string code = GeneratedCodeHandler.GeneratedCode(purchase.PurchaseId);

            if (purchase.GeneratedCode == null)
            {
                purchase.GeneratedCode = new ExecutableСode()
                {
                    Code = code
                };
            }
            else
            {
                purchase.GeneratedCode.Code = code;
            }

            db.SaveChanges();


            return Redirect("~/Admin/AdminIndex");
        }

        [HttpGet]
        public IActionResult AdminIndex()
        {
            var tmp = db.Fields.ToList();
            ViewData["mesForCert"] = System.IO.File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}\{_appOptions.Directories.Certificate}\server.pfx") ? "Существует" : "Отсутствует";
            return View(db.Purchases.ToList());
        }
    }
}
