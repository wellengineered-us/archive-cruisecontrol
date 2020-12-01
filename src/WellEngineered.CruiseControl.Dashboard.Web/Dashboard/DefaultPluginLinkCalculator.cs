using System.Collections.Generic;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.Configuration;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
    public class DefaultPluginLinkCalculator : IPluginLinkCalculator
	{
		private readonly ILinkFactory LinkFactory;
		private readonly IPluginConfiguration pluginConfiguration;

		public DefaultPluginLinkCalculator(ILinkFactory LinkFactory, IPluginConfiguration pluginConfiguration)
		{
			this.LinkFactory = LinkFactory;
			this.pluginConfiguration = pluginConfiguration;
		}

		public IAbsoluteLink[] GetBuildPluginLinks(IBuildSpecifier buildSpecifier)
		{
			var links = new List<IAbsoluteLink>();
			foreach (IBuildPlugin plugin in this.pluginConfiguration.BuildPlugins)
			{
				if (plugin.IsDisplayedForProject(buildSpecifier.ProjectSpecifier))
				{
					links.Add(this.LinkFactory.CreateBuildLink(buildSpecifier, plugin.LinkDescription, plugin.NamedActions[0].ActionName));
				}
			}
            return links.ToArray();
		}

		public IAbsoluteLink[] GetServerPluginLinks(IServerSpecifier serverSpecifier)
		{
            var links = new List<IAbsoluteLink>();
			foreach (IPlugin plugin in this.pluginConfiguration.ServerPlugins)
			{
				links.Add(this.LinkFactory.CreateServerLink(serverSpecifier, plugin.LinkDescription, plugin.NamedActions[0].ActionName));
			}
			return links.ToArray();
		}

		public IAbsoluteLink[] GetProjectPluginLinks(IProjectSpecifier projectSpecifier)
		{
            var links = new List<IAbsoluteLink>();
			foreach (IPlugin plugin in this.pluginConfiguration.ProjectPlugins)
			{
				links.Add(this.LinkFactory.CreateProjectLink(projectSpecifier, plugin.LinkDescription, plugin.NamedActions[0].ActionName));
			}
            return links.ToArray();
		}

		public IAbsoluteLink[] GetFarmPluginLinks()
		{
            var links = new List<IAbsoluteLink>();
			foreach (IPlugin plugin in this.pluginConfiguration.FarmPlugins)
			{
				links.Add(this.LinkFactory.CreateFarmLink(plugin.LinkDescription, plugin.NamedActions[0].ActionName));
			}
            return links.ToArray();
		}
	}
}
