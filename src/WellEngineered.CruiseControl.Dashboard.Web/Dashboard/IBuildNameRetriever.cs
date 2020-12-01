using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface IBuildNameRetriever
	{
        IBuildSpecifier GetLatestBuildSpecifier(IProjectSpecifier projectSpecifier, string sessionToken);
        IBuildSpecifier GetNextBuildSpecifier(IBuildSpecifier buildSpecifier, string sessionToken);
        IBuildSpecifier GetPreviousBuildSpecifier(IBuildSpecifier buildSpecifier, string sessionToken);
	}
}
