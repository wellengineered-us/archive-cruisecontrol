using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.MVC;

namespace WellEngineered.CruiseControl.WebDashboard.IO
{
    public interface ICruiseRequest
    {
        string ServerName { get; }
        string ProjectName { get; }
        string BuildName { get; }

        IServerSpecifier ServerSpecifier { get; }
        IProjectSpecifier ProjectSpecifier { get; }
        IBuildSpecifier BuildSpecifier { get; }

        IRequest Request { get; }
        ICruiseUrlBuilder UrlBuilder { get; }

        /// <summary>
        /// Attempt to retrieve a session token
        /// </summary>
        /// <returns></returns>
        string RetrieveSessionToken();

        /// <summary>
        /// Attempt to retrieve a session token
        /// </summary>
        /// <returns></returns>
        string RetrieveSessionToken(ISessionRetriever sessionRetriever);
    }
}