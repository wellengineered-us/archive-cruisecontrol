using System;

using WellEngineered.CruiseControl.Objection;

using Microsoft.AspNetCore.Http;

namespace WellEngineered.CruiseControl.WebDashboard.Configuration
{
	internal class CachingDashboardConfigurationLoader : IDashboardConfiguration
	{
		private const string DashboardConfigurationKey = "DashboardConfiguration";
		private IDashboardConfiguration dashboardConfiguration;

		public CachingDashboardConfigurationLoader(ObjectSource objectSource, HttpContext context)
		{
			this.dashboardConfiguration = context.GetCache()[DashboardConfigurationKey] as IDashboardConfiguration;
			if (this.dashboardConfiguration == null)
			{
				this.dashboardConfiguration = new DashboardConfigurationLoader(new ObjectionNetReflectorInstantiator(objectSource));
				//context.GetCache().Add(DashboardConfigurationKey, dashboardConfiguration, null, DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.Normal, null);
			}
		}

		public IRemoteServicesConfiguration RemoteServices
		{
			get { return this.dashboardConfiguration.RemoteServices; }
		}

		public IPluginConfiguration PluginConfiguration
		{
			get { return this.dashboardConfiguration.PluginConfiguration; }
		}

        /// <summary>
        /// Clears the cached configuration.
        /// </summary>
        public static void ClearCache()
        {
            IDashboardConfiguration config = __Fixup.GetCurrentHttpContext().GetCache()[DashboardConfigurationKey] as IDashboardConfiguration;
            if (config != null)
            {
                __Fixup.GetCurrentHttpContext().GetCache().Remove(DashboardConfigurationKey);
            }
        }
	}
}