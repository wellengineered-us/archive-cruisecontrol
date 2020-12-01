using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.ViewAllBuilds
{
    /// <title>View All Builds Project Plugin</title>
    /// <version>1.0</version>
	/// <summary>
    /// The View All Builds Project Plugin lists all available builds for a project.
    /// <para>
    /// LinkDescription : View All Builds.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;viewAllBuildsProjectPlugin /&gt;
    /// </code>
    /// </example>
	[ReflectorType("viewAllBuildsProjectPlugin")]
	public class ViewAllBuildsProjectPlugin : ICruiseAction, IPlugin
	{
		public static readonly string ACTION_NAME = "ViewAllBuilds";

		private readonly IAllBuildsViewBuilder viewBuilder;

		public ViewAllBuildsProjectPlugin (IAllBuildsViewBuilder viewBuilder)
		{
			this.viewBuilder = viewBuilder;
		}

		public IResponse Execute(ICruiseRequest cruiseRequest)
		{
			return this.viewBuilder.GenerateAllBuildsView(cruiseRequest.ProjectSpecifier, cruiseRequest.RetrieveSessionToken());
		}

		public string LinkDescription
		{
			get { return "View All Builds"; }
		}

		public INamedAction[] NamedActions
		{
			get {  return new INamedAction[] { new ImmutableNamedAction(ACTION_NAME, this) }; }
		}
	}
}
