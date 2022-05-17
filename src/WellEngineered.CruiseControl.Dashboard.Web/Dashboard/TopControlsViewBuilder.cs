using System.Collections;
using System.Web;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.Plugins.BuildReport;
using WellEngineered.CruiseControl.WebDashboard.Plugins.FarmReport;
using WellEngineered.CruiseControl.WebDashboard.Plugins.ProjectReport;
using WellEngineered.CruiseControl.WebDashboard.Plugins.ServerReport;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class TopControlsViewBuilder : IConditionalGetFingerprintProvider
	{
		private readonly ICruiseRequest request;
		private readonly ILinkFactory linkFactory;
		private readonly IVelocityViewGenerator velocityViewGenerator;
		private readonly IFarmService farmService;
	    private readonly IFingerprintFactory fingerprintFactory;
        private readonly ISessionRetriever sessionRetriever;

	    public TopControlsViewBuilder(ICruiseRequest request, ILinkFactory linkFactory, IVelocityViewGenerator velocityViewGenerator,
            IFarmService farmService, IFingerprintFactory fingerprintFactory, ISessionRetriever sessionRetriever)
		{
			this.request = request;
			this.linkFactory = linkFactory;
			this.velocityViewGenerator = velocityViewGenerator;
			this.farmService = farmService;
		    this.fingerprintFactory = fingerprintFactory;
            this.sessionRetriever = sessionRetriever;
		}

		private string GetCategory()
		{
			// get category from request...
			string category = this.request.Request.GetText("Category");
			
			try 
			{
				// ... or from the project status itself!
				if (string.IsNullOrEmpty(category) &&
					!string.IsNullOrEmpty(this.request.ServerName) &&
					!string.IsNullOrEmpty(this.request.ProjectName))
					category = this.farmService
	                    .GetProjectStatusListAndCaptureExceptions(this.request.ServerSpecifier, this.request.RetrieveSessionToken(this.sessionRetriever))
						.GetStatusForProject(this.request.ProjectName)
						.Category;
			}
			catch (Remote.NoSuchProjectException)
			{
				// we can get here if the user got automatically logged out from the server, thus
				// not having the right to see the requested project anymore. This yields a
				// NoSuchProjectException exception. We mask this exception and set the category
				// to "Unknown". 
				category = "Unknown";
			}

			return category;
		}



		public HtmlFragmentResponse Execute()
		{
			Hashtable velocityContext = new Hashtable();

			string serverName = this.request.ServerName;
			string categoryName = this.GetCategory();
            string projectName = this.request.ProjectName;
			string buildName = this.request.BuildName;

			velocityContext["serverName"] = serverName;
			velocityContext["categoryName"] = categoryName;
			velocityContext["projectName"] = projectName;
			velocityContext["buildName"] = buildName;

			velocityContext["farmLink"] = this.linkFactory.CreateFarmLink("Dashboard", FarmReportFarmPlugin.ACTION_NAME);

			if (serverName != string.Empty)
			{
				velocityContext["serverLink"] = this.linkFactory.CreateServerLink(this.request.ServerSpecifier, ServerReportServerPlugin.ACTION_NAME);
			}

            if (categoryName != string.Empty)
            {
                IServerSpecifier serverSpecifier;
                try
                {
                    serverSpecifier = this.request.ServerSpecifier;
                }
                catch (CruiseControlException)
                {
                    serverSpecifier = null;
                }

                if (serverSpecifier != null)
                {
                    velocityContext["categoryLink"] = new GeneralAbsoluteLink(categoryName, this.linkFactory
                        .CreateServerLink(serverSpecifier, "ViewServerReport")
                        .Url + "?Category=" + HttpUtility.UrlEncode(categoryName));
                }
                else
                {
                    velocityContext["categoryLink"] = new GeneralAbsoluteLink(categoryName, this.linkFactory
                        .CreateFarmLink( "Dashboard", FarmReportFarmPlugin.ACTION_NAME )
                        .Url + "?Category=" + HttpUtility.UrlEncode(categoryName));
                }
            }


			if (projectName != string.Empty)
			{
				velocityContext["projectLink"] = this.linkFactory.CreateProjectLink(this.request.ProjectSpecifier,  ProjectReportProjectPlugin.ACTION_NAME);
			}

			if (buildName != string.Empty)
			{
				velocityContext["buildLink"] = this.linkFactory.CreateBuildLink(this.request.BuildSpecifier,  BuildReportBuildPlugin.ACTION_NAME);
			}

			return this.velocityViewGenerator.GenerateView("TopMenu.vm", velocityContext);
		}

	    public ConditionalGetFingerprint GetFingerprint(IRequest request)
	    {
	        return this.fingerprintFactory.BuildFromFileNames();
	    }
	}
}
