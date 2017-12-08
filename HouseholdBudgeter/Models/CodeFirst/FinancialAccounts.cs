using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models
{
    public class FinancialAccounts
    {
        public bool Active { get; set; }
        public FinancialAccounts()
        {
            this.Transaction = new HashSet<Transactions>();
        }
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal Balance { get; set; }
        [Required]
        public int HouseholdId { get; set; }
        public virtual ICollection<Transactions> Transaction { get; set; }
        

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public DateTimeOffset? LastReconciledDate { get; set; }

        public virtual Households Household { get; set; }
    }
}