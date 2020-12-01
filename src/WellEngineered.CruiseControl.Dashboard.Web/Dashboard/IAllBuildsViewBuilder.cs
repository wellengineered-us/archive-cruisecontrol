using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.MVC;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface IAllBuildsViewBuilder
	{
        HtmlFragmentResponse GenerateAllBuildsView(IProjectSpecifier projectSpecifier, string sessionToken);	
	}
}
