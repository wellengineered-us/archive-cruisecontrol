using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface INamedAction
	{
		string ActionName { get; }
		ICruiseAction Action { get; }
	}
}
