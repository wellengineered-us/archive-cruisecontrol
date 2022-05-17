using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.RSS
{
    /// <summary>
    /// This publisher generates an RSS feed reporting the builds for a project.
    /// The project must use the RSS Publisher to produce the data for the feed.
    ///
    /// This is available from build 1.3.0.3011 onwards. The RSS icon will be displayed on the project page,
    /// whenever a build is done with the RSS Publisher.So there is no configuration to be done in the webdashboard for this.
    ///
    /// Should you want to remove the RSS icon from the project page, remove the RSS publisher from the project,
    /// and delete the RssData.xml file from the artifact folder.
    /// </summary>
    public class RSSFeed : ICruiseAction
    {
        public const string ACTION_NAME = "RSSFeed";
        private readonly IFarmService farmService;

        public RSSFeed(IFarmService farmService)
        {
            this.farmService = farmService;
        }

        public IResponse Execute(ICruiseRequest request)
        {
			var sessionToken = request.RetrieveSessionToken();

            return new XmlFragmentResponse(this.farmService.GetRSSFeed(request.ProjectSpecifier, sessionToken));
        }
    }
}
