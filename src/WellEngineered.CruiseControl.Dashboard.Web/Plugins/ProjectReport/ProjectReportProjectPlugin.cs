using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.WebDashboard.Configuration;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.Plugins.BuildReport;
using WellEngineered.CruiseControl.WebDashboard.Plugins.Statistics;
using WellEngineered.CruiseControl.WebDashboard.Resources;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.ProjectReport
{
    /// <title>Project Report Project Plugin</title>
    /// <version>1.0</version>
    /// <summary>
    /// The Project Report Project Plugin shows you summary details for a specific project. Part of these details are any <link>External
    /// Links</link> you have specified in the project configuration.
    /// A graph with the recent builds is also provided
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;projectReportProjectPlugin /&gt;
    /// </code>
    /// </example>
    [ReflectorType("projectReportProjectPlugin")]
    public class ProjectReportProjectPlugin : ICruiseAction, IPlugin
    {
        private readonly IFarmService farmService;
        private readonly IVelocityViewGenerator viewGenerator;
        private readonly ILinkFactory linkFactory;
        public static readonly string ACTION_NAME = "ViewProjectReport";
        private IBuildPlugin[] pluginNames = null;
        private readonly IRemoteServicesConfiguration configuration;
        private ICruiseUrlBuilder urlBuilder;
        
        // retrieve at most this amount of builds                             
        public static readonly Int32 AmountOfBuildsToRetrieve = 100;


        /// <summary>
        /// Amount in seconds to autorefresh
        /// </summary>
        /// <default>0 - no refresh</default>
        /// <version>1.7</version>
        [ReflectorProperty("refreshInterval", Required = false)]
        public Int32 RefreshInterval { get; set; }


        public ProjectReportProjectPlugin(IFarmService farmService, IVelocityViewGenerator viewGenerator, ILinkFactory linkFactory,
            IRemoteServicesConfiguration configuration, ICruiseUrlBuilder urlBuilder)
        {
            this.farmService = farmService;
            this.viewGenerator = viewGenerator;
            this.linkFactory = linkFactory;
            this.configuration = configuration;
            this.urlBuilder = urlBuilder;
        }

        public IResponse Execute(ICruiseRequest cruiseRequest)
        {
            cruiseRequest.Request.RefreshInterval = this.RefreshInterval;

            Hashtable velocityContext = new Hashtable();
            IProjectSpecifier projectSpecifier = cruiseRequest.ProjectSpecifier;
            IServerSpecifier serverSpecifier = this.FindServer(projectSpecifier);
            var sessionToken = cruiseRequest.RetrieveSessionToken();

            IBuildSpecifier[] buildSpecifiers = this.farmService.GetMostRecentBuildSpecifiers(projectSpecifier, 1, sessionToken);
            if (buildSpecifiers.Length == 1)
            {
                velocityContext["mostRecentBuildUrl"] = this.linkFactory.CreateProjectLink(projectSpecifier, LatestBuildReportProjectPlugin.ACTION_NAME).Url;
            }

            velocityContext["projectName"] = projectSpecifier.ProjectName;
            velocityContext["server"] = serverSpecifier;
            velocityContext["externalLinks"] = this.farmService.GetExternalLinks(projectSpecifier, sessionToken);
            velocityContext["noLogsAvailable"] = (buildSpecifiers.Length == 0);

            velocityContext["StatusMessage"] = this.ForceBuildIfNecessary(projectSpecifier, cruiseRequest.Request, sessionToken);
            ProjectStatus status = this.FindProjectStatus(projectSpecifier, cruiseRequest);
            velocityContext["status"] = status;
            velocityContext["StartStopButtonName"] = (status.Status == ProjectIntegratorState.Running) ? "StopBuild" : "StartBuild"; 
            velocityContext["StartStopButtonValue"] = (status.Status == ProjectIntegratorState.Running) ? "Stop" : "Start";
            velocityContext["ForceAbortBuildButtonName"] = (status.Activity.IsSleeping() ? "ForceBuild" : "AbortBuild");
		    velocityContext["ForceAbortBuildButtonValue"] = (status.Activity.IsSleeping() ? "Force" : "Abort");
            velocityContext["ParametersUrl"] = this.urlBuilder.BuildProjectUrl(ProjectParametersAction.ActionName, projectSpecifier);
            velocityContext["AllowForceBuild"] = (serverSpecifier.AllowForceBuild && status.ShowForceBuildButton);
            velocityContext["AllowStartStopBuild"] = (serverSpecifier.AllowStartStopBuild && status.ShowStartStopButton);


            if (cruiseRequest.Request.ApplicationPath == "/")
                velocityContext["applicationPath"] = string.Empty;
            else
                velocityContext["applicationPath"] = cruiseRequest.Request.ApplicationPath;

            velocityContext["rssDataPresent"] = this.farmService.GetRSSFeed(projectSpecifier, sessionToken).Length > 0;

            // I (willemsruben) can not figure out why the line below does not work :-(
            // velocityContext["rss"] = linkFactory.CreateProjectLink(projectSpecifier, WebDashboard.Plugins.RSS.RSSFeed.ACTION_NAME).Url;
            //
            velocityContext["rss"] = RSSLinkBuilder.CreateRSSLink(this.linkFactory, projectSpecifier);

            velocityContext["ohloh"] = this.farmService.GetLinkedSiteId(projectSpecifier, sessionToken, "ohloh") ?? string.Empty;

            string subReportData = this.GetPluginSubReport(cruiseRequest, projectSpecifier, buildSpecifiers);
            if (subReportData != null && subReportData != String.Empty)
                velocityContext["pluginInfo"] = subReportData;



            BuildGraph GraphMaker;
            // if the amount of builds exceed this, foresee extra column(s) for the days                         
            //   adjusting the Y-axis of the graph                                                               
            Int32 MaxBuildTreshhold = 15;
            // Limits the X-axis to this amount of days                                                          
            Int32 MaxAmountOfDaysToDisplay = 15;
            // the amount of columns to foresee for 1 day in the graph                                           
            Int32 DateMultiPlier;

            GraphMaker = new BuildGraph(
                this.farmService.GetMostRecentBuildSpecifiers(projectSpecifier, AmountOfBuildsToRetrieve, sessionToken),
                this.linkFactory,
                Translations.RetrieveCurrent());

            velocityContext["graphDayInfo"] = GraphMaker.GetBuildHistory(MaxAmountOfDaysToDisplay);
            velocityContext["highestAmountPerDay"] = GraphMaker.HighestAmountPerDay;

            DateMultiPlier = (GraphMaker.HighestAmountPerDay / MaxBuildTreshhold) + 1;
            velocityContext["dateMultiPlier"] = DateMultiPlier;


            Int32 okpercent = 100;
            if (GraphMaker.AmountOfOKBuilds + GraphMaker.AmountOfFailedBuilds > 0)
            {
                okpercent = 100 * GraphMaker.AmountOfOKBuilds / (GraphMaker.AmountOfOKBuilds + GraphMaker.AmountOfFailedBuilds);
            }
            velocityContext["OKPercent"] = okpercent;
            velocityContext["NOKPercent"] = 100 - okpercent;
                                           
            return this.viewGenerator.GenerateView(@"ProjectReport.vm", velocityContext);
        }

        private IServerSpecifier FindServer(IProjectSpecifier projectSpecifier)
        {
            foreach (ServerLocation server in this.configuration.Servers)
            {
                if (string.Equals(projectSpecifier.ServerSpecifier.ServerName, server.Name))
                {
                    return new DefaultServerSpecifier(server.Name, server.AllowForceBuild, server.AllowStartStopBuild);
                }
            }
            throw new CruiseControlException("Unable to find specified server");
        }

        private ProjectStatus FindProjectStatus(IProjectSpecifier projectSpecifier, ICruiseRequest request)
        {
            ProjectStatusListAndExceptions list = this.farmService.GetProjectStatusListAndCaptureExceptions(projectSpecifier.ServerSpecifier, request.RetrieveSessionToken());
            foreach (ProjectStatusOnServer status in list.StatusAndServerList)
            {
                if (string.Equals(status.ProjectStatus.Name, projectSpecifier.ProjectName, StringComparison.OrdinalIgnoreCase))
                {
                    return status.ProjectStatus;
                }
            }
            throw new CruiseControlException("Unable to retrieve project status");
        }

        private string ForceBuildIfNecessary(IProjectSpecifier projectSpecifier, IRequest request, string sessionToken)
        {
            // Retrieve the parameters
            var parameters = new Dictionary<string, string>();
            foreach (string parameterName in __Fixup.GetCurrentHttpContext().Request.Form.Keys)
            {
                if (parameterName.StartsWith("param_"))
                {
                    parameters.Add(parameterName.Substring(6), __Fixup.GetCurrentHttpContext().Request.Form[parameterName]);
                }
            }

            if (!string.IsNullOrEmpty(request.FindParameterStartingWith("StopBuild")))
            {
                this.farmService.Stop(projectSpecifier, sessionToken);
                return string.Format(System.Globalization.CultureInfo.CurrentCulture,"Stopping project {0}", projectSpecifier.ProjectName);
            }
            else if (!string.IsNullOrEmpty(request.FindParameterStartingWith("StartBuild")))
            {
                this.farmService.Start(projectSpecifier, sessionToken);
                return string.Format(System.Globalization.CultureInfo.CurrentCulture,"Starting project {0}", projectSpecifier.ProjectName);
            }
            else if (!string.IsNullOrEmpty(request.FindParameterStartingWith("ForceBuild")))
            {
                this.farmService.ForceBuild(projectSpecifier, sessionToken, parameters);
                return string.Format(System.Globalization.CultureInfo.CurrentCulture,"Build successfully forced for {0}", projectSpecifier.ProjectName);
            }
            else if (!string.IsNullOrEmpty(request.FindParameterStartingWith("AbortBuild")))
            {
                this.farmService.AbortBuild(projectSpecifier, sessionToken);
                return string.Format(System.Globalization.CultureInfo.CurrentCulture,"Abort successfully forced for {0}", projectSpecifier.ProjectName);
            }
            else
            {
                return string.Empty;
            }
        }

        public string LinkDescription
        {
            get { return "Project Report"; }
        }

        public INamedAction[] NamedActions
        {
            get { return new INamedAction[] { new ImmutableNamedAction(ACTION_NAME, this) }; }
        }

        private string GetPluginSubReport(ICruiseRequest cruiseRequest,
                                          IProjectSpecifier projectSpecifier, IBuildSpecifier[] buildSpecifiers)
        {
            if (buildSpecifiers.Length > 0 && this.pluginNames != null)
            {
                string outputResponse = String.Empty;

                ModifiedCruiseRequest req = new ModifiedCruiseRequest(cruiseRequest.Request, cruiseRequest.UrlBuilder);
                req.ReplaceBuildSpecifier(buildSpecifiers[0]);

                foreach (IBuildPlugin buildPlugIn in this.pluginNames)
                {
                    if (buildPlugIn != null && buildPlugIn.IsDisplayedForProject(projectSpecifier) &&
                        buildPlugIn.NamedActions != null)
                    {
                        foreach (INamedAction namedAction in buildPlugIn.NamedActions)
                        {
                            IResponse resp = namedAction.Action.Execute(req);

                            if (resp != null && resp is HtmlFragmentResponse)
                                outputResponse += ((HtmlFragmentResponse)resp).ResponseFragment;
                        }
                    }
                }
                return outputResponse;
            }
            return null;
        }

        private class ModifiedCruiseRequest : ICruiseRequest
        {
            private readonly IRequest request;

            private IServerSpecifier serverSpecifier = null;
            private IProjectSpecifier projectSpecifier = null;
            private IBuildSpecifier buildSpecifier = null;
            private ICruiseUrlBuilder urlBuilder;

            public ModifiedCruiseRequest(IRequest request, ICruiseUrlBuilder urlBuilder)
            {
                this.request = request;
                this.urlBuilder = urlBuilder;
            }

            public ICruiseUrlBuilder UrlBuilder
            {
				get { return this.urlBuilder; }
            }

            public string ServerName
            {
                get { return (this.serverSpecifier != null) ? this.serverSpecifier.ServerName : this.FindRESTSpecifiedResource(DefaultCruiseUrlBuilder.ServerRESTSpecifier); }
            }

            public string ProjectName
            {
                get { return (this.projectSpecifier != null) ? this.projectSpecifier.ProjectName : this.FindRESTSpecifiedResource(DefaultCruiseUrlBuilder.ProjectRESTSpecifier); }
            }

            public string BuildName
            {
                get { return (this.buildSpecifier != null) ? this.buildSpecifier.BuildName : this.FindRESTSpecifiedResource(DefaultCruiseUrlBuilder.BuildRESTSpecifier); }
            }

            private string FindRESTSpecifiedResource(string specifier)
            {
                string[] subFolders = this.request.SubFolders;

                for (int i = 0; i < subFolders.Length; i += 2)
                {
                    if (subFolders[i] == specifier)
                    {
                        if (i < subFolders.Length)
                        {
                            return HttpUtility.UrlDecode(subFolders[i + 1]);
                        }
                        else
                        {
                            throw new CruiseControlException(
                                string.Format(System.Globalization.CultureInfo.CurrentCulture,"unexpected URL format - found {0} REST Specifier, but no following value", specifier));
                        }
                    }
                }

                return string.Empty;
            }

            public IServerSpecifier ServerSpecifier
            {
                get { return (this.serverSpecifier != null) ? this.serverSpecifier : new DefaultServerSpecifier(this.ServerName); }
            }

            public IProjectSpecifier ProjectSpecifier
            {
                get { return (this.projectSpecifier != null) ? this.projectSpecifier : new DefaultProjectSpecifier(this.ServerSpecifier, this.ProjectName); }
            }

            public IBuildSpecifier BuildSpecifier
            {
                get { return (this.buildSpecifier != null) ? this.buildSpecifier : new DefaultBuildSpecifier(this.ProjectSpecifier, this.BuildName); }
            }

            public IRequest Request
            {
                get { return this.request; }
            }

            public void ReplaceBuildSpecifier(IBuildSpecifier buildSpecifier)
            {
                this.buildSpecifier = buildSpecifier;
            }

            /// <summary>
            /// Attempt to retrieve a session token
            /// </summary>
            /// <returns></returns>
            public virtual string RetrieveSessionToken()
            {
                return this.RetrieveSessionToken(null);
            }

            /// <summary>
            /// Attempt to retrieve a session token
            /// </summary>
            /// <returns></returns>
            public virtual string RetrieveSessionToken(ISessionRetriever sessionRetriever)
            {
                // Attempt to find a session token
                string sessionToken = this.request.GetText("sessionToken");
                if (string.IsNullOrEmpty(sessionToken) && (sessionRetriever != null))
                {
                    sessionToken = sessionRetriever.RetrieveSessionToken(this.request);
                }
                return sessionToken;
            }
        }
    }
}
