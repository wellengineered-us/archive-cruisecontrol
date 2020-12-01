using WellEngineered.CruiseControl.Core;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.FarmReport
{
    public class XmlServerReportAction : IAction
    {
        public const string ACTION_NAME = "XmlServerReport";

        private readonly IFarmService farmService;

        public XmlServerReportAction(IFarmService farmService)
        {
            this.farmService = farmService;
        }

        public IResponse Execute(IRequest request)
        {
            CruiseServerSnapshotListAndExceptions allCruiseServerSnapshots = this.farmService.GetCruiseServerSnapshotListAndExceptions(null);
            CruiseServerSnapshot[] cruiseServerSnapshots = allCruiseServerSnapshots.Snapshots;

            return new XmlFragmentResponse(new CruiseXmlWriter().Write(cruiseServerSnapshots));
        }
    }
}