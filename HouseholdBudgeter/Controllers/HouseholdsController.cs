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
using System.Threading.Tasks;

namespace HouseholdBudgeter.Controllers
{
    public class HouseholdsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Households
        [Authorize]
        public ActionResult Index(int? id)
        {
            ViewBag.AcctId = id;

            var user = db.Users.Find(User.Identity.GetUserId());
            var transactions = user.Household.FinancialAccount.SelectMany(a => a.Transaction).ToList();
            var incomeItems = transactions.Where(t => t.Category.Expense == false && t.Active == true);
            var expenseItems = transactions.Where(t => t.Category.Expense == true && t.Active == true);
            var incomeTotal = incomeItems.Where(item => item.Reconciled == true).Sum(item => item.Amount);
            var expenseTotal = expenseItems.Where(item => item.Reconciled == true).Sum(item => item.Amount);
            var household = user.Household;
            var financialAccount = db.FinancialAccount.Where(f => f.Household.Id == household.Id && f.Active == true).ToList();
            var budgetexptotal = household.Budget.Where(b => b.HouseholdId == household.Id).Where(b => b.Category.Expense == true).Sum(b => b.Amount);
            var budgetinctotal = household.Budget.Where(b => b.HouseholdId == household.Id).Where(b => b.Category.Expense == false).Sum(b => b.Amount);
            if (household != null)
            {
                var houseBud = db.Budget.Where(b => b.HouseholdId == household.Id);
                var accountCount = household.FinancialAccount.Where(u => u.Active == true).Count();
                if (accountCount > 0)
                {
                    var totalInc = incomeItems.Sum(t => t.Amount);
                    var totalExp = expenseItems.Sum(t => t.Amount);
                    var totalamt = incomeItems.Sum(t => t.Amount) - expenseItems.Sum(t => t.Amount);
                    
                    ViewBag.incTotal = totalInc;
                    ViewBag.expTotal = totalExp;
                    ViewBag.amtTotal = totalamt;
                    ViewBag.budgetTotal = budgetexptotal - budgetinctotal;
                    ViewBag.recTotal = incomeTotal - expenseTotal;
                    ViewBag.remBud = budgetexptotal - budgetinctotal - totalExp;
                    ViewBag.account = financialAccount.ToList();
                }

                if(houseBud != null)
                {
                    var budget = household.Budget.ToList();
                    ViewBag.houseBudget = budget;
                }
                ViewBag.openBud = "Please make budget";
                ViewBag.openAcc = "Please create an account";
                return View(household);
            }
            else
            {
                ViewBag.JoinHouse = "First create or join a household";
                return RedirectToAction("CreateJoinHousehold", "Households");
            }
        }

        
        // GET: Households/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Households households = db.Household.Find(id);
            if (households == null)
            {
                return HttpNotFound();
            }
            return View(households);
        }

        // GET: Households/CreateJoinHousehold
        [Authorize]
        public ActionResult CreateJoinHousehold(int? id)
        {
            Households household = db.Household.Find(id);
            if (household == null)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                var email = user.Email;

                if (email != null)
                {
                    var previoushouse = user.PreviousHouseholdId;
                    var invitation = db.Invitation.FirstOrDefault(i => i.InviteEmail == email);

                    if (previoushouse != null)
                    {
                        user.HouseholdId = previoushouse;
                        db.SaveChanges();

                        var prevhousehold = db.Household.Where(p => p.Id == previoushouse).ToList();

                        ViewBag.HouseholdArchive = new SelectList(prevhousehold, "Id", "Name");

                        user.HouseholdId = null;
                        db.SaveChanges();
                    }
                    if (user != null && invitation != null)
                    {
                        var householdid = invitation.HouseholdId;
                        var invitehousehold = db.Household.Where(i => i.Id == householdid).ToList();

                        ViewBag.HouseholdInv = new SelectList(invitehousehold, "Id", "Name");
                    }
                    
                }
            }

            return View(household);
            
        }

        // POST: Households/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult>Create([Bind(Include = "Id,Name")] Households household)
        {
            
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                if (user.HouseholdId == null)
                {
                    household.Created = System.DateTimeOffset.Now;
                    db.Household.Add(household);
                    db.SaveChanges();
                    var attachuser = db.Household.FirstOrDefault(h => h.Id == household.Id);
                    user.HouseholdId = attachuser.Id;
                    db.SaveChanges();

                    await ControllerContext.HttpContext.RefreshAuthentication(user);
                    return RedirectToAction("Index");
                }

                return RedirectToAction("Index", "Households", new { id = user.HouseholdId });
            }

            return View(household);
        }
        
        // POST: Households/Join
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task <ActionResult> Join()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var invitation = db.Invitation.Where(i => i.InviteEmail == user.Email).ToList();
            Households household = db.Household.FirstOrDefault(h => h.Id == user.HouseholdId);
            if (user != null && invitation != null && household == null)
            {
                foreach (var inv in invitation)
                {
                    var householdid = inv.HouseholdId;
                    user.HouseholdId = householdid;
                    db.Invitation.Remove(inv);
                    db.SaveChanges();
                }

                await ControllerContext.HttpContext.RefreshAuthentication(user);
                return RedirectToAction("Index", "Households", new { id = user.HouseholdId });
            }
            return RedirectToAction("CreateJoinHousehold", "Households");
        }

        //POST:Households/Archive/5
        [HttpPost]
        public ActionResult Archive()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            user.HouseholdId = user.PreviousHouseholdId;
            user.PreviousHouseholdId = null;
            db.SaveChanges();

            return RedirectToAction("Index", "Home", new { id = user.HouseholdId });
        } 


        // GET: Households/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Households households = db.Household.Find(id);
            if (households == null)
            {
                return HttpNotFound();
            }
            return View(households);
        }

        // POST: Households/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Created,Updated")] Households households)
        {
            if (ModelState.IsValid)
            {
                db.Entry(households).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(households);
        }

        // GET: Households/Leave/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Households households = db.Household.Find(id);
            if (households == null)
            {
                return HttpNotFound();
            }
            return View(households);
        }

        // POST: Households/Leave/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task <ActionResult> DeleteConfirmed(int id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            if(user != null)
            {
                user.PreviousHouseholdId = user.HouseholdId;
                user.HouseholdId = null;
                db.SaveChanges();
                await ControllerContext.HttpContext.RefreshAuthentication(user);
                return RedirectToAction("CreateJoinHousehold", "Households");
            }

            return RedirectToAction("Index");

        }

        // GET: Households/Invitation/5
        [HttpGet]
        public ActionResult Invite()
        {
            return View();
        }

        //POST: Households/Invitation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task <ActionResult> Invite([Bind(Include = "InviteEmail, InviteName")] Invitations invitation)
        {

            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
                var householdid = user.HouseholdId;
                if (householdid != null)
                {
                    invitation.HouseholdId = (int)householdid;
                    db.Invitation.Add(invitation);
                    db.SaveChanges();

                    var invite = invitation.InviteEmail;
                    var receiver = db.Invitation.Find(invitation.Id).InviteName;
                    var callBackUrl_NonUser = Url.Action("Register", "Account", new { id = invitation.HouseholdId, inviteId = invitation.Id }, protocol: Request.Url.Scheme);
                    var callBackUrl_IsUser = Url.Action("CreateJoinHousehold", "Households", new { id = invitation.HouseholdId, inviteId = invitation.Id }, protocol: Request.Url.Scheme);
                    var email = new EmailService();
                    var mail = new IdentityMessage
                    {
                        Subject = $"BudgetPro Invitation from {user.FirstName}{user.LastName}",
                        Destination = invite,
                        Body = $"Hi {receiver}! You have been invited to join BudgetPro by {user.FirstName}{user.LastName}. If you have already registered please click <a href=\"" + callBackUrl_IsUser + "\">here</a> to join. If you have not registered please click <a href=\"" + callBackUrl_NonUser + "\">here</a> to register."
                    };
                    await email.SendAsync(mail);

                    return RedirectToAction("Index");
                }
                return RedirectToAction("Index");
            }
            return View("Invite","Households");
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
