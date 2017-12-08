using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HouseholdBudgeter.Models;
using Microsoft.AspNet.Identity;

namespace HouseholdBudgeter.Controllers
{
    public class FinancialAccountsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: FinancialAccounts
        [Authorize]
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var userhouseholdid = user.HouseholdId;
            var financialAccount = db.FinancialAccount.Where(f => f.Household.Id == userhouseholdid && f.Active == true);

            return View(financialAccount.ToList());

        }

        // GET: FinancialAccounts/Details/5
        public PartialViewResult _AcctDetails(int? id)
        {
            if (id == null)
            {
                return PartialView(HttpStatusCode.BadRequest);
            }
            var user = db.Users.Find(User.Identity.GetUserId());
            FinancialAccounts financialAccounts = db.FinancialAccount.Find(id);
            var userhousehold = db.Users.Find(User.Identity.GetUserId()).Household;
            
            var incomeItems = financialAccounts.Transaction.Where(t => t.Category.Expense == false && t.Active == true);
            var expenseItems = financialAccounts.Transaction.Where(t => t.Category.Expense == true && t.Active == true);
            var totalRecexp = expenseItems.Where(t => t.Reconciled == true && t.Category.Expense == true).Sum(t => t.Amount);
            var totalRecInc = incomeItems.Where(t => t.Reconciled == true && t.Category.Expense == false).Sum(t => t.Amount);
            var budgetexptotal = userhousehold.Budget.Where(b => b.HouseholdId == userhousehold.Id).Where(b => b.Category.Expense == true).Sum(b => b.Amount);
            var budgetinctotal = userhousehold.Budget.Where(b => b.HouseholdId == userhousehold.Id).Where(b => b.Category.Expense == false).Sum(b => b.Amount);
            var transactions = userhousehold.FinancialAccount.SelectMany(item => item.Transaction).ToList();
            var expense = transactions.Where(t => t.Category.Expense == true && t.Active == true);
            var exptotal = expense.Where(item => item.Reconciled == true).Sum(item => item.Amount);
            ViewBag.totalAmt = incomeItems.Sum(t => t.Amount) - expenseItems.Sum(t => t.Amount);
            ViewBag.recAmount = totalRecInc - totalRecexp;
            ViewBag.budgetTotal = budgetexptotal - budgetinctotal;
            ViewBag.remBud = budgetexptotal - budgetinctotal - exptotal;
            ViewBag.incTotal = incomeItems.Sum(t => t.Amount);
            ViewBag.expTotal = expenseItems.Sum(t => t.Amount);

            if (financialAccounts == null)
            {
                return PartialView(userhousehold);
            }

            return PartialView(financialAccounts);
        }

        // GET: FinancialAccounts/Create
        public ActionResult Create()
        {
            ViewBag.HouseholdId = new SelectList(db.Household, "Id", "Name");
            return View();
        }

        // POST: FinancialAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Balance,HouseholdId")] FinancialAccounts financialAccounts)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                var household = user.Household;

                if(household == null)
                {
                    TempData["householdId"] = "Please first create or join a household";

                    return RedirectToAction("CreateJoinHousehold", "Households");
                }
                financialAccounts.Created = System.DateTimeOffset.Now;
                financialAccounts.HouseholdId = household.Id;
                financialAccounts.Active = true;
                db.FinancialAccount.Add(financialAccounts);
                db.SaveChanges();

                if(financialAccounts.Balance != 0)
                {
                    Transactions transaction = new Transactions();
                    transaction.Active = true;
                    transaction.Amount = financialAccounts.Balance;
                    transaction.CategoryId = 4;
                    transaction.Created = System.DateTimeOffset.Now;
                    transaction.Description = "Initial Balance";
                    transaction.FinancialAccountId = financialAccounts.Id;
                    transaction.LastUpdatedUserId = user.Id;
                    transaction.Reconciled = true;
                    transaction.TransactionDate = System.DateTimeOffset.Now;
                    transaction.TransactionUserId = user.Id;
                    db.Transaction.Add(transaction);
                    db.SaveChanges();
                }

                var householdaccount = user.Household.FinancialAccount.Where(h => h.Id == financialAccounts.Id);
                var attachaccount = db.FinancialAccount.Where(a => a.Id == financialAccounts.Id);
                householdaccount = attachaccount;
                db.SaveChanges();

                return RedirectToAction("Index", "Households", new { id = financialAccounts.Id});
            }

            ViewBag.HouseholdId = new SelectList(db.Household, "Id", "Name", financialAccounts.HouseholdId);
            return View(financialAccounts);
        }

        // GET: FinancialAccounts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FinancialAccounts financialAccounts = db.FinancialAccount.Find(id);
            if (financialAccounts == null)
            {
                return HttpNotFound();
            }
            ViewBag.HouseholdId = new SelectList(db.Household, "Id", "Name", financialAccounts.HouseholdId);
            return View(financialAccounts);
        }

        // POST: FinancialAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Balance,HouseholdId,Created,Updated,LastReconciledDate")] FinancialAccounts financialAccounts)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                var household = user.Household;
                financialAccounts.HouseholdId = household.Id;
                financialAccounts.Updated = System.DateTimeOffset.Now;
                financialAccounts.Active = true;
                db.Entry(financialAccounts).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Households", new { id = financialAccounts.Id });
            }
            ViewBag.HouseholdId = new SelectList(db.Household, "Id", "Name", financialAccounts.HouseholdId);
            return RedirectToAction("Index", "Households", new { id = financialAccounts.Id });
        }

        // GET: FinancialAccounts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FinancialAccounts financialAccounts = db.FinancialAccount.Find(id);
            if (financialAccounts == null)
            {
                return HttpNotFound();
            }
            return View(financialAccounts);
        }

        // POST: FinancialAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FinancialAccounts financialAccounts = db.FinancialAccount.Find(id);
            financialAccounts.Active = false;
            db.Entry(financialAccounts).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", "Households");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
