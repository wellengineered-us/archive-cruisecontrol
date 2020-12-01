using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.Plugins.FarmReport;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard.Actions
{
	public class DefaultAction : IAction
	{
		private readonly ILinkFactory linkFactory;

		public DefaultAction(ILinkFactory linkFactory)
		{
			this.linkFactory = linkFactory;
		}

		public IResponse Execute(IRequest request)
		{
			return new RedirectResponse(this.linkFactory.CreateFarmLink(string.Empty, FarmReportFarmPlugin.ACTION_NAME).Url);
		}
	}
}