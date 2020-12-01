using System.IO;
using System.Xml;

using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.ForceBuild
{
	public class ForceBuildXmlAction : ICruiseAction
	{
		public const string ACTION_NAME = "ForceBuild";

		private readonly IFarmService farmService;

		public ForceBuildXmlAction(IFarmService farmService)
		{
			this.farmService = farmService;
		}

		public IResponse Execute(ICruiseRequest request)
		{
            string sessionToken = null;
            if ((request != null) && (request.Request != null)) request.Request.GetText("sessionToken");
            this.farmService.ForceBuild(request.ProjectSpecifier, sessionToken);

			StringWriter stringWriter = new StringWriter();
			XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
			xmlWriter.WriteStartElement("ForceBuildResult");
			xmlWriter.WriteString("Build Forced for " + request.ProjectName);
			xmlWriter.WriteEndElement();

			return new XmlFragmentResponse(stringWriter.ToString());
		}
	}
}