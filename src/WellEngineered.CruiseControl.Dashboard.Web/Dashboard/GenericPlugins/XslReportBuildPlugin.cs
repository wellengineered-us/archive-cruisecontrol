using System.Collections.Generic;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.Dashboard.Actions;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard.GenericPlugins
{
    /// <summary>
    /// The Xsl Report Build Plugin shows detailed output for a specific build using a configured XSL Transform.
    /// <para>
    /// LinkDescription : no description set
    /// </para>
    /// </summary>
    /// <title>xsl Report Build Plugin</title>
    /// <example>
    /// <code>
    /// &lt;xslReportBuildPlugin description="NUnit Details" actionName="NUnitDetailsBuildReport" xslFileName="xsl\tests.xsl" /&gt;
    /// </code>
    /// </example>
    [ReflectorType("xslReportBuildPlugin")]
    public class XslReportBuildPlugin : ProjectConfigurableBuildPlugin
    {
        private readonly IActionInstantiator actionInstantiator;
        private string xslFileName = string.Empty;
        private string description = "no description set";
        private string actionName = "NoActionSet";
        private XsltParameter[] parameters = new XsltParameter[0];

        public XslReportBuildPlugin(IActionInstantiator actionInstantiator)
        {
            this.actionInstantiator = actionInstantiator;
        }

        /// <summary>
        /// Optional parameters to pass into the XSLT.
        /// </summary>
        [ReflectorProperty("parameters", Required = false)]
        public XsltParameter[] Parameters 
        {
            get { return this.parameters; }
            set { this.parameters = value; } 
        }

        // These 2 are separate due to inheritence / property monkey-ness
        /// <summary>
        /// The description for the hyperlink you would like to see in the build log panel.
        /// The <link>BuildReportBuildPlugin</link> has description : Build Report
        /// </summary>
        /// <version>1.3</version>
        /// <default>no description set</default>
        [ReflectorProperty("description")]
        public string ConfiguredLinkDescription
        {
            get { return this.description; }
            set { this.description = value; }
        }

        // See note on ConfiguredLinkDescription
        public override string LinkDescription
        {
            get { return this.description; }
        }

        /// <summary>
        /// The suffix for the hyperlink. For the <link>BuildReportBuildPlugin</link> this is : ViewBuildReport
        /// A name for the action for this instance of the plugin. This must be strictly alpha Numerical with no spaces and unique.
        /// </summary>
        /// <version>1.0</version>
        /// <default>NoActionSet</default>
        [ReflectorProperty("actionName")]
        public string ActionName
        {
            get { return this.actionName; }
            set { this.actionName = value; }
        }


        /// <summary>
        /// The xsl file to use to transform the build log. Location is relative to the web.config file.
        /// Standard installation places the xsl files in the xsl folder.
        /// </summary>
        /// <version>1.0</version>
        /// <default></default>
        [ReflectorProperty("xslFileName")]
        public string XslFileName
        {
            get { return this.xslFileName; }
            set { this.xslFileName = value; }
        }

        public override INamedAction[] NamedActions
        {
            get
            {
                XslReportBuildAction action = (XslReportBuildAction)this.actionInstantiator.InstantiateAction(typeof(XslReportBuildAction));
                action.XslFileName = this.XslFileName;
                var pars = new List<XsltParameter>();
                pars.AddRange(this.Parameters);
                action.Parameters = pars;
                return new INamedAction[] { new ImmutableNamedAction(this.actionName, action) };
            }
        }
    }
}
