using WellEngineered.CruiseControl.WebDashboard.MVC;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
    /// <summary>
    /// Interface for retrieving sessions.
    /// </summary>
    public interface ISessionRetriever
    {
        /// <summary>
        /// Retrieves the session token from a request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        string RetrieveSessionToken(IRequest request);

        /// <summary>
        /// Retrieves the display name from a request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        string RetrieveDisplayName(IRequest request);
    }
}
