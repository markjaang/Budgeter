using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using HouseholdBudgeter.Models;
using HouseholdBudgeter;

namespace HouseholdBudgeter.Models
{
    public static class RefreshAuth
    {
        public static async Task RefreshAuthentication(this HttpContextBase context, ApplicationUser user)
        {
            context.GetOwinContext().Authentication.SignOut();
            await context.GetOwinContext().Get<ApplicationSignInManager>().SignInAsync(user, isPersistent: false, rememberBrowser: false);
        }
    }
}