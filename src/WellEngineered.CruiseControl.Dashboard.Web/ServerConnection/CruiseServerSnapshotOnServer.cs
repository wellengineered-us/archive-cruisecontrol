using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.WebDashboard.ServerConnection
{
	public class CruiseServerSnapshotOnServer
	{
		private readonly IServerSpecifier serverSpecifier;
        private readonly CruiseServerSnapshot cruiseServerSnapshot;

        public CruiseServerSnapshotOnServer(CruiseServerSnapshot cruiseServerSnapshot, IServerSpecifier serverSpecifier)
		{
			this.serverSpecifier = serverSpecifier;
            this.cruiseServerSnapshot = cruiseServerSnapshot;
		}

		public IServerSpecifier ServerSpecifier
		{
			get { return this.serverSpecifier; }
		}

        public CruiseServerSnapshot CruiseServerSnapshot
		{
            get { return this.cruiseServerSnapshot; }
		}
	}
}