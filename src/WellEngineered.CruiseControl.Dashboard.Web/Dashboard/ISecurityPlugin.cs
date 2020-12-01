using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
    public interface ISecurityPlugin
        : IPlugin
    {
        bool IsAllowedForServer(IServerSpecifier serviceSpecifier);
        ISessionStorer SessionStorer { get; set; }
    }
}
