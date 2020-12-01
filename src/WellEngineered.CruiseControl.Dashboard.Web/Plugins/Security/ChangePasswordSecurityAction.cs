using System;
using System.Collections;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.Security
{
    public class ChangePasswordSecurityAction
        : ICruiseAction
    {
        #region Public consts
        public const string ActionName = "ServerChangePassword";
        #endregion

        #region Private fields
        private readonly IFarmService farmService;
        private readonly IVelocityViewGenerator viewGenerator;
        #endregion

        #region Constructors
        public ChangePasswordSecurityAction(IFarmService farmService, IVelocityViewGenerator viewGenerator,
            ISessionStorer storer)
        {
            this.farmService = farmService;
            this.viewGenerator = viewGenerator;
        }
        #endregion

        #region Public methods
        public IResponse Execute(ICruiseRequest cruiseRequest)
        {
            Hashtable velocityContext = new Hashtable();
            velocityContext["message"] = string.Empty;
            velocityContext["error"] = string.Empty;
            string oldPassword = cruiseRequest.Request.GetText("oldPassword");
            string newPassword1 = cruiseRequest.Request.GetText("newPassword1");
            string newPassword2 = cruiseRequest.Request.GetText("newPassword2");
            if (!string.IsNullOrEmpty(oldPassword) &&
                !string.IsNullOrEmpty(newPassword1))
            {
                try
                {
                    if (newPassword1 != newPassword2) throw new CruiseControlException("New passwords do not match");
					string sessionToken = cruiseRequest.RetrieveSessionToken();
                    this.farmService.ChangePassword(cruiseRequest.ServerName, sessionToken, oldPassword, newPassword1);
                    velocityContext["message"] = "Password has been changed";
                }
                catch (Exception error)
                {
                    velocityContext["error"] = error.Message;
                }
            }
            return this.viewGenerator.GenerateView("ChangePasswordAction.vm", velocityContext);
        }
        #endregion
    }
}
