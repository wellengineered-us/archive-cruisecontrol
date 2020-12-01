using WellEngineered.CruiseControl.WebDashboard.IO;

using Microsoft.AspNetCore.Http;

namespace WellEngineered.CruiseControl.WebDashboard.MVC
{
	public class XmlFragmentResponse : IResponse
	{
		private readonly string xmlFragment;
	    private ConditionalGetFingerprint serverFingerprint;

	    public XmlFragmentResponse(string xmlFragment)
		{
			this.xmlFragment = xmlFragment;
		}

		public string ResponseFragment
		{
			get { return this.xmlFragment; }
		}

		public void Process(HttpResponse response)
		{
            response.Headers.Add("Last-Modified", this.serverFingerprint.LastModifiedTime.ToString("r"));
            response.Headers.Add("ETag", this.serverFingerprint.ETag);
            response.Headers.Add("Cache-Control", "private, max-age=0");
            response.ContentType = MimeType.Xml.ContentType;
			response.Write(this.xmlFragment);
		}

	    public ConditionalGetFingerprint ServerFingerprint
	    {
            get { return this.serverFingerprint; }
	        set { this.serverFingerprint = value; }
	    }
	}
}