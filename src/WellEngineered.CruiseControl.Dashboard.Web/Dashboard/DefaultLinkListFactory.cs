using System.Collections.Generic;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
    public class DefaultLinkListFactory : ILinkListFactory
	{
		private readonly ILinkFactory linkFactory;

		public DefaultLinkListFactory(ILinkFactory linkFactory)
		{
			this.linkFactory = linkFactory;
		}

		public IAbsoluteLink[] CreateStyledBuildLinkList(IBuildSpecifier[] buildSpecifiers, string action)
		{
			return CreateStyledBuildLinkList(buildSpecifiers, null, action);
		}

		public IAbsoluteLink[] CreateServerLinkList(IServerSpecifier[] serverSpecifiers, string action)
		{
			var lstLinks = new List<IAbsoluteLink>();
			foreach (IServerSpecifier serverSpecifier in serverSpecifiers)
			{
				lstLinks.Add(this.linkFactory.CreateServerLink(serverSpecifier, action));
			}

			return lstLinks.ToArray();
		}

		public IAbsoluteLink[] CreateStyledBuildLinkList(IBuildSpecifier[] buildSpecifiers, IBuildSpecifier selectedBuildSpecifier, string action)
		{
			var displayableBuildLinkList = new List<IAbsoluteLink>();
			
			foreach (IBuildSpecifier buildSpecifier in buildSpecifiers)
			{
				if (buildSpecifier.Equals(selectedBuildSpecifier))
				{
					displayableBuildLinkList.Add(this.linkFactory.CreateStyledSelectedBuildLink(buildSpecifier, action));
				}
				else
				{
					displayableBuildLinkList.Add(this.linkFactory.CreateStyledBuildLink(buildSpecifier, action));
				}
			}

			return displayableBuildLinkList.ToArray();			
		}
	}
}
