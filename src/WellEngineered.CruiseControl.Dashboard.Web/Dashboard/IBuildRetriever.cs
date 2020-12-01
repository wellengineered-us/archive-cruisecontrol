using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface IBuildRetriever
	{
        Build GetBuild(IBuildSpecifier buildSpecifier, string sessionToken);
	}
}
