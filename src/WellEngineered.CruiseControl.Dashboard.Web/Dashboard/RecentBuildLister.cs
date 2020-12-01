using System;
using System.Collections;

using WellEngineered.CruiseControl.Core;
using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.Plugins.BuildReport;
using WellEngineered.CruiseControl.WebDashboard.Plugins.ViewAllBuilds;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class RecentBuildLister : IRecentBuildsViewBuilder, IAllBuildsViewBuilder, IConditionalGetFingerprintProvider
	{
		private readonly IVelocityTransformer velocityTransformer;
		private readonly IVelocityViewGenerator velocityViewGenerator;
		private readonly ILinkFactory linkFactory;
		private readonly ILinkListFactory linkListFactory;
	    private readonly IFingerprintFactory fingerprintFactory;
	    private readonly IFarmService farmService;
        private readonly ICruiseUrlBuilder urlBuilder;
        private readonly ISessionRetriever retriever;

		public RecentBuildLister(IFarmService farmService, IVelocityTransformer velocityTransformer, 
			IVelocityViewGenerator viewGenerator, ILinkFactory linkFactory, ILinkListFactory linkListFactory, IFingerprintFactory fingerprintFactory,
            ICruiseUrlBuilder urlBuilder, ISessionRetriever retriever)
		{
			this.farmService = farmService;
			this.velocityTransformer = velocityTransformer;
			this.velocityViewGenerator = viewGenerator;
			this.linkFactory = linkFactory;
			this.linkListFactory = linkListFactory;
            this.urlBuilder = urlBuilder;
		    this.fingerprintFactory = fingerprintFactory;
            this.retriever = retriever;
		}

		// ToDo - use concatenatable views here, not strings
		// ToDo - something better for errors
        public string BuildRecentBuildsTable(IProjectSpecifier projectSpecifier, string sessionToken)
		{
			return BuildRecentBuildsTable(projectSpecifier, null, sessionToken);
		}

        public string BuildRecentBuildsTable(IBuildSpecifier buildSpecifier, string sessionToken)
		{
			return this.BuildRecentBuildsTable(buildSpecifier.ProjectSpecifier, buildSpecifier, sessionToken);
		}

		private string BuildRecentBuildsTable(IProjectSpecifier projectSpecifier, IBuildSpecifier buildSpecifier, string sessionToken)
		{
			Hashtable primaryContext = new Hashtable();
			Hashtable secondaryContext = new Hashtable();

			try
			{
				IBuildSpecifier[] mostRecentBuildSpecifiers = this.farmService.GetMostRecentBuildSpecifiers(projectSpecifier, 10, sessionToken);
				secondaryContext["links"] = this.linkListFactory.CreateStyledBuildLinkList(mostRecentBuildSpecifiers, buildSpecifier, BuildReportBuildPlugin.ACTION_NAME);
				primaryContext["buildRows"] = this.velocityTransformer.Transform(@"BuildRows.vm", secondaryContext);
				primaryContext["allBuildsLink"] = this.linkFactory.CreateProjectLink(projectSpecifier, string.Empty, ViewAllBuildsProjectPlugin.ACTION_NAME);

				return this.velocityTransformer.Transform(@"RecentBuilds.vm", primaryContext);
			}
			catch (Exception)
			{
				// Assume exception also caught where we care about (i.e. by action)
				return string.Empty;
			}
		}

        public HtmlFragmentResponse GenerateAllBuildsView(IProjectSpecifier projectSpecifier, string sessionToken)
		{
			Hashtable primaryContext = new Hashtable();
			Hashtable secondaryContext = new Hashtable();

			secondaryContext["links"] = this.linkListFactory.CreateStyledBuildLinkList(this.farmService.GetBuildSpecifiers(projectSpecifier, sessionToken), BuildReportBuildPlugin.ACTION_NAME);
			primaryContext["buildRows"] = this.velocityTransformer.Transform(@"BuildRows.vm", secondaryContext);
			primaryContext["allBuildsLink"] = this.linkFactory.CreateProjectLink(projectSpecifier, string.Empty, ViewAllBuildsProjectPlugin.ACTION_NAME);

			return this.velocityViewGenerator.GenerateView(@"AllBuilds.vm", primaryContext);
		}

	    public ConditionalGetFingerprint GetFingerprint(IRequest request)
	    {
	        ICruiseRequest cruiseRequest = new NameValueCruiseRequestFactory().CreateCruiseRequest(request, this.urlBuilder, this.retriever);
	        IBuildSpecifier mostRecentBuildSpecifier =
	            this.farmService.GetMostRecentBuildSpecifiers(cruiseRequest.ProjectSpecifier, 1, cruiseRequest.RetrieveSessionToken())[0];
	        DateTime mostRecentBuildDate = new LogFile(mostRecentBuildSpecifier.BuildName).Date;
	        ConditionalGetFingerprint mostRecentBuildFingerprint =
	            this.fingerprintFactory.BuildFromDate(mostRecentBuildDate);
            ConditionalGetFingerprint mostRecentTemplateFingerprint = 
                this.fingerprintFactory.BuildFromFileNames(@"BuildRows.vm", @"RecentBuilds.vm", @"AllBuilds.vm");
	        return mostRecentBuildFingerprint.Combine(mostRecentTemplateFingerprint);
	    }
	}
}
