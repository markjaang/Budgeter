using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.Helpers
{
    public static class BalanceHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static void UpdateBalance(this Transactions transaction, string userId)
        {
            var user = db.Users.FirstOrDefault(u => u.Id.Equals(userId));
            var userHHID = Convert.ToInt32(user.HouseholdId);
            var account = db.FinancialAccount.FirstOrDefault(a => a.Id == transaction.FinancialAccountId);
            
            if (transaction.Category.Expense == true)
            {
                account.Balance -= transaction.Amount; // SUBTRACT
            }
            else
            {
                account.Balance += transaction.Amount; // ADD
            }

            db.SaveChanges();
        }

        public static void ReverseBal(this Transactions transaction, string userId)
        {
            var user = db.Users.FirstOrDefault(u => u.Id.Equals(userId));
            var userHHID = Convert.ToInt32(user.HouseholdId);
            var account = db.FinancialAccount.FirstOrDefault(a => a.Id == transaction.FinancialAccountId);

            if (transaction.Category.Expense == true)
            {
                account.Balance += transaction.Amount; // ADD BACK
            }
            else
            {
                account.Balance -= transaction.Amount; // SUBTRACT BACK
            }

            db.SaveChanges();
        }

        public static decimal GetBalance(this decimal amount, bool Expense)
        {
            decimal Balance = 0;
            if ( Expense == true )
            {
                Balance -= amount;
            }
            else
            {
                Balance += amount;
            }
            return Balance;
        }
    }
}