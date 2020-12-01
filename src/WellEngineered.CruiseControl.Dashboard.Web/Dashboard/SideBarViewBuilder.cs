using System.Collections;
using System.Collections.Generic;
using System.Web;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.Plugins.BuildReport;
using WellEngineered.CruiseControl.WebDashboard.Plugins.FarmReport;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
    public class SideBarViewBuilder : IConditionalGetFingerprintProvider
    {
        private readonly ICruiseRequest request;
        private readonly IBuildNameRetriever buildNameRetriever;
        private readonly IRecentBuildsViewBuilder recentBuildsViewBuilder;
        private readonly IPluginLinkCalculator pluginLinkCalculator;
        private readonly IVelocityViewGenerator velocityViewGenerator;
        private readonly ILinkListFactory linkListFactory;
        private readonly ILinkFactory linkFactory;
        private readonly IFarmService farmService;
        private readonly IFingerprintFactory fingerprintFactory;

        public SideBarViewBuilder(ICruiseRequest request, IBuildNameRetriever buildNameRetriever, IRecentBuildsViewBuilder recentBuildsViewBuilder, IPluginLinkCalculator pluginLinkCalculator, IVelocityViewGenerator velocityViewGenerator, ILinkFactory linkFactory, ILinkListFactory linkListFactory, IFarmService farmService, IFingerprintFactory fingerprintFactory)
        {
            this.request = request;
            this.buildNameRetriever = buildNameRetriever;
            this.recentBuildsViewBuilder = recentBuildsViewBuilder;
            this.pluginLinkCalculator = pluginLinkCalculator;
            this.velocityViewGenerator = velocityViewGenerator;
            this.linkListFactory = linkListFactory;
            this.linkFactory = linkFactory;
            this.farmService = farmService;
            this.fingerprintFactory = fingerprintFactory;
        }

        private IAbsoluteLink[] GetCategoryLinks(IServerSpecifier serverSpecifier)
        {
            if (serverSpecifier == null)
                return null;

            // create list of categories
            List<string> categories = new List<string>();

            foreach (ProjectStatusOnServer status in this.farmService
                .GetProjectStatusListAndCaptureExceptions(serverSpecifier, this.request.RetrieveSessionToken())
                .StatusAndServerList)
            {
                string category = status.ProjectStatus.Category;

                if (!string.IsNullOrEmpty(category) && !categories.Contains(category))
                    categories.Add(category);
            }

            // sort list if at least one element exists
            if (categories.Count == 0)
                return null;
            else
                categories.Sort();

            // use just created list to assemble wanted links
            List<GeneralAbsoluteLink> links = new List<GeneralAbsoluteLink>();
            string urlTemplate = this.linkFactory
                .CreateServerLink(serverSpecifier, "ViewServerReport")
                .Url + "?Category=";

            foreach (string category in categories)
                links.Add(new GeneralAbsoluteLink(category, urlTemplate + HttpUtility.UrlEncode(category)));

            return links.ToArray();
        }

        
        private IAbsoluteLink[] GetCategoryLinks(IServerSpecifier[] serverSpecifiers, ICruiseRequest request)
        {
            if (serverSpecifiers == null) return null;

            List<string> categories = new List<string>();

            foreach (IServerSpecifier serverSpecifier in serverSpecifiers)
            {
                System.Diagnostics.Debug.WriteLine(serverSpecifier.ServerName);

                foreach (ProjectStatusOnServer status in this.farmService
                    .GetProjectStatusListAndCaptureExceptions(serverSpecifier, request.RetrieveSessionToken())
                    .StatusAndServerList)
                {
                    string category = status.ProjectStatus.Category;
                    System.Diagnostics.Debug.WriteLine(category);


                    if (!string.IsNullOrEmpty(category) && !categories.Contains(category))
                        categories.Add(category);
                }
            }

            // sort list if at least one element exists
            if (categories.Count == 0) return null;

            categories.Sort();

            // use just created list to assemble wanted links
            List<GeneralAbsoluteLink> links = new List<GeneralAbsoluteLink>();
            string urlTemplate = this.linkFactory.CreateFarmLink("Dashboard", FarmReportFarmPlugin.ACTION_NAME).Url + "?Category=";

            foreach (string category in categories)
                links.Add(new GeneralAbsoluteLink(category, urlTemplate + HttpUtility.UrlEncode(category)));

            return links.ToArray();
        }

        public HtmlFragmentResponse Execute(ICruiseRequest request)
        {
            Hashtable velocityContext = new Hashtable();
            string velocityTemplateName;

            string serverName = request.ServerName;
            if (serverName == string.Empty)
            {
                velocityContext["links"] = this.pluginLinkCalculator.GetFarmPluginLinks();

                IServerSpecifier[] serverspecifiers = this.farmService.GetServerSpecifiers();
                velocityContext["serverlinks"] = this.linkListFactory.CreateServerLinkList(serverspecifiers, "ViewServerReport");

                IAbsoluteLink[] categoryLinks = this.GetCategoryLinks(serverspecifiers, request);
                velocityContext["showCategories"] = (categoryLinks != null) ? true : false;
                velocityContext["categorylinks"] = categoryLinks;
                velocityContext["farmLink"] = this.linkFactory.CreateFarmLink("Dashboard", FarmReportFarmPlugin.ACTION_NAME);
                velocityTemplateName = @"FarmSideBar.vm";
            }
            else
            {
                string projectName = request.ProjectName;
                if (projectName == string.Empty)
                {
                    IServerSpecifier serverSpecifier = request.ServerSpecifier;
                    velocityContext["links"] = this.pluginLinkCalculator.GetServerPluginLinks(serverSpecifier);
                    velocityContext["serverlink"] = this.linkFactory.CreateServerLink(serverSpecifier, "ViewServerReport");

                    IAbsoluteLink[] categoryLinks = new IAbsoluteLink[0];
                    try
                    {
                        categoryLinks = this.GetCategoryLinks(serverSpecifier);
                    }
                    catch
                    {
                        // Ignore any error here - this is normally because the URL is incorrect, the error will be displayed in the main content
                    }

                    velocityContext["showCategories"] = (categoryLinks != null) ? true : false;
                    velocityContext["categorylinks"] = categoryLinks;

                    velocityTemplateName = @"ServerSideBar.vm";
                }
                else
                {
                    string buildName = request.BuildName;
                    if (buildName == string.Empty)
                    {
                        IProjectSpecifier projectSpecifier = request.ProjectSpecifier;
                        velocityContext["links"] = this.pluginLinkCalculator.GetProjectPluginLinks(projectSpecifier);
                        velocityContext["recentBuildsTable"] = this.recentBuildsViewBuilder.BuildRecentBuildsTable(projectSpecifier, request.RetrieveSessionToken());
                        velocityTemplateName = @"ProjectSideBar.vm";
                    }
                    else
                    {
                        IBuildSpecifier buildSpecifier = request.BuildSpecifier;
                        velocityContext["links"] = this.pluginLinkCalculator.GetBuildPluginLinks(buildSpecifier);
                        velocityContext["recentBuildsTable"] = this.recentBuildsViewBuilder.BuildRecentBuildsTable(buildSpecifier, request.RetrieveSessionToken());
                        velocityContext["latestLink"] = this.linkFactory.CreateProjectLink(request.ProjectSpecifier, LatestBuildReportProjectPlugin.ACTION_NAME);
                        velocityContext["nextLink"] = this.linkFactory.CreateBuildLink(this.buildNameRetriever.GetNextBuildSpecifier(buildSpecifier, request.RetrieveSessionToken()), string.Empty, BuildReportBuildPlugin.ACTION_NAME);
                        velocityContext["previousLink"] = this.linkFactory.CreateBuildLink(this.buildNameRetriever.GetPreviousBuildSpecifier(buildSpecifier, request.RetrieveSessionToken()), string.Empty, BuildReportBuildPlugin.ACTION_NAME);
                        velocityTemplateName = @"BuildSideBar.vm";
                    }
                }
            }

            return this.velocityViewGenerator.GenerateView(velocityTemplateName, velocityContext);
        }

        public ConditionalGetFingerprint GetFingerprint(IRequest request)
        {
            ConditionalGetFingerprint mostRecentTemplateFingerprint =
                this.fingerprintFactory.BuildFromFileNames(@"FarmSideBar.vm", @"ServerSideBar.vm", @"ProjectSideBar.vm", @"BuildSideBar.vm");
            return ((IConditionalGetFingerprintProvider)this.recentBuildsViewBuilder).GetFingerprint(request).Combine(
                mostRecentTemplateFingerprint);
        }
    }
}
