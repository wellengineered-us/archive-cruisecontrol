namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface IPlugin
	{
		INamedAction[] NamedActions { get; }
		string LinkDescription { get; }
	}
}