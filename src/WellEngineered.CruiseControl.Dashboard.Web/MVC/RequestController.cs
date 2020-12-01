using WellEngineered.CruiseControl.WebDashboard.IO;

namespace WellEngineered.CruiseControl.WebDashboard.MVC
{
    public class RequestController
    {
        private readonly IRequest request;
        private readonly IFingerprintFactory fingerprintFactory;
        private readonly IActionFactory actionFactory;

        public RequestController(IActionFactory actionFactory, IRequest request, IFingerprintFactory fingerprintFactory)
        {
            this.actionFactory = actionFactory;
            this.request = request;
            this.fingerprintFactory = fingerprintFactory;
        }

        public IResponse Do()
        {
            ConditionalGetFingerprint serverFingerprint = this.GetServerFingerprint();
            ConditionalGetFingerprint clientFingerprint = this.fingerprintFactory.BuildFromRequest(this.request);
            if (serverFingerprint.Equals(clientFingerprint))
            {
                return new NotModifiedResponse(serverFingerprint);
            }

            IAction action = this.actionFactory.Create(this.request);
            IResponse response = action.Execute(this.request);
            response.ServerFingerprint = serverFingerprint;
            return response;
        }

        private ConditionalGetFingerprint GetServerFingerprint()
        {
            IConditionalGetFingerprintProvider fingerPrintProvider = this.actionFactory.CreateFingerprintProvider(this.request);
            if (fingerPrintProvider == null)
            {
                return ConditionalGetFingerprint.NOT_AVAILABLE;
            }
            else
            {
                return fingerPrintProvider.GetFingerprint(this.request);
            }
        }
    }
}