using WellEngineered.CruiseControl.WebDashboard.IO;

using Microsoft.AspNetCore.Http;

namespace WellEngineered.CruiseControl.WebDashboard.MVC
{
    public class JsonFragmentResponse : IResponse
    {
        private readonly string jsonFragment;
        private ConditionalGetFingerprint serverFingerprint;

        public JsonFragmentResponse(string jsonFragment)
        {
            this.jsonFragment = jsonFragment;
        }

        public string ResponseFragment
        {
            get { return this.jsonFragment; }
        }

        public void Process(HttpResponse response)
        {
            response.Headers.Add("Last-Modified", this.serverFingerprint.LastModifiedTime.ToString("r"));
            response.Headers.Add("ETag", this.serverFingerprint.ETag);
            response.Headers.Add("Cache-Control", "private, max-age=0");
            response.ContentType = MimeType.Json.ContentType;
            response.Write(this.jsonFragment);
        }

        public ConditionalGetFingerprint ServerFingerprint
        {
            get { return this.serverFingerprint; }
            set { this.serverFingerprint = value; }
        }
    }
}