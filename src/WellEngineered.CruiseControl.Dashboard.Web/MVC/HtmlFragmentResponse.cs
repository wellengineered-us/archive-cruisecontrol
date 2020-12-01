using WellEngineered.CruiseControl.WebDashboard.IO;

using Microsoft.AspNetCore.Http;

namespace WellEngineered.CruiseControl.WebDashboard.MVC
{
	public class HtmlFragmentResponse : IResponse
	{
		private readonly string htmlFragment;
	    private ConditionalGetFingerprint serverFingerprint;

	    public HtmlFragmentResponse(string htmlFragment)
		{
			this.htmlFragment = htmlFragment;
		}

		public string ResponseFragment
		{
			get { return this.htmlFragment; }
		}

		public void Process(HttpResponse response)
		{
			if (this.IsClientSideCacheable())
			{
				this.AddHeadersToEnable403Cacheing(response);
			}
            response.ContentType = MimeType.Html.ContentType;
            response.Write(this.htmlFragment);
		}

		private void AddHeadersToEnable403Cacheing(HttpResponse response)
		{
            //var browser = __Fixup.GetCurrentHttpContext().Request.Browser;
            //if (browser.IsBrowser("IE") && (browser.MajorVersion <= 6))
            //{
            //    // Turn off caching due to issues with IE 6.0
            //    //response.Cache.SetCacheability(HttpCacheability.NoCache);
            //    //response.Cache.SetNoStore();
            //}
            //else
            {
                response.Headers.Add("Last-Modified", this.serverFingerprint.LastModifiedTime.ToString("r"));
                response.Headers.Add("ETag", this.serverFingerprint.ETag);
                response.Headers.Add("Cache-Control", "no-store");
            }
		}

		private bool IsClientSideCacheable()
		{
			return !(ConditionalGetFingerprint.NOT_AVAILABLE.Equals(this.ServerFingerprint));
		}

		public ConditionalGetFingerprint ServerFingerprint
	    {
            get { return this.serverFingerprint; }
	        set { this.serverFingerprint = value; }
	    }
	}
}