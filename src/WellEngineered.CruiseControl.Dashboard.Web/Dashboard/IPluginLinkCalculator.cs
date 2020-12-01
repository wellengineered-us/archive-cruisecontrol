using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface IPluginLinkCalculator
	{
		IAbsoluteLink[] GetFarmPluginLinks();
		IAbsoluteLink[] GetServerPluginLinks(IServerSpecifier serverSpecifier);
		IAbsoluteLink[] GetProjectPluginLinks(IProjectSpecifier projectSpecifier);
		IAbsoluteLink[] GetBuildPluginLinks(IBuildSpecifier buildSpecifier);
	}
}
