using System;
using System.Collections;

using WellEngineered.CruiseControl.Core;
using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.BuildReport
{
	public class HtmlBuildLogAction : ICruiseAction, IConditionalGetFingerprintProvider
	{
		public static readonly string ACTION_NAME = "ViewBuildLog";

        public static int DisableHighlightingWhenLogExceedsKB = 0;

        private const string TEMPLATE_NAME = @"BuildLog.vm";

		private readonly IBuildRetriever buildRetriever;
		private readonly IVelocityViewGenerator viewGenerator;
		private readonly ICruiseUrlBuilder urlBuilder;
	    private readonly IFingerprintFactory fingerprintFactory;
        private readonly ISessionRetriever retriever;

	    public HtmlBuildLogAction(IBuildRetriever buildRetriever, IVelocityViewGenerator viewGenerator,
		                          ICruiseUrlBuilder urlBuilder, IFingerprintFactory fingerprintFactory,
            ISessionRetriever retriever)
		{
			this.buildRetriever = buildRetriever;
			this.viewGenerator = viewGenerator;
			this.urlBuilder = urlBuilder;
		    this.fingerprintFactory = fingerprintFactory;
            this.retriever = retriever;
		}

		public IResponse Execute(ICruiseRequest cruiseRequest)
		{
			Hashtable velocityContext = new Hashtable();
			IBuildSpecifier buildSpecifier = cruiseRequest.BuildSpecifier;
			Build build = this.buildRetriever.GetBuild(buildSpecifier, cruiseRequest.RetrieveSessionToken());
            string xmliFiedLog = build.Log.Replace("<", "&lt;").Replace(">", "&gt;");
            velocityContext["log"] = xmliFiedLog;
            velocityContext["ShowHighLight"] = xmliFiedLog.Length < ( DisableHighlightingWhenLogExceedsKB * 1024);

			// TODO - urk, this is a hack, need a better way of setting extensions
			string oldExtension = this.urlBuilder.Extension;
			this.urlBuilder.Extension = "xml";
			velocityContext["logUrl"] = this.urlBuilder.BuildBuildUrl(XmlBuildLogAction.ACTION_NAME, buildSpecifier);
			this.urlBuilder.Extension = oldExtension;

		    return this.viewGenerator.GenerateView(TEMPLATE_NAME, velocityContext);
		}

	    public ConditionalGetFingerprint GetFingerprint(IRequest request)
	    {
	        // TODO - Maybe should get date from Build type rather than LogFile?
	        ICruiseRequest cruiseRequest = new NameValueCruiseRequestFactory().CreateCruiseRequest(request, this.urlBuilder, this.retriever);
            LogFile logFile = new LogFile(cruiseRequest.BuildSpecifier.BuildName);
	        DateTime buildDate = logFile.Date;
	        ConditionalGetFingerprint logFingerprint = this.fingerprintFactory.BuildFromDate(buildDate);
	        ConditionalGetFingerprint templateFingerprint = this.fingerprintFactory.BuildFromFileNames(TEMPLATE_NAME);
	        return logFingerprint.Combine(templateFingerprint);
	    }
	}

}