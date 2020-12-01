using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.DeleteProject
{
	public class ShowDeleteProjectAction : ICruiseAction
	{
		public static readonly string ACTION_NAME = "ShowDeleteProject";

		private readonly IDeleteProjectViewBuilder viewBuilder;

		public ShowDeleteProjectAction(IDeleteProjectViewBuilder viewBuilder)
		{
			this.viewBuilder = viewBuilder;
		}

		public IResponse Execute(ICruiseRequest request)
		{
			return this.viewBuilder.BuildView(this.BuildModel(request.ProjectSpecifier));
		}

		private DeleteProjectModel BuildModel(IProjectSpecifier projectSpecifier)
		{
			return new DeleteProjectModel(projectSpecifier, string.Format("Please confirm you want to delete {0}, and choose which extra delete actions you want to perform", projectSpecifier.ProjectName), true, true, true, true);
		}
	}
}
