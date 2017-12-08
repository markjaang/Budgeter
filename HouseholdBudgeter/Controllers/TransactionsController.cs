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
using HouseholdBudgeter.Models.Helpers;

namespace HouseholdBudgeter.Controllers
{
    public class TransactionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transactions
        public PartialViewResult _TransIndex()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var householdid = user.HouseholdId;
            if(householdid == null)
            {
                TempData["householdId"] = "Please first create or join a household";

                return PartialView("CreateJoinHousehold", "Households");
            }
            var transaction = db.Transaction.Include(t => t.Category).Include(t => t.FinancialAccount).Include(t => t.LastUpdatedUser).Include(t => t.TransactionUser).Include(t => t.Reconciled);
            return PartialView(transaction.ToList());
        }

        // GET: Reconciliation
        public ActionResult Reconcile(int? id)
        {
            Transactions transaction = db.Transaction.Find(id);
            return View(transaction);
        }

        // POST: Reconciliation
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reconcile([Bind(Include = "Id,Amount,CategoryId")] Transactions transaction)
        {
            if (ModelState.IsValid)
            {
                Transactions oldTr = db.Transaction.FirstOrDefault(t => t.Id == transaction.Id);
                FinancialAccounts account = db.FinancialAccount.Find(oldTr.FinancialAccountId);
                var userid = User.Identity.GetUserId();
                if(oldTr.Reconciled == false)
                {
                    transaction.TransactionUserId = userid;
                    transaction.Reconciled = true;
                    oldTr.Reconciled = true;
                    transaction.Created = System.DateTimeOffset.Now;
                    transaction.TransactionDate = System.DateTimeOffset.Now;
                    transaction.Active = true;
                    transaction.FinancialAccountId = oldTr.FinancialAccountId;
                    transaction.Description = oldTr.Description + "(Reconciled Difference)";
                    decimal difference;
                    difference = transaction.Amount - oldTr.Amount;
                    transaction.Amount = difference;
                    if (oldTr.Category.Expense == true && difference >= 0)
                    {
                        transaction.CategoryId = 27;
                    }
                    else if (oldTr.Category.Expense == true && difference < 0)
                    {
                        transaction.CategoryId = 4;
                        difference = -difference;
                    }
                    else if(oldTr.Category.Expense == false && difference >= 0)
                    {
                        transaction.CategoryId = 4;
                    }
                    else
                    {
                        transaction.CategoryId = 27;
                        difference = -difference;
                    }

                    db.SaveChanges();

                    var cat = db.Category.Find(transaction.CategoryId);

                    if (cat.Expense == true)
                    {
                        account.Balance -= transaction.Amount; 
                    }
                    else
                    {
                        account.Balance += transaction.Amount; 
                    }


                    db.Transaction.Add(transaction);


                    db.SaveChanges();

                }

                ViewBag.Recon = "Transaction has already been reconciled";
            }
            return RedirectToAction("Index", "Households", new { id = transaction.FinancialAccountId});
        }

        // GET: Transactions/Details/5
        public PartialViewResult _TransDetails(int? id)
        {
            if (id == null)
            {
                return PartialView(HttpStatusCode.BadRequest);
            }
            Transactions transactions = db.Transaction.Find(id);
            var useraccount = db.Users.Find(User.Identity.GetUserId()).Household.FinancialAccount;

            if (transactions == null)
            {
                return PartialView(useraccount);
            }
            return PartialView(transactions);
        }

        // GET: Transactions/Create
        public ActionResult Create(int? id)
        {
            if(id != null)
            {
                var financialAccount = db.FinancialAccount.Find(id);
            }
            ViewBag.CategoryId = new SelectList(db.Category.OrderBy(u => u.Expense), "Id", "Name");
            ViewBag.LastUpdatedUserId = new SelectList(db.Users, "Id", "FirstName");
            ViewBag.TransactionUserId = new SelectList(db.Users, "Id", "FirstName");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Description,CategoryId,Amount,Reconciled,TransactionDate,TransactionUserId,LastUpdatedUserId,Updated")] Transactions transactions, int? id)
        {
            var financialAccount = db.FinancialAccount.Find(id);

            if (ModelState.IsValid)
            {
                var userid = User.Identity.GetUserId();

                transactions.TransactionUserId = userid;
                transactions.LastUpdatedUserId = userid;
                transactions.FinancialAccountId = financialAccount.Id;
                transactions.Created = System.DateTimeOffset.Now;
                transactions.Active = true;

                db.Transaction.Add(transactions);
                db.SaveChanges();

                transactions.Category = db.Category.FirstOrDefault(c => c.Id == transactions.CategoryId);
                db.SaveChanges();

                transactions = db.Transaction.Include("Category").FirstOrDefault(t => t.Id == transactions.Id);
                transactions.UpdateBalance(userid);

                return RedirectToAction("Index", "Households", new { id = financialAccount.Id });
            }

            ViewBag.CategoryId = new SelectList(db.Category, "Expense", "Name", transactions.CategoryId);
            ViewBag.FinancialAccountId = new SelectList(db.FinancialAccount, "Id", "Name", transactions.FinancialAccountId);
            ViewBag.LastUpdatedUserId = new SelectList(db.Users, "Id", "FirstName", transactions.LastUpdatedUserId);
            ViewBag.TransactionUserId = new SelectList(db.Users, "Id", "FirstName", transactions.TransactionUserId);
            return RedirectToAction("Index", "Households", new { id = financialAccount.Id });
        }

        // GET: Transactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transactions transactions = db.Transaction.Find(id);
            if (transactions == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Category, "Id", "Name", transactions.CategoryId);
            ViewBag.FinancialAccountId = new SelectList(db.FinancialAccount, "Id", "Name", transactions.FinancialAccountId);
            ViewBag.LastUpdatedUserId = new SelectList(db.Users, "Id", "FirstName", transactions.LastUpdatedUserId);
            ViewBag.TransactionUserId = new SelectList(db.Users, "Id", "FirstName", transactions.TransactionUserId);
            return View(transactions);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Description,CategoryId,Amount,Reconciled,TransactionDate,FinancialAccountId,TransactionUserId,LastUpdatedUserId")] Transactions transactions)
        {
            if (ModelState.IsValid)
            {
                var userid = User.Identity.GetUserId();
                var OldTrans = db.Transaction.AsNoTracking().FirstOrDefault(t => t.Id == transactions.Id);
                OldTrans.ReverseBal(userid);

                transactions.Active = true;
                transactions.LastUpdatedUserId = userid;
                transactions.Updated = System.DateTimeOffset.Now;
                db.Entry(transactions).State = EntityState.Modified;
                db.SaveChanges();

                var financialAccountid = db.FinancialAccount.FirstOrDefault(f => f.Id == transactions.FinancialAccountId).Id;
                transactions.FinancialAccountId = financialAccountid;
                transactions = db.Transaction.Include("Category").FirstOrDefault(t => t.Id == transactions.Id);
                transactions.UpdateBalance(userid);
                db.SaveChanges();

                return RedirectToAction("Index", "Households", new { id = financialAccountid });
            }
            ViewBag.CategoryId = new SelectList(db.Category, "Id", "Name", transactions.CategoryId);
            ViewBag.FinancialAccountId = new SelectList(db.FinancialAccount, "Id", "Name", transactions.FinancialAccountId);
            ViewBag.LastUpdatedUserId = new SelectList(db.Users, "Id", "FirstName", transactions.LastUpdatedUserId);
            ViewBag.TransactionUserId = new SelectList(db.Users, "Id", "FirstName", transactions.TransactionUserId);
            return RedirectToAction("Index", "Households");
        }

        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transactions transactions = db.Transaction.Find(id);
            if (transactions == null)
            {
                return HttpNotFound();
            }
            return View(transactions);
        }


        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transactions transactions = db.Transaction.Find(id);
            var user = transactions.TransactionUser;
            var account = transactions.FinancialAccount;

            if (transactions != null)
            {
                if(transactions.Active == true)
                {
                    if (transactions.Category.Expense == true)
                    {
                        account.Balance += transactions.Amount;
                    }
                    else
                    {
                        account.Balance -= transactions.Amount;
                    }
                }

                db.Transaction.Remove(transactions);
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Households", new { id = account.Id });
        }


        // POST: Transactions/Void/5
        [Authorize]
        public ActionResult Void(int id)
        {
            Transactions transactions = db.Transaction.Find(id);
            var user = transactions.TransactionUser;
            var account = transactions.FinancialAccount;

            if (transactions != null)
            {
                if (transactions.Active == true)
                {
                    if (transactions.Category.Expense == true)
                    {
                        account.Balance += transactions.Amount;
                    }
                    else
                    {
                        account.Balance -= transactions.Amount;
                    }


                    transactions.Active = false;
                    db.Entry(transactions).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    if (transactions.Category.Expense == true)
                    {
                        account.Balance -= transactions.Amount;
                    }
                    else
                    {
                        account.Balance += transactions.Amount;
                    }


                    transactions.Active = true;
                    db.Entry(transactions).State = EntityState.Modified;
                    db.SaveChanges();
                }

            }

            return RedirectToAction("Index","Households", new { id = account.Id});

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
