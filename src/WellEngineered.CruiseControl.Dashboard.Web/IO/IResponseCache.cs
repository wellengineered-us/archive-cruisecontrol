using WellEngineered.CruiseControl.WebDashboard.MVC;

namespace WellEngineered.CruiseControl.WebDashboard.IO
{
	public interface IResponseCache
	{
		IResponse Get(IRequest request);
		void Insert(IRequest request, IResponse response);
	}

}