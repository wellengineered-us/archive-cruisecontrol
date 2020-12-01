using WellEngineered.CruiseControl.WebDashboard.IO;

namespace WellEngineered.CruiseControl.WebDashboard.MVC.Cruise
{
	public class BuildCheckingProxyAction : ICruiseAction
	{
		private readonly IErrorViewBuilder errorViewBuilder;
		private readonly ICruiseAction proxiedAction;

		public BuildCheckingProxyAction(ICruiseAction proxiedAction, IErrorViewBuilder errorViewBuilder)
		{
			this.proxiedAction = proxiedAction;
			this.errorViewBuilder = errorViewBuilder;
		}

		public IResponse Execute(ICruiseRequest cruiseRequest)
		{
			if (cruiseRequest.BuildName == string.Empty)
			{
				return this.errorViewBuilder.BuildView(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Error - Action [{0}] expects Build Name to be specified in request", this.proxiedAction.GetType().FullName));
			}
			else
			{
				return this.proxiedAction.Execute(cruiseRequest);
			}
		}
	}
}
