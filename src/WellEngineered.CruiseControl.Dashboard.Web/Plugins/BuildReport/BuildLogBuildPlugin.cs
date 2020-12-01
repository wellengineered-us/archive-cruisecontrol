using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.Dashboard.GenericPlugins;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.BuildReport
{
    /// <summary>
    /// Shows the entire build log. No parsing has been done, 
    /// so it is the same as looking at the buildlog file from the artifact folder.
    /// <para>
    /// LinkDescription : View Build Log
    /// </para>
    /// </summary>
    /// <title>View Build Log Plugin</title>
    /// <version>1.0</version>
    [ReflectorType("buildLogBuildPlugin")]
    public class BuildLogBuildPlugin : ProjectConfigurableBuildPlugin
    {
        private readonly IActionInstantiator actionInstantiator;
        private int disableHighlightingWhenLogExceedsKB=50; 


        public BuildLogBuildPlugin(IActionInstantiator actionInstantiator)
        {
            this.actionInstantiator = actionInstantiator;
        }

        public override string LinkDescription
        {
            get { return "View Build Log"; }
        }


        [ReflectorProperty("disableHighlightingWhenLogExceedsKB", Required = false)]
        public int DisableHighlightingWhenLogExceedsKB
        {
            get { return this.disableHighlightingWhenLogExceedsKB; }
            set { this.disableHighlightingWhenLogExceedsKB = value; }
        }

        public override INamedAction[] NamedActions
        {
            get
            {
                HtmlBuildLogAction.DisableHighlightingWhenLogExceedsKB = this.disableHighlightingWhenLogExceedsKB;
                return new INamedAction[]
					{
						new ImmutableNamedAction(HtmlBuildLogAction.ACTION_NAME, this.actionInstantiator.InstantiateAction(typeof (HtmlBuildLogAction)))
// We don't define this here right now since we need a way to define decorators
// See CruiseObjectSourceInitializer for linked ToDo
//					new TypedAction(XmlBuildLogAction.ACTION_NAME, typeof(XmlBuildLogAction)), 
					};
            }
        }
    }
}