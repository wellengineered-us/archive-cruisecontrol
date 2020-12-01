using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;

namespace WellEngineered.CruiseControl.WebDashboard.MVC.Cruise
{
    public class CruiseActionProxyAction : IAction, IConditionalGetFingerprintProvider
    {
        private readonly ICruiseRequestFactory cruiseRequestFactory;
        private readonly ICruiseAction proxiedAction;
        private readonly ICruiseUrlBuilder urlBuilder;
        private readonly ISessionRetriever retriever;

        public CruiseActionProxyAction(ICruiseAction proxiedAction, 
            ICruiseRequestFactory cruiseRequestFactory,
            ICruiseUrlBuilder urlBuilder,
            ISessionRetriever retriever)
        {
            this.proxiedAction = proxiedAction;
            this.cruiseRequestFactory = cruiseRequestFactory;
            this.urlBuilder = urlBuilder;
            this.retriever = retriever;
        }

        public IResponse Execute(IRequest request)
        {
            return this.proxiedAction.Execute(this.cruiseRequestFactory.CreateCruiseRequest(request, this.urlBuilder, this.retriever));
        }


        public ConditionalGetFingerprint GetFingerprint(IRequest request)
        {
            return ((IConditionalGetFingerprintProvider) this.proxiedAction).GetFingerprint(request);
        }
    }
}