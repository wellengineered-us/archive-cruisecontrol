using System.Collections;

using WellEngineered.CruiseControl.WebDashboard.Configuration;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.Plugins.Security;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
    public class LoginViewBuilder
    {
        private readonly ICruiseRequest request;
        private readonly ILinkFactory linkFactory;
        private readonly IVelocityViewGenerator velocityViewGenerator;
        private readonly IDashboardConfiguration configuration;
		private readonly ISessionRetriever retriever;

        public LoginViewBuilder(ICruiseRequest request, ILinkFactory linkFactory, 
            IVelocityViewGenerator velocityViewGenerator, IDashboardConfiguration configuration,
            ISessionRetriever retriever)
        {
            this.request = request;
            this.linkFactory = linkFactory;
            this.velocityViewGenerator = velocityViewGenerator;
            this.configuration = configuration;
            this.retriever = retriever;

            this.BuildServerName = request.ServerName;
            this.ProjectName = request.ProjectName;
        }


        public readonly string BuildServerName;
        public readonly string ProjectName;

        public HtmlFragmentResponse Execute()
        {
            Hashtable velocityContext = new Hashtable();
            velocityContext["changePassword"] = string.Empty;

            string serverName = this.request.ServerName;
            if (!string.IsNullOrEmpty(serverName) && (this.configuration.PluginConfiguration.SecurityPlugins.Length > 0))
            {
                string sessionToken = this.retriever.RetrieveSessionToken(this.request.Request);

                string userName = string.Empty;


                if (string.IsNullOrEmpty(sessionToken))
                {
                    velocityContext["action"] = this.linkFactory.CreateServerLink(this.request.ServerSpecifier,
                        "Login",
                        this.configuration.PluginConfiguration.SecurityPlugins[0].NamedActions[0].ActionName);
                }
                else
                {
                    velocityContext["action"] = this.linkFactory.CreateServerLink(this.request.ServerSpecifier,
                        "Logout",
                        LogoutSecurityAction.ActionName);

                    velocityContext["changePassword"] = this.linkFactory.CreateServerLink(this.request.ServerSpecifier,
                        "Change Password",
                        ChangePasswordSecurityAction.ActionName);

                    string displayName = this.retriever.RetrieveDisplayName(this.request.Request);

                    if (string.IsNullOrEmpty(displayName))
                        userName = sessionToken;
                    else
                        userName = displayName;
                }

                velocityContext["userName"] = userName;

            }
            else
            {
                velocityContext["action"] = string.Empty;
            }

            return this.velocityViewGenerator.GenerateView("LoginAction.vm", velocityContext);
        }
    }
}
