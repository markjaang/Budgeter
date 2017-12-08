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
    public class BudgetsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Budgets
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            Households household = user.Household;
            var budget = household.Budget;
            return View(budget.ToList());
        }

        // GET: Budgets/Details/5
        public PartialViewResult _BudDetails(int? id)
        {
            if (id == null)
            {
                return PartialView(HttpStatusCode.BadRequest);
            }
            Budgets budgets = db.Budget.Find(id);

            if (budgets == null)
            {
                return PartialView ("Index","Households");
            }
            return PartialView(budgets);
        }

        // GET: Budgets/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Category.OrderBy(c => c.Expense), "Id", "Name");
            return View();
        }

        // POST: Budgets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CategoryId,Description,Amount")] Budgets budgets)
        {
            

            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                var household = user.Household;

                budgets.HouseholdId = household.Id;
                budgets.Household = household;
                budgets.AuthorUserId = user.Id;
                budgets.AuthorUser = user;
                budgets.UpdatedLastUserId = user.Id;
                budgets.UpdatedlastUser = user;
                budgets.Created = System.DateTimeOffset.Now;
                db.Budget.Add(budgets);
                db.SaveChanges();
                return RedirectToAction("Index", "Households");
            }

            ViewBag.CategoryId = new SelectList(db.Category, "Id", "Name", budgets.CategoryId);
            return RedirectToAction("Index", "Households");
        }

        // GET: Budgets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budgets budget = db.Budget.Find(id);

            if (budget == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Category, "Id", "Name", budget.CategoryId);
            return View(budget);
        }

        // POST: Budgets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Created,CategoryId,Description,Amount")] Budgets budgets)
        {
            
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                var household = user.Household;
                budgets.HouseholdId = household.Id;
                budgets.UpdatedLastUserId = user.Id;
                budgets.Updated = System.DateTimeOffset.Now;
                db.Entry(budgets).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Households");
            }

            ViewBag.CategoryId = new SelectList(db.Category, "Id", "Name", budgets.CategoryId);

            return RedirectToAction("Index", "Households");
        }

        // GET: Budgets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budgets budgets = db.Budget.Find(id);
            if (budgets == null)
            {
                return HttpNotFound();
            }
            return View(budgets);
        }

        // POST: Budgets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Budgets budgets = db.Budget.Find(id);
            db.Budget.Remove(budgets);
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
