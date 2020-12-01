using WellEngineered.CruiseControl.WebDashboard.IO;

using Microsoft.AspNetCore.Http;

namespace WellEngineered.CruiseControl.WebDashboard.MVC
{
    public class NotModifiedResponse : IResponse
    {
        private ConditionalGetFingerprint serverFingerprint;

        public NotModifiedResponse(ConditionalGetFingerprint serverFingerprint)
        {
            this.serverFingerprint = serverFingerprint;
        }

        public void Process(HttpResponse response)
        {
            response.StatusCode = 304;
            response.Headers.Add("ETag", this.serverFingerprint.ETag);
            response.Headers.Add("Cache-Control", "private, max-age=0");
        }

        public ConditionalGetFingerprint ServerFingerprint
        {
            get { return this.serverFingerprint; }
            set { this.serverFingerprint = value; }
        }
    }
}