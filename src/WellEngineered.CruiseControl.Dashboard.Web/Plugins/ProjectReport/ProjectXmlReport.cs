using WellEngineered.CruiseControl.Core;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.ProjectReport
{
    public class ProjectXmlReport : ICruiseAction
    {
        public const string ActionName = "ProjectXml";
        private readonly IFarmService farmService;
        private readonly ISessionRetriever sessionRetriever;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectXmlReport"/> class.
        /// </summary>
        /// <param name="farmService">The farm service.</param>
        /// <param name="sessionRetriever">The session retriever.</param>
        public ProjectXmlReport(IFarmService farmService, ISessionRetriever sessionRetriever)
        {
            this.sessionRetriever = sessionRetriever;
            this.farmService = farmService;
        }

        /// <summary>
        /// Executes the specified cruise request.
        /// </summary>
        /// <param name="cruiseRequest">The cruise request.</param>
        /// <returns></returns>
        public IResponse Execute(ICruiseRequest cruiseRequest)
        {
            ProjectStatusListAndExceptions projectStatuses = this.farmService.GetProjectStatusListAndCaptureExceptions(cruiseRequest.ServerSpecifier,
                cruiseRequest.RetrieveSessionToken(this.sessionRetriever));
            ProjectStatus projectStatus = projectStatuses.GetStatusForProject(cruiseRequest.ProjectName);
            string xml = new CruiseXmlWriter().Write(projectStatus);
            return new XmlFragmentResponse(xml);
        }
    }
}
