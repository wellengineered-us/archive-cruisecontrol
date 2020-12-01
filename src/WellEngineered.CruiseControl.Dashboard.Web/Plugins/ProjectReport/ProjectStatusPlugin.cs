using System.Collections;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.ProjectReport
{
    /// <title>Project Status Plugin</title>
    /// <version>1.0</version>
    /// <summary>
    /// Displays the status of a project.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;viewProjectStatusPlugin /&gt;
    /// </code>
    /// </example>
    [ReflectorType("viewProjectStatusPlugin")]
    public class ProjectStatusPlugin : ICruiseAction, IPlugin
    {
        private readonly IVelocityViewGenerator viewGenerator;
        private readonly IFarmService farmServer;
        private readonly ICruiseUrlBuilder urlBuilder;

        public ProjectStatusPlugin(IFarmService farmServer, IVelocityViewGenerator viewGenerator, ICruiseUrlBuilder urlBuilder)
        {
            this.farmServer = farmServer;
            this.viewGenerator = viewGenerator;
            this.urlBuilder = urlBuilder;
        }

        /// <summary>
        /// Executes the specified cruise request.
        /// </summary>
        /// <param name="cruiseRequest">The cruise request.</param>
        /// <returns></returns>
        public IResponse Execute(ICruiseRequest cruiseRequest)
        {
            var projectSpecifier = cruiseRequest.ProjectSpecifier;
            var velocityContext = new Hashtable();
            velocityContext["dataUrl"] = this.urlBuilder.BuildProjectUrl(ProjectStatusAction.ActionName, projectSpecifier) + "?view=json";
            velocityContext["projectName"] = projectSpecifier.ProjectName;
            if (cruiseRequest.Request.ApplicationPath == "/")
            {
                velocityContext["applicationPath"] = string.Empty;
            }
            else
            {
                velocityContext["applicationPath"] = cruiseRequest.Request.ApplicationPath;
            }
            return this.viewGenerator.GenerateView(@"ProjectStatusReport.vm", velocityContext);
        }

        public string LinkDescription
        {
            get { return "Project Status"; }
        }

        public INamedAction[] NamedActions
        {
            get { return new INamedAction[] { new ImmutableNamedAction("ViewProjectStatus", this) }; }
        }
    }
}
