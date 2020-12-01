using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class DefaultLinkFactory : ILinkFactory
	{
		private readonly IUrlBuilder urlBuilder;
		private readonly ICruiseUrlBuilder cruiseUrlBuilder;
		private readonly IBuildNameFormatter buildNameFormatter;

		public DefaultLinkFactory(IUrlBuilder urlBuilder, ICruiseUrlBuilder cruiseUrlBuilder, IBuildNameFormatter buildNameFormatter)
		{
			this.urlBuilder = urlBuilder;
			this.cruiseUrlBuilder = cruiseUrlBuilder;
			this.buildNameFormatter = buildNameFormatter;
		}

		public IAbsoluteLink CreateBuildLink(IBuildSpecifier buildSpecifier, string text, string action)
		{
			return new BuildLink(this.cruiseUrlBuilder, buildSpecifier, text, action);
		}

		public IAbsoluteLink CreateBuildLink(IBuildSpecifier buildSpecifier, string action)
		{
			return new BuildLink(this.cruiseUrlBuilder, buildSpecifier, this.buildNameFormatter.GetPrettyBuildName(buildSpecifier), action);
		}

		public IAbsoluteLink CreateProjectLink(IProjectSpecifier projectSpecifier, string text, string action)
		{
			return new ProjectLink(this.cruiseUrlBuilder, projectSpecifier, text, action);
		}

		public IAbsoluteLink CreateProjectLink(IProjectSpecifier projectSpecifier, string action)
		{
			return new ProjectLink(this.cruiseUrlBuilder, projectSpecifier, projectSpecifier.ProjectName, action);
		}

		public IAbsoluteLink CreateServerLink(IServerSpecifier serverSpecifier, string text, string action)
		{
			return new ServerLink(this.cruiseUrlBuilder, serverSpecifier, text, action);
		}

		public IAbsoluteLink CreateServerLink(IServerSpecifier serverSpecifier, string action)
		{
			return new ServerLink(this.cruiseUrlBuilder, serverSpecifier, serverSpecifier.ServerName, action);
		}

		public IAbsoluteLink CreateFarmLink(string text, string action)
		{
			return new FarmLink(this.urlBuilder, text, action);
		}

		public IAbsoluteLink CreateStyledBuildLink(IBuildSpecifier specifier, string action)
		{
			IAbsoluteLink link = this.CreateBuildLink(specifier, this.buildNameFormatter.GetPrettyBuildName(specifier), action);
			link.LinkClass = this.buildNameFormatter.GetCssClassForBuildLink(specifier);
			return link;
		}

		public IAbsoluteLink CreateStyledSelectedBuildLink(IBuildSpecifier specifier, string action)
		{
			IAbsoluteLink link = this.CreateBuildLink(specifier, this.buildNameFormatter.GetPrettyBuildName(specifier), action);
			link.LinkClass = this.buildNameFormatter.GetCssClassForSelectedBuildLink(specifier);
			return link;
		}
	}
}
