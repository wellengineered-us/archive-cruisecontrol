using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.WebDashboard.ServerConnection
{
	public class ProjectStatusOnServer
	{
		private readonly IServerSpecifier serverSpecifier;
		private readonly ProjectStatus projectStatus;

		public IServerSpecifier ServerSpecifier
		{
			get { return this.serverSpecifier; }
		}

		public ProjectStatus ProjectStatus
		{
			get { return this.projectStatus; }
		}

		public ProjectStatusOnServer(ProjectStatus projectStatus, IServerSpecifier serverSpecifier)
		{
			this.serverSpecifier = serverSpecifier;
			this.projectStatus = projectStatus;
		}
	}
}