using System.Collections.Generic;
using System.Xml;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.Remote.Parameters;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.FarmReport
{
    public class XmlProjectParametersReportAction : IAction
    {
        public const string ACTION_NAME = "XmlProjectParametersReport";
        private readonly ICruiseUrlBuilder urlBuilder;
        private readonly ISessionRetriever retriever;

        private readonly IFarmService farmService;

        public XmlProjectParametersReportAction(IFarmService farmService, 
            ICruiseUrlBuilder urlBuilder,
            ISessionRetriever retriever)
        {
            this.farmService = farmService;
            this.urlBuilder = urlBuilder;
            this.retriever = retriever;
        }

        public IResponse Execute(IRequest request)
        {
            ICruiseRequest actualRequest = new RequestWrappingCruiseRequest(request, this.urlBuilder, this.retriever);
            List<ParameterBase> parameters = this.farmService.ListBuildParameters(actualRequest.ProjectSpecifier, null);

            XmlDocument document = new XmlDocument();
            XmlElement rootNode = document.CreateElement("parameters");
            document.AppendChild(rootNode);
            foreach (ParameterBase parameter in parameters)
            {
                XmlElement paramNode = document.CreateElement("parameter");
                paramNode.SetAttribute("name", parameter.Name);
                paramNode.SetAttribute("displayName", parameter.DisplayName);
                paramNode.SetAttribute("description", parameter.Description);
                paramNode.SetAttribute("defaultValue", parameter.ClientDefaultValue);
                rootNode.AppendChild(paramNode);

                if (parameter.AllowedValues != null)
                {
                    foreach (string value in parameter.AllowedValues)
                    {
                        XmlElement valueNode = document.CreateElement("value");
                        paramNode.AppendChild(valueNode);
                        valueNode.InnerText = value;
                    }
                }
            }

            return new XmlFragmentResponse(rootNode.OuterXml);
        }
    }
}
