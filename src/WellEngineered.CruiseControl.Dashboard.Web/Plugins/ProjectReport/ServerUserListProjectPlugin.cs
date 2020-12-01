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
    /// <title>User List Project Plugin</title>
    /// <version>1.5</version>
    /// <summary>
    /// Displays all the users in the system, plus the security rights they have for the project.
    /// <para>
    /// LinkDescription : View User List.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;serverUserListProjectPlugin /&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// <para type="tip">
    /// This can be installed using the "User List" package.
    /// </para>
    /// </remarks>
    [ReflectorType("serverUserListProjectPlugin")]
	public class ServerUserListProjectPlugin : ICruiseAction, IPlugin
	{
        public const string ActionName = "ViewProjectUserList";
        private readonly ServerUserListServerPlugin plugin;

		public ServerUserListProjectPlugin(IFarmService farmService, 
            IVelocityViewGenerator viewGenerator, 
            ISessionRetriever sessionRetriever,
            IUrlBuilder urlBuilder)
		{
            this.plugin = new ServerUserListServerPlugin(farmService, viewGenerator, sessionRetriever, urlBuilder);
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
