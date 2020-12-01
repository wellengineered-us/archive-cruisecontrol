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
    /// <title>Server Log Project Plugin</title>
    /// <version>1.0</version>
    /// <summary>
    /// The Server Log Project Plugin shows you recent activity that has been output to the server log for a specific project. 
    /// <para>
    /// LinkDescription : View Server Log.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;serverLogProjectPlugin /&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// Read the <link>Server Application Config File</link> page for more help on build server logging.
    /// </remarks>
    [ReflectorType("serverLogProjectPlugin")]
	public class ServerLogProjectPlugin : ICruiseAction, IPlugin
	{
		public const string ActionName = "ViewServerProjectLog";
		private readonly ServerLogServerPlugin plugin;

        public ServerLogProjectPlugin(IFarmService farmService, IVelocityViewGenerator viewGenerator, ICruiseUrlBuilder urlBuilder)
		{
			this.plugin = new ServerLogServerPlugin(farmService, viewGenerator, urlBuilder);
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