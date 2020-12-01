using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
    /// <summary>
    /// Provides a session store.
    /// </summary>
    public interface ISessionStore
    {
        ISessionStorer RetrieveStorer();
        ISessionRetriever RetrieveRetriever();
    }
}
