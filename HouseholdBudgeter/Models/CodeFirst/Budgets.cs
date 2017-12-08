using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models
{
    public class Budgets
    {
        public int Id { get; set; }
        public int HouseholdId { get; set; }
        [Required]
        public bool Expense { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; }
        [Required]
        public int Frequency { get; set; }
        public string AuthorUserId { get; set; }
        public string UpdatedLastUserId { get; set; }

        public virtual Households Household { get; set; }
        public virtual Categories Category { get; set; }
        public virtual ApplicationUser AuthorUser { get; set; }
        public virtual ApplicationUser UpdatedlastUser { get; set; }
        
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}