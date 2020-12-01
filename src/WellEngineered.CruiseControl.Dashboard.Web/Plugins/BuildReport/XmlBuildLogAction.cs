using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.BuildReport
{
	public class XmlBuildLogAction : ICruiseAction
	{
		public static readonly string ACTION_NAME = "XmlBuildLog";

		private readonly IBuildRetriever buildRetriever;

		public XmlBuildLogAction(IBuildRetriever buildRetriever)
		{
			this.buildRetriever = buildRetriever;
		}

		public IResponse Execute(ICruiseRequest cruiseRequest)
		{
			return new XmlFragmentResponse(this.buildRetriever.GetBuild(cruiseRequest.BuildSpecifier, cruiseRequest.RetrieveSessionToken()).Log);
		}
	}
}