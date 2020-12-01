using System.Collections;
using System.Collections.Generic;
using System.Web;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.Plugins.ProjectReport;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.ServerReport
{
    /// <title>Server Log Server Plugin</title>
    /// <version>1.0</version>
    /// <summary>
    /// The Server Log Server Plugin shows you recent activity that has been output to the server log for a specific build server.
    /// <para>
    /// LinkDescription : View Server Log.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;serverLogServerPlugin /&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// Read the <link>Server Application Config File</link> page for more help on build server logging.
    /// </remarks>
	[ReflectorType("serverLogServerPlugin")]
	public class ServerLogServerPlugin : ICruiseAction, IPlugin
	{
		private const string ActionName = "ViewServerLog";
		private readonly IFarmService farmService;
		private readonly IVelocityViewGenerator viewGenerator;
		private readonly ICruiseUrlBuilder urlBuilder;

        public ServerLogServerPlugin(IFarmService farmService, IVelocityViewGenerator viewGenerator, ICruiseUrlBuilder urlBuilder)
		{
			this.farmService = farmService;
			this.viewGenerator = viewGenerator;
			this.urlBuilder = urlBuilder;
		}

		public IResponse Execute(ICruiseRequest request)
		{
			Hashtable velocityContext = new Hashtable();
			var links = new List<IAbsoluteLink>();
			links.Add(new ServerLink(this.urlBuilder, request.ServerSpecifier, "Server Log", ActionName));

			ProjectStatusListAndExceptions projects = this.farmService.GetProjectStatusListAndCaptureExceptions(request.ServerSpecifier,
                request.RetrieveSessionToken());
			foreach (ProjectStatusOnServer projectStatusOnServer in projects.StatusAndServerList)
			{
				DefaultProjectSpecifier projectSpecifier = new DefaultProjectSpecifier(projectStatusOnServer.ServerSpecifier, projectStatusOnServer.ProjectStatus.Name);
				links.Add(new ProjectLink(this.urlBuilder, projectSpecifier, projectSpecifier.ProjectName, ServerLogProjectPlugin.ActionName));
			}
			velocityContext["projectLinks"] = links;
            if (string.IsNullOrEmpty(request.ProjectName))
			{
				velocityContext["log"] = HttpUtility.HtmlEncode(this.farmService.GetServerLog(request.ServerSpecifier, request.RetrieveSessionToken()));
			}
			else
			{
				velocityContext["currentProject"] = request.ProjectSpecifier.ProjectName;
				velocityContext["log"] = HttpUtility.HtmlEncode(this.farmService.GetServerLog(request.ProjectSpecifier, request.RetrieveSessionToken()));
			}

			return this.viewGenerator.GenerateView(@"ServerLog.vm", velocityContext);
		}

		public string LinkDescription
		{
			get { return "View Server Log"; }
		}

		public INamedAction[] NamedActions
		{
			get {  return new INamedAction[] { new ImmutableNamedAction(ActionName, this) }; }
		}
	}
}
