using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.PrivateBuild.CSharpFormat;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.ServerReport
{
    /// <title>Server Security Configuration Server Plugin</title>
    /// <version>1.5</version>
    /// <summary>
    /// Displays the security configuration for a build server.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;serverSecurityConfigurationServerPlugin /&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// <para type="tip">
    /// This can be installed using the "Security Configuration Display" package.
    /// </para>
    /// </remarks>
    [ReflectorType("serverSecurityConfigurationServerPlugin")]
    public class ServerSecurityConfigurationServerPlugin : ICruiseAction, IPlugin
    {
        private const string ActionName = "ViewServerSecurityConfiguration";
        private readonly IFarmService farmService;
        private readonly IVelocityViewGenerator viewGenerator;
        private readonly ISessionRetriever sessionRetriever;

        public ServerSecurityConfigurationServerPlugin(IFarmService farmService, 
            IVelocityViewGenerator viewGenerator, 
            ISessionRetriever sessionRetriever)
        {
            this.farmService = farmService;
            this.viewGenerator = viewGenerator;
            this.sessionRetriever = sessionRetriever;
        }

        public IResponse Execute(ICruiseRequest request)
        {
            Hashtable velocityContext = new Hashtable();
            var links = new List<IAbsoluteLink>();
            links.Add(new ServerLink(request.UrlBuilder, request.ServerSpecifier, "Server Security Configuration", ActionName));

            ProjectStatusListAndExceptions projects = this.farmService.GetProjectStatusListAndCaptureExceptions(request.ServerSpecifier, request.RetrieveSessionToken());
            foreach (ProjectStatusOnServer projectStatusOnServer in projects.StatusAndServerList)
            {
                DefaultProjectSpecifier projectSpecifier = new DefaultProjectSpecifier(projectStatusOnServer.ServerSpecifier, projectStatusOnServer.ProjectStatus.Name);
                links.Add(new ProjectLink(request.UrlBuilder, projectSpecifier, projectSpecifier.ProjectName, ServerSecurityConfigurationServerPlugin.ActionName));
            }
            velocityContext["projectLinks"] = links;
            string sessionToken = request.RetrieveSessionToken(this.sessionRetriever);
            string securityConfig = this.farmService.GetServerSecurity(request.ServerSpecifier, sessionToken);
			if (!string.IsNullOrEmpty(securityConfig))
			{
            	XmlDocument document = new XmlDocument();
            	document.LoadXml(securityConfig);
	            if (string.IsNullOrEmpty(request.ProjectName))
	            {
	                securityConfig = document.SelectSingleNode("/security/manager").OuterXml;
	            }
	            else
	            {
	                velocityContext["currentProject"] = request.ProjectSpecifier.ProjectName;
	                string xpath = string.Format(System.Globalization.CultureInfo.CurrentCulture,"/security/projects/projectSecurity[name='{0}']/authorisation", request.ProjectSpecifier.ProjectName);
	                securityConfig = document.SelectSingleNode(xpath).OuterXml;
	            }
	            string xmlData = this.FormatXml(securityConfig);
	            velocityContext["log"] = xmlData;
			}
			else
			{
				velocityContext["log"] = "No security configuration could be retrieved from the server";
			}
            return this.viewGenerator.GenerateView(@"SecurityConfiguration.vm", velocityContext);
        }

        public string LinkDescription
        {
            get { return "View Security Configuration"; }
        }

        public INamedAction[] NamedActions
        {
            get { return new INamedAction[] { new ImmutableNamedAction(ActionName, this) }; }
        }

        private string FormatXml(string projectXml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(projectXml);
            StringWriter buffer = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(buffer);
            writer.Formatting = Formatting.Indented;
            document.WriteTo(writer);
            HtmlFormat formatter = new HtmlFormat();
            formatter.LineNumbers = false;
            formatter.Alternate = false;
            formatter.EmbedStyleSheet = false;
            return formatter.FormatCode(buffer.ToString());
        }
    }
}
