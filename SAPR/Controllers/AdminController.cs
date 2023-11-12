using Microsoft.AspNetCore.Mvc;
//using RuleCompiller;
using SAPR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            Assembly asm = Assembly.LoadFrom(@"C:\Users\predi\OneDrive\Рабочий стол\бекап от 28.08.2023\_\6 семестр УлГТУ ИВТ\САПР\SAPR\bin\Debug\net5.0\RuleCompiller.dll");
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
            return View(db.Purchases.ToList());
        }
    }
}
