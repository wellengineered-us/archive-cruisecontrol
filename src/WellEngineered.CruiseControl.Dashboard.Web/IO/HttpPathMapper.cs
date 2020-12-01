using System.IO;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

using Microsoft.AspNetCore.Http;

namespace WellEngineered.CruiseControl.WebDashboard.IO
{
	/// <summary>
	/// Maps paths by using an Http components
	/// </summary>
	public class HttpPathMapper : IPhysicalApplicationPathProvider
	{
		private readonly HttpContext context;

		public HttpPathMapper(HttpContext context)
		{
			this.context = context;
		}

	    public string GetFullPathFor(string appRelativePath)
	    {
	        return Path.Combine(this.context.Request.Path.Value, appRelativePath);
	    }
	}
}