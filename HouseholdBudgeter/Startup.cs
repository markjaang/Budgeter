using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HouseholdBudgeter.Startup))]
namespace HouseholdBudgeter
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
