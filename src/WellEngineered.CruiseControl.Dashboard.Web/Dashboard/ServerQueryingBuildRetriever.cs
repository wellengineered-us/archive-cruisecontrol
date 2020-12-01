using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class ServerQueryingBuildRetriever : IBuildRetriever
	{
		private readonly ICruiseManagerWrapper cruiseManagerWrapper;

		public ServerQueryingBuildRetriever(ICruiseManagerWrapper cruiseManagerWrapper)
		{
			this.cruiseManagerWrapper = cruiseManagerWrapper;
		}

        public Build GetBuild(IBuildSpecifier buildSpecifier, string sessionToken)
		{
			string log = this.cruiseManagerWrapper.GetLog(buildSpecifier, sessionToken);

			return new Build(buildSpecifier, log);
		}
	}
}
