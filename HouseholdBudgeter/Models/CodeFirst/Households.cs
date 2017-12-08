using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models
{
    public class Households
    {
        public Households()
        {
            this.User = new HashSet<ApplicationUser>();
            this.FinancialAccount = new HashSet<FinancialAccounts>();
            this.Invitation = new HashSet<Invitations>();
            this.Budget = new HashSet<Budgets>();
            this.Category = new HashSet<Categories>();
        }
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public virtual ICollection<Budgets> Budget { get; set; }
        public virtual ICollection<ApplicationUser> User { get; set; }
        public virtual ICollection<FinancialAccounts> FinancialAccount { get; set; }
        public virtual ICollection<Invitations> Invitation { get; set; }
        public virtual ICollection<Categories> Category { get; set; }


    }
}