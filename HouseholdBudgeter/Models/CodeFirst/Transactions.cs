using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models
{
    public class Transactions
    {
        public bool Active { get; set; }
        public int Id { get; set; }
        public string Description { get; set; }
        [Required]
        public int? CategoryId { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; }
        [Required]
        public bool Reconciled { get; set; }
        public DateTimeOffset TransactionDate { get; set; }

        public int FinancialAccountId { get; set; }
        public string TransactionUserId { get; set; }
        public string LastUpdatedUserId { get; set; }

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public virtual Categories Category { get; set; }
        public virtual FinancialAccounts FinancialAccount { get; set; }
        public virtual ApplicationUser TransactionUser { get; set; }
        public virtual ApplicationUser LastUpdatedUser { get; set; }
    }
}