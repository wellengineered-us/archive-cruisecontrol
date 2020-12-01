using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.Plugins.ServerReport;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.ProjectReport
{
    /// <title>Server Audit History Project Plugin</title>
    /// <version>1.5</version>
    /// <summary>
    /// The Server Audit History Project Plugin displays the audit log from the project.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;serverAuditHistoryProjectPlugin /&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// This requires that the currently logged in user has the required permissions on the server.
    /// </remarks>
    [ReflectorType("serverAuditHistoryProjectPlugin")]
	public class ServerAuditHistoryProjectPlugin : ICruiseAction, IPlugin
	{
        public const string ActionName = "ViewProjectAuditHistory";
        private readonly ServerAuditHistoryServerPlugin plugin;

		public ServerAuditHistoryProjectPlugin(IFarmService farmService, 
            IVelocityViewGenerator viewGenerator, 
            ISessionRetriever sessionRetriever,
            IUrlBuilder urlBuilder)
		{
            this.plugin = new ServerAuditHistoryServerPlugin(farmService, viewGenerator, sessionRetriever, urlBuilder);
		}

		public IResponse Execute(ICruiseRequest request)
		{
			return this.plugin.Execute(request);
		}

		public string LinkDescription
		{
			get { return this.plugin.LinkDescription; }
		}

		public INamedAction[] NamedActions
		{
			get {  return new INamedAction[] { new ImmutableNamedAction(ActionName, this) }; }
		}	
	}
}
