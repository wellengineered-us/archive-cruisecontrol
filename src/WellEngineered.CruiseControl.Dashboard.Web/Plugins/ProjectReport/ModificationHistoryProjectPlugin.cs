using System.Collections;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.ProjectReport
{
    /// <title>Modification History Project Plugin</title>
    /// <summary>
    /// This plugin shows all the modifications of a project. The newest builds are shown first.
    /// The project must use the <link>ModificationHistory Publisher</link> to get results to show up.
    /// <para>
    /// LinkDescription : View Final Project Status.
    /// </para>
    /// <example>
    /// <code title="Minimalist Example">
    /// &lt;projectPlugins&gt;
    /// &lt;modificationHistoryProjectPlugin /&gt; 
    /// &lt;/projectPlugins&gt;
    /// </code>
    /// <code title="Full Example">
    /// &lt;projectPlugins&gt;
    /// &lt;modificationHistoryProjectPlugin  onlyShowBuildsWithModifications="true"  /&gt;   
    /// &lt;/projectPlugins&gt;    
    /// </code>
    /// </example>
    /// </summary>
    /// <version>1.4.3</version>
    [ReflectorType("modificationHistoryProjectPlugin")]
    public class ModificationHistoryProjectPlugin : ICruiseAction, IPlugin
    {
        public const string ActionName = "ViewProjectModificationHistory";
        private const string XslFileName = @"xsl\ModificationHistory.xsl";
        private readonly IPhysicalApplicationPathProvider pathProvider;
        private bool onlyShowBuildsWithModifications = false;

        private readonly IFarmService farmService;
        private ITransformer transformer;

        /// <summary>
        /// Filters out builds without modifications when set to true.
        /// </summary>
        /// <default>false</default>
        /// <version>1.4.3</version>
        [ReflectorProperty("onlyShowBuildsWithModifications", Required = false)]
        public bool OnlyShowBuildsWithModifications
        {
            get { return this.onlyShowBuildsWithModifications; }
            set { this.onlyShowBuildsWithModifications = value; }
        }

        public ModificationHistoryProjectPlugin(IFarmService farmService, IPhysicalApplicationPathProvider pathProvider)
        {
            this.farmService = farmService;
            this.transformer = new XslTransformer();
            this.pathProvider = pathProvider;
        }

        public IResponse Execute(ICruiseRequest cruiseRequest)
        {
            Hashtable xsltArgs = new Hashtable();
            if (cruiseRequest.Request.ApplicationPath == "/")
                xsltArgs["applicationPath"] = string.Empty;
            else
                xsltArgs["applicationPath"] = cruiseRequest.Request.ApplicationPath;

            xsltArgs["onlyShowBuildsWithModifications"] = this.OnlyShowBuildsWithModifications;

            string HistoryDocument = this.farmService.GetModificationHistoryDocument(cruiseRequest.ProjectSpecifier, cruiseRequest.RetrieveSessionToken());
            if (HistoryDocument.Length == 0)
            {
                return new HtmlFragmentResponse("No history Data found, make sure you use the modificationHistory Publisher for this project");
            }
            else
            {
                string xslFile = this.pathProvider.GetFullPathFor(XslFileName);
                return new HtmlFragmentResponse(this.transformer.Transform(HistoryDocument, xslFile, xsltArgs));
            }

        }

        public string LinkDescription
        {
            get { return "View Modification History"; }
        }

        public INamedAction[] NamedActions
        {
            get { return new INamedAction[] { new ImmutableNamedAction(ActionName, this) }; }
        }

    }
}
