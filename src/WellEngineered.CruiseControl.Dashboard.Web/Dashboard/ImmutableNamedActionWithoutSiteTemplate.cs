using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
    /// <summary>
    /// An immutable action that doesn't use the site template.
    /// </summary>
    public class ImmutableNamedActionWithoutSiteTemplate
        : ImmutableNamedAction, INoSiteTemplateAction
    {
        public ImmutableNamedActionWithoutSiteTemplate(string actionName, ICruiseAction action)
            : base(actionName, action)
        { }
    }
}
