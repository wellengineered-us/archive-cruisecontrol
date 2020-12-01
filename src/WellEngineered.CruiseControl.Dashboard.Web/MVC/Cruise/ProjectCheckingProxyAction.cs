using WellEngineered.CruiseControl.WebDashboard.IO;

namespace WellEngineered.CruiseControl.WebDashboard.MVC.Cruise
{
	public class ProjectCheckingProxyAction : ICruiseAction
	{
		private readonly IErrorViewBuilder errorViewBuilder;
		private readonly ICruiseAction proxiedAction;

		public ProjectCheckingProxyAction(ICruiseAction proxiedAction, IErrorViewBuilder errorViewBuilder)
		{
			this.proxiedAction = proxiedAction;
			this.errorViewBuilder = errorViewBuilder;
		}

		public IResponse Execute(ICruiseRequest cruiseRequest)
		{
			if (cruiseRequest.ProjectName == string.Empty)
			{
				return this.errorViewBuilder.BuildView(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Error - Action [{0}] expects Project to be specified in request", this.proxiedAction.GetType().FullName));
			}
			else
			{
				return this.proxiedAction.Execute(cruiseRequest);
			}
		}
	}
}
