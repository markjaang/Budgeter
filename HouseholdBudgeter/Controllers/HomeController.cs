using HouseholdBudgeter.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HouseholdBudgeter.Controllers
{
    [RequireHttps]
    
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Landing()
        {
            return View();
        }

        public ActionResult Index()
        {
                var user = db.Users.Find(User.Identity.GetUserId());
                if (user.HouseholdId != 0)
                {
                    var household = db.Household.Find(user.HouseholdId);
                    return View(household);
                }
             

            return View();
        }


        public ActionResult GetChart()
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                var transactions = user.Household.FinancialAccount.SelectMany(a => a.Transaction).ToList();
                var budgetexpense = user.Household.Budget.Where(b => b.Expense == true).Sum(b => b.Amount);
                var expenseTotal = transactions.Where(item => item.Active == true && item.Category.Expense == true).Sum(item => item.Amount);                

                var data = new[] { new {label = "Household Budget", value = budgetexpense },
                new { label = "Household Expense", value = expenseTotal } };

                return Content(JsonConvert.SerializeObject(data), "application/json");
            }

            return View("CreateJoinHousehold", "Households");
        }


        public ActionResult GetBar()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var household = db.Household.Find(user.HouseholdId);
            var accountCount = household.FinancialAccount.Where(u => u.Active == true).Count();
            if (accountCount > 0)
            {
                object[] data = new object[accountCount];
                var eList = new List<Transactions>();
                var iList = new List<Transactions>();
                int counter = 0;
                foreach (var n in household.FinancialAccount.Where(u => u.Active == true))
                {
                    if (n.Transaction.Count > 0)
                    {
                        var eTransactions = n.Transaction.Where(u => u.Category.Expense == true && u.Active == true).ToList();
                        var iTransactions = n.Transaction.Where(u => u.Category.Expense == false && u.Active == true).ToList();
                        data[counter] = new { name = n.Name, income = iTransactions.Sum(u => u.Amount), expense = eTransactions.Sum(u => u.Amount) };
                        counter++;
                    }
                    else
                    {
                        data[counter] = new { name = n.Name, income = 0, expense = 0 };
                        counter++;
                    }
                }
                return Content(JsonConvert.SerializeObject(data), "application/json");
            }
            else
            {
                ViewBag.AccCrt = "Please create accounts";
            }
            return View();
        }
    }
}