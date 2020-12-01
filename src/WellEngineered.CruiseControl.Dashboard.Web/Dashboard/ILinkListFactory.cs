using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface ILinkListFactory
	{
		IAbsoluteLink[] CreateStyledBuildLinkList(IBuildSpecifier[] buildSpecifiers, string action);
		IAbsoluteLink[] CreateStyledBuildLinkList(IBuildSpecifier[] buildSpecifiers, IBuildSpecifier selectedBuildSpecifier, string action);
		IAbsoluteLink[] CreateServerLinkList(IServerSpecifier[] serverSpecifiers, string action);
	}
}
