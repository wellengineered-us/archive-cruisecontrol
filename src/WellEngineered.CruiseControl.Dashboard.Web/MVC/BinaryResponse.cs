using WellEngineered.CruiseControl.WebDashboard.IO;

using Microsoft.AspNetCore.Http;

namespace WellEngineered.CruiseControl.WebDashboard.MVC
{
    public class BinaryResponse : IResponse
    {
        private byte[] content;
        private ConditionalGetFingerprint serverFingerprint;

        public BinaryResponse(byte[] content)
        {
            this.content = content;
        }

        public void Process(HttpResponse response)
        {
            response.Headers.Add("Last-Modified", this.serverFingerprint.LastModifiedTime.ToString("r"));
	        response.Headers.Add("ETag", this.serverFingerprint.ETag);
	        response.Headers.Add("Cache-Control", "private, max-age=0");
            response.BinaryWrite(this.content);
        }

        public ConditionalGetFingerprint ServerFingerprint
        {
            get { return this.serverFingerprint; }
            set { this.serverFingerprint = value; }
        }
    }
}