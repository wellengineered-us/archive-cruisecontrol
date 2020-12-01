using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	[ReflectorType("namedAction")]
	public class ConfigurableNamedAction : INamedAction
	{
		private string actionName;
		private ICruiseAction action;

		[ReflectorProperty("name")]
		public string ActionName
		{
			get { return this.actionName; }
			set { this.actionName = value; }
		}

		[ReflectorProperty("action", InstanceTypeKey="type")]
		public ICruiseAction Action
		{
			get { return this.action; }
			set { this.action = value; }
		}
	}
}
