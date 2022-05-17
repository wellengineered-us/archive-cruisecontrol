using System;
using System.Collections;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.Security
{
	public class LogoutSecurityAction
		: ICruiseAction
	{
		#region Public consts
		public const string ActionName = "ServerLogout";
		#endregion

		#region Private fields
		private readonly IFarmService farmService;
		private readonly IVelocityViewGenerator viewGenerator;
		private readonly ISessionStorer storer;
		#endregion

		#region Constructors
		public LogoutSecurityAction(IFarmService farmService, IVelocityViewGenerator viewGenerator,
			ISessionStorer storer)
		{
			this.farmService = farmService;
			this.viewGenerator = viewGenerator;
			this.storer = storer;
		}
		#endregion

		#region Public methods
		public IResponse Execute(ICruiseRequest cruiseRequest)
		{
			Hashtable velocityContext = new Hashtable();
			string sessionToken = cruiseRequest.RetrieveSessionToken();
			if (!string.IsNullOrEmpty(sessionToken))
				this.farmService.Logout(cruiseRequest.ServerName, sessionToken);
			this.storer.StoreSessionToken(null);
			this.storer.StoreDisplayName(null);
			return this.viewGenerator.GenerateView("LoggedOut.vm", velocityContext);
		}
		#endregion
	}
}
