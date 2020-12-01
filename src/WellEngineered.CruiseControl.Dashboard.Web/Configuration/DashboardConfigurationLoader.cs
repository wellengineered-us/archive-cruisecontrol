using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;

using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector;
using WellEngineered.CruiseControl.Objection;
using WellEngineered.CruiseControl.WebDashboard.IO;

namespace WellEngineered.CruiseControl.WebDashboard.Configuration
{
	// ToDo - testing
	public class DashboardConfigurationLoader : IDashboardConfiguration
	{
        private const string CONFIG_ASSEMBLY_PATTERN = "ccnet.*.plugin.dll";

		private readonly ObjectionNetReflectorInstantiator instantiator;
		private static readonly string DashboardConfigAppSettingKey = "DashboardConfigLocation";
		private static readonly string DefaultDashboardConfigLocation = "dashboard.config";
		private IRemoteServicesConfiguration remoteServicesConfiguration;
		private IPluginConfiguration pluginsConfiguration;
		private NetReflectorTypeTable typeTable;

		public DashboardConfigurationLoader(ObjectionNetReflectorInstantiator instantiator)
		{
			this.instantiator = instantiator;
			this.typeTable = this.GetTypeTable();
		}

		private void LoadRemoteServicesConfiguration()
		{
			if (this.remoteServicesConfiguration == null)
			{
				this.remoteServicesConfiguration = (IRemoteServicesConfiguration) this.Load("/dashboard/remoteServices");
			}
		}

		private void LoadPluginsConfiguration()
		{
			if (this.pluginsConfiguration == null)
			{
				this.pluginsConfiguration = (IPluginConfiguration) this.Load("/dashboard/plugins");
			}
		}

		private object Load(string xpath)
		{
			string dashboardConfig;
			using (StreamReader sr = new StreamReader(CalculateDashboardConfigPath()))
			{
				dashboardConfig = sr.ReadToEnd();
			}

			XmlNode node = XmlUtil.SelectNode(dashboardConfig, xpath);
			return NetReflector.Read(node, this.typeTable);
		}

		private NetReflectorTypeTable GetTypeTable()
		{
			NetReflectorTypeTable newTypeTable = NetReflectorTypeTable.CreateDefault(this.instantiator);

			// split the relative search path only by ';', thats also valid with Mono on Unix
            foreach (string searchPathDir in AppDomain.CurrentDomain.RelativeSearchPath.Split(';'))
            {
                newTypeTable.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, searchPathDir), CONFIG_ASSEMBLY_PATTERN);
            }
			newTypeTable.Add(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), CONFIG_ASSEMBLY_PATTERN);
			return newTypeTable;
		}

		public static string CalculateDashboardConfigPath()
		{
			string path = ConfigurationManager.AppSettings[DashboardConfigAppSettingKey];
			if (path == null || path == string.Empty)
			{
				path = DefaultDashboardConfigLocation;
			}
			if (! Path.IsPathRooted(path))
			{
				path = ProgramDataFolder.MapPath(path);
			}
			return path;
		}

		public IRemoteServicesConfiguration RemoteServices
		{
			get
			{
				this.LoadRemoteServicesConfiguration();
				return this.remoteServicesConfiguration;
			}
		}

		public IPluginConfiguration PluginConfiguration
		{
			get
			{
				this.LoadPluginsConfiguration();
				return this.pluginsConfiguration;
			}
		}
	}
}