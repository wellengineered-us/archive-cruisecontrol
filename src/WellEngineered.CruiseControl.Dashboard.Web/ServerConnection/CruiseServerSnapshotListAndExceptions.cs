using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.WebDashboard.ServerConnection
{
    public class CruiseServerSnapshotListAndExceptions
    {
        private readonly CruiseServerSnapshotOnServer[] snapshotAndServerList;
        private readonly CruiseServerException[] exceptions;

        public CruiseServerSnapshotListAndExceptions(CruiseServerSnapshotOnServer[] snapshotAndServerList, CruiseServerException[] exceptions)
        {
            this.snapshotAndServerList = snapshotAndServerList;
            this.exceptions = exceptions;
        }

        public CruiseServerSnapshotOnServer[] SnapshotAndServerList
        {
            get { return this.snapshotAndServerList; }
        }

        public CruiseServerException[] Exceptions
        {
            get { return this.exceptions; }
        }

        public CruiseServerSnapshot[] Snapshots
        {
            get
            {
                CruiseServerSnapshot[] snapshots = new CruiseServerSnapshot[this.snapshotAndServerList.Length];
                for (int i = 0; i < snapshots.Length; i++)
                {
                    snapshots[i] = this.snapshotAndServerList[i].CruiseServerSnapshot;
                }
                return snapshots;
            }
        }
    }
}