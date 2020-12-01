using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.BuildReport
{
    /// <summary>
    /// Shows the build page <link>Build Report Build Plugin</link> of the latest build of the project.
    /// <para>
    /// LinkDescription : Latest Build.
    /// </para>
    /// </summary>
    /// <title>Latest Build Report Project Plugin</title>
	[ReflectorType("latestBuildReportProjectPlugin")]
	public class LatestBuildReportProjectPlugin : ICruiseAction, IPlugin
	{
		private readonly IFarmService farmService;
		private readonly ILinkFactory linkFactory;
		public static readonly string ACTION_NAME = "ViewLatestBuildReport";

		public LatestBuildReportProjectPlugin(IFarmService farmService, ILinkFactory linkFactory)
		{
			this.farmService = farmService;
			this.linkFactory = linkFactory;
		}

		public IResponse Execute(ICruiseRequest cruiseRequest)
		{
			IProjectSpecifier projectSpecifier = cruiseRequest.ProjectSpecifier;
			IBuildSpecifier[] buildSpecifiers = this.farmService.GetMostRecentBuildSpecifiers(projectSpecifier, 1, cruiseRequest.RetrieveSessionToken());
			if (buildSpecifiers.Length == 1)
			{
				return new RedirectResponse(this.linkFactory.CreateBuildLink(buildSpecifiers[0], BuildReportBuildPlugin.ACTION_NAME).Url);
			}
			else
			{
				return new HtmlFragmentResponse("There are no complete builds for this project");
			}
		}

		public INamedAction[] NamedActions
		{
			get {  return new INamedAction[] { new ImmutableNamedAction(ACTION_NAME, this) }; }
		}

		public string LinkDescription
		{
			get { return "Latest Build"; }
		}
	}
}