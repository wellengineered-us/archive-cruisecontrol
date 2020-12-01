using System;
using System.Collections;
using System.Collections.Generic;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.ServerReport
{
    /// <title>Queue Status Server Plugin</title>
    /// <version>1.4.3</version>
    /// <summary>
    /// The Queue Status Server Plugin displays the status of the queues on the specified server.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;queueStatusServerPlugin /&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// <para type="tip">
    /// This can be installed using the "Queue Status Display" package.
    /// </para>
    /// </remarks>
    [ReflectorType("queueStatusServerPlugin")]
    public class QueueStatusServerPlugin : ICruiseAction, IPlugin
	{
		private readonly IFarmService farmService;
		private readonly IVelocityViewGenerator viewGenerator;


        /// <summary>
        /// Amount in seconds to autorefresh
        /// </summary>
        /// <default>0 - no refresh</default>
        /// <version>1.7</version>
        [ReflectorProperty("refreshInterval", Required = false)]
        public Int32 RefreshInterval { get; set; }


		public QueueStatusServerPlugin(IFarmService farmService, IVelocityViewGenerator viewGenerator)
		{
			this.farmService = farmService;
			this.viewGenerator = viewGenerator;
		}

		public IResponse Execute(ICruiseRequest request)
		{
            request.Request.RefreshInterval = this.RefreshInterval;

			Hashtable velocityContext = new Hashtable();

            CruiseServerSnapshotListAndExceptions snapshot = this.farmService.GetCruiseServerSnapshotListAndExceptions(request.ServerSpecifier, request.RetrieveSessionToken());
            List<QueueSnapshot> queues = new List<QueueSnapshot>();
            for (int snapshotLoop = 0; snapshotLoop < snapshot.Snapshots.Length; snapshotLoop++)
            {
                QueueSetSnapshot queueSnapshot = snapshot.Snapshots[snapshotLoop].QueueSetSnapshot;
                for (int queueLoop = 0; queueLoop < queueSnapshot.Queues.Count; queueLoop++)
                {
                    queues.Add(queueSnapshot.Queues[queueLoop]);
                }
            }
            velocityContext["queues"] = queues.ToArray();
			
			return this.viewGenerator.GenerateView(@"ServerQueueStatus.vm", velocityContext);
		}

		public string LinkDescription
		{
			get { return "View Queue Status"; }
		}

		public INamedAction[] NamedActions
		{
			get {  return new INamedAction[] { new ImmutableNamedAction("ViewServerQueues", this) }; }
		}

	}
}
