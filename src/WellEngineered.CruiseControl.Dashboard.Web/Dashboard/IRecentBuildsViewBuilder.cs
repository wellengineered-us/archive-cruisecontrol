using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface IRecentBuildsViewBuilder
	{
        string BuildRecentBuildsTable(IProjectSpecifier projectSpecifier, string sessionToken);
        string BuildRecentBuildsTable(IBuildSpecifier buildSpecifier, string sessionToken);
	}
}
