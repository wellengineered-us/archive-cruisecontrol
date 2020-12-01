using System.Collections;
using System.IO;
using System.Web;
using System.Xml;

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
    /// <title>View Configuration Project Plugin</title>
    /// <version>1.3.0</version>
    /// <summary>
    /// The View Configuration Project Plugin shows the configuration for a project.
    /// <para>
    /// LinkDescription : Project Configuration.
    /// </para>
    /// </summary>
    /// <example>
    /// <code title="Minimalist Example">
    /// &lt;viewConfigurationProjectPlugin /&gt;
    /// </code>
    /// <code title="Full Example">
    /// &lt;viewConfigurationProjectPlugin hidePasswords="false" /&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// <para type="tip">
    /// This can be installed using the "Project Configuration Display" package.
    /// </para>
    /// </remarks>
    [ReflectorType("viewConfigurationProjectPlugin")]
    public class ViewConfigurationProjectPlugin : ICruiseAction, IPlugin
    {
        private readonly ICruiseManagerWrapper cruiseManager;
        private readonly IVelocityViewGenerator viewGenerator;

        private bool hidePasswords = true;

        /// <summary>
        /// Whether to hide the passwords.
        /// </summary>
        /// <version>1.4.0</version>
        /// <default>true</default>
        /// <remarks>
        /// From version 1.4.0 the passwords are masked by default. Use this setting if you need to see the passwords.
        /// </remarks>
        [ReflectorProperty("hidePasswords", Required = false)]
        public bool HidePasswords
        {
            get { return this.hidePasswords; }
            set { this.hidePasswords = value; }
        }

        public ViewConfigurationProjectPlugin(ICruiseManagerWrapper cruiseManager, IVelocityViewGenerator viewGenerator)
        {
            this.cruiseManager = cruiseManager;
            this.viewGenerator = viewGenerator;
        }

        public IResponse Execute(ICruiseRequest cruiseRequest)
        {
            IProjectSpecifier projectSpecifier = cruiseRequest.ProjectSpecifier;
            string projectXml = this.cruiseManager.GetProject(projectSpecifier, cruiseRequest.RetrieveSessionToken());
            //return new HtmlFragmentResponse("<pre><code>" + HttpUtility.HtmlEncode(FormatXml(projectXml)) + "</code></pre>");
            var xmlContext = new Hashtable();
            xmlContext["xml"] = HttpUtility.HtmlEncode(this.FormatXml(projectXml));
            return this.viewGenerator.GenerateView(@"ProjectConfiguration.vm", xmlContext);
        }

        private string FormatXml(string projectXml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(projectXml);
            StringWriter buffer = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(buffer);
            writer.Formatting = Formatting.Indented;
            document.WriteTo(writer);

            string Result;
            if (this.hidePasswords)
            {
                Result = this.SecureProjectView(buffer.ToString());
            }
            else
            {
                Result = buffer.ToString();
            }


            return Result;
        }

        public string LinkDescription
        {
            get { return "Project Configuration"; }
        }

        public INamedAction[] NamedActions
        {
            get { return new INamedAction[] { new ImmutableNamedAction("ViewProjectConfiguration", this) }; }
        }


        private string SecureProjectView(string project)
        {
            const string PasswordReplacement = "*****";

            System.IO.StringReader projectReader = new StringReader(project);
            string projectLine = projectReader.ReadLine();

            System.IO.StringWriter result = new StringWriter();
            string replacedPassword=string.Empty;

            int startPos;
            int endPos;


            while (!(projectLine == null))
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(projectLine,"password", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    endPos = projectLine.IndexOf("</");

                    if (endPos > 0)
                    {
                        //structure : <password>somevalue</password>

                        startPos = projectLine.IndexOf(">");
                        replacedPassword = projectLine.Substring(0,startPos+1);
                        replacedPassword += PasswordReplacement;
                        replacedPassword += projectLine.Substring(endPos);
                    }
                    else
                    {
                        //structure : <password />
                        replacedPassword = projectLine.Replace(" /",string.Empty);
                        string temp = replacedPassword.Trim();

                        replacedPassword += PasswordReplacement;
                        replacedPassword += temp.Insert(1,"/");
                    }

                    result.WriteLine(replacedPassword);
                }
                else
                {
                    result.WriteLine(projectLine);
                }

                projectLine = projectReader.ReadLine();
            }

            return result.ToString();
        }
    }
}
