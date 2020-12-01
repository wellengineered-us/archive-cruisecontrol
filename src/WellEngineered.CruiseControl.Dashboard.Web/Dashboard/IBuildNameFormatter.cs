using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface IBuildNameFormatter
	{
		string GetPrettyBuildName(IBuildSpecifier buildSpecifier);
		string GetCssClassForBuildLink(IBuildSpecifier buildSpecifier);
		string GetCssClassForSelectedBuildLink(IBuildSpecifier buildSpecifier);
	}
}
