using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard.GenericPlugins
{
	[ReflectorType("configurablePlugin")]
	public class ConfigurablePlugin : IPlugin
	{
		private string description;
		private INamedAction[] namedActions;

		[ReflectorProperty("description")]
		public string LinkDescription
		{
			get
			{
				return this.description;
			}
			set
			{
				this.description = value;
			}
		}

        [ReflectorProperty("actions", Required = true)]
		public INamedAction[] NamedActions
		{
			get
			{
				return this.namedActions;
			}
			set
			{
				this.namedActions = value;
			}
		}
	}
}
