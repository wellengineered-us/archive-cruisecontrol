using System;
using System.Collections;
using System.Collections.Generic;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote.Security;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.ServerReport
{
	/// <title>User List Server Plugin</title>
	/// <version>1.5</version>
	/// <summary>
	/// Displays all the users in the system, plus the security rights they have on the server.
	/// </summary>
	/// <example>
	/// <code title="Minimalist Example">
	/// &lt;serverUserListServerPlugin /&gt;
	/// </code>
	/// <code title="Full Example">
	/// &lt;serverUserListServerPlugin resetPassword="anewpassword" /&gt;
	/// </code>
	/// </example>
	/// <remarks>
	/// <para type="tip">
	/// This can be installed using the "User List" package.
	/// </para>
	/// </remarks>
	[ReflectorType("serverUserListServerPlugin")]
	public class ServerUserListServerPlugin : ICruiseAction, IPlugin
	{
		#region Private consts
		private const string ActionName = "ViewServerUserList";
		private const string DiagnosticsActionName = "ViewUserSecurityDiagnostics";
		#endregion

		#region Private fields
		private readonly IFarmService farmService;
		private readonly IVelocityViewGenerator viewGenerator;
		private readonly ISessionRetriever sessionRetriever;
		private readonly IUrlBuilder urlBuilder;
		private string resetPassword = "password";
		#endregion

		#region Constructors
		public ServerUserListServerPlugin(IFarmService farmService,
			IVelocityViewGenerator viewGenerator,
			ISessionRetriever sessionRetriever,
			IUrlBuilder urlBuilder)
		{
			this.farmService = farmService;
			this.viewGenerator = viewGenerator;
			this.sessionRetriever = sessionRetriever;
			this.urlBuilder = urlBuilder;
		}
		#endregion

		#region Public properties
		public string LinkDescription
		{
			get { return "View User List"; }
		}

		public INamedAction[] NamedActions
		{
			get
			{
				return new INamedAction[] { new ImmutableNamedAction(ActionName, this),
                        new ImmutableNamedActionWithoutSiteTemplate(DiagnosticsActionName, this)
                    };
			}
		}

		/// <summary>
		/// The password to use when reseting the password.
		/// </summary>
		/// <version>1.5</version>
		/// <default>None</default>
		[ReflectorProperty("resetPassword", Required = false)]
		public string ResetPassword
		{
			get { return this.resetPassword; }
			set { this.resetPassword = value; }
		}
		#endregion

		#region Public methods
		public IResponse Execute(ICruiseRequest request)
		{
			string userName = __Fixup.GetCurrentHttpContext().Request.Form["user"];
			string action = __Fixup.GetCurrentHttpContext().Request.Form["action"];
			if (string.IsNullOrEmpty(userName))
			{
				return this.GenerateUserList(request, string.Empty, string.Empty);
			}
			else
			{
				if (action == "Reset password")
				{
					string message = string.Empty;
					string errorMsg = string.Empty;
					try
					{
						string sessionToken = this.sessionRetriever.RetrieveSessionToken(request.Request);
						this.farmService.ResetPassword(request.ServerName, sessionToken, userName, this.resetPassword);
						message = "The password has been reset";
					}
					catch (Exception error)
					{
						errorMsg = error.Message;
					}
					return this.GenerateUserList(request, message, errorMsg);
				}
				else
				{
					return this.GenerateUserDiagnostics(request, userName);
				}
			}
		}
		#endregion

		#region Private methdods
		private IResponse GenerateUserDiagnostics(ICruiseRequest request, string userName)
		{
			Hashtable velocityContext = new Hashtable();

			velocityContext["userName"] = userName;
			velocityContext["message"] = string.Empty;
			string sessionToken = request.RetrieveSessionToken(this.sessionRetriever);
			if (!string.IsNullOrEmpty(request.ProjectName))
			{
				velocityContext["projectName"] = request.ProjectName;
				velocityContext["diagnostics"] = this.farmService.DiagnoseSecurityPermissions(request.ProjectSpecifier, sessionToken, userName);
			}
			else
			{
				velocityContext["diagnostics"] = this.farmService.DiagnoseSecurityPermissions(request.ServerSpecifier, sessionToken, userName);
			}

			return this.viewGenerator.GenerateView(@"UserDiagnostics.vm", velocityContext);
		}

		private IResponse GenerateUserList(ICruiseRequest request, string message, string error)
		{
			Hashtable velocityContext = new Hashtable();
			velocityContext["message"] = message;
			velocityContext["error"] = error;

			var links = new List<IAbsoluteLink>();
			links.Add(new ServerLink(request.UrlBuilder, request.ServerSpecifier, "User List", ActionName));

			ProjectStatusListAndExceptions projects = this.farmService.GetProjectStatusListAndCaptureExceptions(request.ServerSpecifier, request.RetrieveSessionToken());
			foreach (ProjectStatusOnServer projectStatusOnServer in projects.StatusAndServerList)
			{
				DefaultProjectSpecifier projectSpecifier = new DefaultProjectSpecifier(projectStatusOnServer.ServerSpecifier, projectStatusOnServer.ProjectStatus.Name);
				links.Add(new ProjectLink(request.UrlBuilder, projectSpecifier, projectSpecifier.ProjectName, ServerUserListServerPlugin.ActionName));
			}
			velocityContext["projectLinks"] = links;
			string sessionToken = request.RetrieveSessionToken(this.sessionRetriever);
			List<UserDetails> allUsers = this.farmService.ListAllUsers(request.ServerSpecifier, sessionToken);
			foreach (UserDetails user in allUsers)
			{
				if (user.DisplayName == null) user.DisplayName = string.Empty;
			}
			velocityContext["users"] = allUsers;
			if (!string.IsNullOrEmpty(request.ProjectName))
			{
				velocityContext["currentProject"] = request.ProjectName;
				velocityContext["diagnosticsCall"] = new ProjectLink(request.UrlBuilder, request.ProjectSpecifier, string.Empty, DiagnosticsActionName);
			}
			else
			{
				velocityContext["diagnosticsCall"] = new ServerLink(request.UrlBuilder, request.ServerSpecifier, string.Empty, DiagnosticsActionName);
			}

			return this.viewGenerator.GenerateView(@"UserList.vm", velocityContext);
		}
		#endregion
	}
}
