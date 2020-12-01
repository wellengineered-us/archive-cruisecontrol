using WellEngineered.CruiseControl.WebDashboard.IO;

using Microsoft.AspNetCore.Http;

namespace WellEngineered.CruiseControl.WebDashboard.MVC
{
	public interface IResponse
	{
		void Process(HttpResponse response);
        // TODO: Getter only needed for testing
        ConditionalGetFingerprint ServerFingerprint { get; set; }
	}
}