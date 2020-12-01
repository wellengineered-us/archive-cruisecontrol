using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.DeleteProject
{
	public class DoDeleteProjectAction : ICruiseAction
	{
		public static readonly string ACTION_NAME = "DoDeleteProject";

		private readonly IFarmService farmService;
		private readonly IDeleteProjectViewBuilder viewBuilder;

		public DoDeleteProjectAction(IDeleteProjectViewBuilder viewBuilder, IFarmService farmService)
		{
			this.viewBuilder = viewBuilder;
			this.farmService = farmService;
		}

		public IResponse Execute(ICruiseRequest request)
		{
			IProjectSpecifier projectSpecifier = request.ProjectSpecifier;
			bool purgeWorkingDirectory = request.Request.GetChecked("PurgeWorkingDirectory");
			bool purgeArtifactDirectory = request.Request.GetChecked("PurgeArtifactDirectory");
			bool purgeSourceControlEnvironment = request.Request.GetChecked("PurgeSourceControlEnvironment");
			this.farmService.DeleteProject(projectSpecifier, purgeWorkingDirectory, purgeArtifactDirectory, purgeSourceControlEnvironment, request.RetrieveSessionToken());
			return this.viewBuilder.BuildView(this.BuildModel(projectSpecifier, purgeWorkingDirectory, purgeArtifactDirectory, purgeSourceControlEnvironment));
		}

		private DeleteProjectModel BuildModel(IProjectSpecifier projectSpecifier, bool purgeWorkingDirectory, bool purgeArtifactDirectory, bool purgeSourceControlEnvironment)
		{
			return new DeleteProjectModel(projectSpecifier, string.Format("Project Deleted"), false, 
				purgeWorkingDirectory, purgeArtifactDirectory, purgeSourceControlEnvironment);
		}
	}
}
