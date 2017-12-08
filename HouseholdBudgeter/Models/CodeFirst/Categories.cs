using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models
{
    public class Categories
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public bool Expense { get; set; }
        public int? HouseholdId { get; set; }

        public virtual Households Household { get; set; }
    }
}