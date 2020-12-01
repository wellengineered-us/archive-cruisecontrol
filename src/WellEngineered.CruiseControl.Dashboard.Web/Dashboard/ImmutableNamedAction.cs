using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class ImmutableNamedAction : INamedAction
	{
		private readonly string actionName;
		private readonly ICruiseAction action;

		public ImmutableNamedAction(string actionName, ICruiseAction action)
		{
			this.actionName = actionName;
			this.action = action;
		}

		public string ActionName
		{
			get { return this.actionName; }
		}

		public ICruiseAction Action
		{
			get { return this.action; }
		}
	}
}