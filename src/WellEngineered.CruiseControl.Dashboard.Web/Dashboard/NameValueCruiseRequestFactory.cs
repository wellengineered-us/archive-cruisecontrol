using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class NameValueCruiseRequestFactory : ICruiseRequestFactory
	{
		public ICruiseRequest CreateCruiseRequest (IRequest request, 
            ICruiseUrlBuilder urlBuilder,
            ISessionRetriever retriever)
		{
			return new RequestWrappingCruiseRequest(request, urlBuilder, retriever);
		}
	}
}
