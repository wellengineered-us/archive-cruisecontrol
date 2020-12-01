using WellEngineered.CruiseControl.WebDashboard.IO;

using Microsoft.AspNetCore.Http;

namespace WellEngineered.CruiseControl.WebDashboard.MVC
{
	public class RedirectResponse : IResponse
	{
		private readonly string redirectUrl;

		public RedirectResponse(string redirectURL)
		{
			this.redirectUrl = redirectURL;
		}

		public string Url
		{
			get { return this.redirectUrl; }
		}

		public void Process(HttpResponse response)
		{
			response.Redirect(this.redirectUrl);
		}

	    public ConditionalGetFingerprint ServerFingerprint
	    {
            get { return ConditionalGetFingerprint.NOT_AVAILABLE; }
	        set { /* ignore attempts to fingerprint redirects */ }
	    }
	}
}