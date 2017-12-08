using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;


public static class Extensions
{
    public static string GetHouseholdId(this IIdentity user)
    {
        var claimsIdentity = (ClaimsIdentity)user;
        var HouseholdClaim = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "HouseholdId");

        if (HouseholdClaim != null)
            return HouseholdClaim.Value;
        else
            return null;
    }

    public static bool IsInHousehold(this IIdentity user)
    {
        var cUser = (ClaimsIdentity)user;
        var hid = cUser.Claims.FirstOrDefault(c => c.Type == "HouseholdId");
        return (hid != null && !string.IsNullOrWhiteSpace(hid.Value));
    }
}