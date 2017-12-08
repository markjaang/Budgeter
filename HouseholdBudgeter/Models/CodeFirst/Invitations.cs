using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models
{
    public class Invitations
    {
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string InviteEmail { get; set; }
        public int HouseholdId { get; set; }
        [Required]
        public string InviteName { get; set; }
        public virtual Households Household { get; set; }



    }
}