using WellEngineered.CruiseControl.WebDashboard.IO;

namespace WellEngineered.CruiseControl.WebDashboard.MVC.Cruise
{
	public class CachingActionProxy : IAction
	{
		private readonly IAction proxiedAction;
		private readonly IResponseCache cache;

		public CachingActionProxy(IAction proxiedAction, IResponseCache cache)
		{
			this.proxiedAction = proxiedAction;
			this.cache = cache;
		}

		public IResponse Execute(IRequest request)
		{
			IResponse cachedResponse = this.cache.Get(request);
			if (cachedResponse != null)
				return cachedResponse;

			IResponse response = this.proxiedAction.Execute(request);
			this.cache.Insert(request, response);
			return response;
		}
	}
}