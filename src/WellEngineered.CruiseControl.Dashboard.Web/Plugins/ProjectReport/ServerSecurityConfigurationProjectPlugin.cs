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
    /// <title>Server Security Configuration Project Plugin</title>
    /// <version>1.5</version>
    /// <summary>
    /// Displays the security configuration for a project on a build server.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;serverSecurityConfigurationProjectPlugin /&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// <para type="tip">
    /// This can be installed using the "Security Configuration Display" package.
    /// </para>
    /// </remarks>
    [ReflectorType("serverSecurityConfigurationProjectPlugin")]
	public class ServerSecurityConfigurationProjectPlugin : ICruiseAction, IPlugin
	{
        public const string ActionName = "ViewProjectSecurityConfiguration";
        private readonly ServerSecurityConfigurationServerPlugin plugin;

		public ServerSecurityConfigurationProjectPlugin(IFarmService farmService, 
            IVelocityViewGenerator viewGenerator, 
            ISessionRetriever sessionRetriever)
		{
            this.plugin = new ServerSecurityConfigurationServerPlugin(farmService, viewGenerator, sessionRetriever);
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
