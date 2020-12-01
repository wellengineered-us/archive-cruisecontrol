using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface IBuildPlugin : IPlugin
	{
		bool IsDisplayedForProject(IProjectSpecifier project);
	}
}
