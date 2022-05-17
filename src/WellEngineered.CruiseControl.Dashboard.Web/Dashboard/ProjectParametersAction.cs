using System.Collections;
using System.Collections.Generic;

using WellEngineered.CruiseControl.Remote.Parameters;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
    public class ProjectParametersAction
        : ICruiseAction
    {
        public const string ActionName = "ViewProjectParameters";
        private readonly IFarmService farmService;
        private readonly IVelocityViewGenerator viewGenerator;

        public ProjectParametersAction(IFarmService farmService, IVelocityViewGenerator viewGenerator)
        {
            this.farmService = farmService;
            this.viewGenerator = viewGenerator;
        }

        public IResponse Execute(ICruiseRequest cruiseRequest)
        {
            // Check for any build parameters
            List<ParameterBase> buildParameters = this.farmService.ListBuildParameters(cruiseRequest.ProjectSpecifier, cruiseRequest.RetrieveSessionToken());
            if ((buildParameters != null) && (buildParameters.Count > 0))
            {
                // Send an HTML fragment with the parameters
                Hashtable velocityContext = new Hashtable();
                velocityContext["parameters"] = buildParameters;
                velocityContext["projectName"] = cruiseRequest.ProjectName;
                velocityContext["serverName"] = cruiseRequest.ServerName;
                return this.viewGenerator.GenerateView(@"ProjectParameters.vm", velocityContext);
            }
            else
            {
                // Tell the client there are no parameters
                return new HtmlFragmentResponse("NONE");
            }
        }
    }
}
