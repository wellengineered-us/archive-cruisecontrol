using System;
using System.Collections;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.Remote.Messages;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.Security
{
    public class UserNameSecurityAction
        : ICruiseAction
    {
        #region Private fields
        private readonly IFarmService farmService;
        private readonly IVelocityViewGenerator viewGenerator;
        private readonly ISessionStorer storer;
        private bool hidePassword;
        #endregion

        #region Constructors
        public UserNameSecurityAction(IFarmService farmService, IVelocityViewGenerator viewGenerator,
            ISessionStorer storer, bool hidePassword)
        {
            this.farmService = farmService;
            this.viewGenerator = viewGenerator;
            this.storer = storer;
            this.hidePassword = hidePassword;
        }
        #endregion

        #region Public methods
        public IResponse Execute(ICruiseRequest cruiseRequest)
        {
            Hashtable velocityContext = new Hashtable();
            string userName = cruiseRequest.Request.GetText("userName");
            string template = @"UserNameLogin.vm";
            if (!string.IsNullOrEmpty(userName))
            {
                try
                {
                    LoginRequest credentials = new LoginRequest(userName);
                    string password = cruiseRequest.Request.GetText("password");
                    if (!string.IsNullOrEmpty(password)) credentials.AddCredential(LoginRequest.PasswordCredential, password);
                    string sessionToken = this.farmService.Login(cruiseRequest.ServerName, credentials);
                    if (string.IsNullOrEmpty(sessionToken)) throw new CruiseControlException("Login failed!");
                    this.storer.StoreSessionToken(sessionToken);
                    this.storer.StoreDisplayName(this.farmService.GetDisplayName(cruiseRequest.ServerName, sessionToken));
                    template = "LoggedIn.vm";
                }
                catch (Exception error)
                {
                    velocityContext["errorMessage"] = error.Message;
                }
            }
            velocityContext["hidePassword"] = this.hidePassword;
            return this.viewGenerator.GenerateView(template, velocityContext);
        }
        #endregion
    }
}
