namespace WellEngineered.CruiseControl.WebDashboard.MVC.Cruise
{
	public class NoCacheabilityActionProxy : IAction
	{
		private readonly IAction proxiedAction;

		public NoCacheabilityActionProxy(IAction proxiedAction)
		{
			this.proxiedAction = proxiedAction;
		}

		public IResponse Execute(IRequest request)
		{
			//__Fixup.GetCurrentHttpContext().Response.Cache.SetNoStore();
			return this.proxiedAction.Execute(request);
		}
	}
}