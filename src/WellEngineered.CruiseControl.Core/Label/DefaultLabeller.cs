using System.Globalization;
using System.Text.RegularExpressions;

using WellEngineered.CruiseControl.Core.Config;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.Label
{
    /// <summary>
    /// <para>
    /// By default, CCNet uses a plain incrementing build number as a build label. Some source controls (e.g. <link>Perforce Source Control
    /// Block</link>) require you to use a different naming scheme if you want CCNet to apply labels to source control on successful builds.
    /// </para>
    /// <para>
    /// You can do this by specifying your own configuration of the default labeller in your project.
    /// </para>
    /// </summary>
    /// <title>Default Labeller</title>
    /// <version>1.0</version>
    /// <example>
    /// <code>
    /// &lt;labeller type="defaultlabeller"&gt;
    /// &lt;initialBuildLabel&gt;1&lt;/initialBuildLabel&gt;
    /// &lt;prefix&gt;Foo-1-&lt;/prefix&gt;
    /// &lt;incrementOnFailure&gt;true&lt;/incrementOnFailure&gt;
    /// &lt;labelFormat&gt;00000&lt;/labelFormat&gt;
    /// &lt;/labeller&gt;
    /// </code>
    /// </example>
    [ReflectorType("defaultlabeller")]
    public class DefaultLabeller
        : LabellerBase
    {
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public const int INITIAL_LABEL = 1;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLabeller"/> class.
        /// </summary>
        public DefaultLabeller()
        {
            this.LabelPrefix = string.Empty;
            this.LabelPostfix = string.Empty;
            this.InitialBuildLabel = INITIAL_LABEL;
            this.IncrementOnFailed = false;
            this.LabelFormat = "0";
            this.LabelPrefixFile = string.Empty;
            this.LabelPrefixsFileSearchPattern = string.Empty;
        }
        #endregion

        /// <summary>
        /// Any string to be put in front of all labels.
        /// </summary>
        /// <version>1.0</version>
        /// <default>None</default>
        [ReflectorProperty("prefix", Required = false)]
        public string LabelPrefix { get; set; }

        /// <summary>
        /// Any string to be put at the end of all labels.
        /// </summary>
        /// <version>1.0</version>
        /// <default>None</default>
        [ReflectorProperty("postfix", Required = false)]
        public string LabelPostfix { get; set; }

        /// <summary>
        /// Allows you to set the initial build number.
        /// This will only be used when on the first build of a project, meaning that when you change this value,
        /// you'll have to stop the CCNet service and delete the state file.
        /// </summary>
        /// <version>1.5</version>
        /// <default>1</default>
        [ReflectorProperty("initialBuildLabel", Required = false)]
        public int InitialBuildLabel { get; set; }

        /// <summary>
        /// If true, the label will be incremented even if the build fails. Otherwise it will only be incremented if the build succeeds. 
        /// </summary>
        /// <version>1.1</version>
        /// <default>false</default>
        [ReflectorProperty("incrementOnFailure", Required = false)]
        public bool IncrementOnFailed { get; set; }

        /// <summary>
        /// A format applied to the buildnumber. 
        /// </summary>
        /// <version>1.0</version>
        /// <default>0</default>
        [ReflectorProperty("labelFormat", Required = false)]
        public string LabelFormat { get; set; }


        /// <summary>
        /// The location to a file which contains the prefix to be used.
        /// If this is filled in, the LabelPrefix setting is ignored, and the value from the file is used instead.
        /// This allows other people to set the prefix, without them needing to adjust CCNet.config, 
        /// or bother a person who has access. Tip : place this file in a separate folder that is shared, 
        /// so that others do not have direct access to the build server.
        /// </summary>
        /// <version>1.7</version>
        /// <default>None</default>
        [ReflectorProperty("labelPrefixFile", Required = false)]
        public string LabelPrefixFile { get; set; }


        /// <summary>
        /// A reg-ex expression to enforce the data of the contents in LabelPrefixFile. The first match is taken.
        /// This allows a bit more userfriendlyness. (skip whitespace, extra lines and so)
        /// example : \d+\.\d+\.\d+\.
        /// This will enforce a layout of 3 numbers separated by a period
        /// ideal for a version prefix
        /// </summary>
        [ReflectorProperty("labelPrefixFileSearchPattern", Required = false)]
        public string LabelPrefixsFileSearchPattern { get; set; }


        /// <summary>
        /// Generates the specified integration result.	
        /// </summary>
        /// <param name="integrationResult">The integration result.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string Generate(IIntegrationResult integrationResult)
        {
            if (!string.IsNullOrEmpty(this.LabelPrefixFile))
            {
                Log.Debug("Reading prefix from file : " + this.LabelPrefixFile);
                this.LabelPrefix = this.GetPrefixFromFile();
            }

            IntegrationSummary lastIntegration = integrationResult.LastIntegration;
            if (integrationResult == null || lastIntegration.IsInitial())
            {
                return this.LabelPrefix + this.InitialBuildLabel.ToString(this.LabelFormat, CultureInfo.CurrentCulture) + this.LabelPostfix;
            }
            else if (this.ShouldIncrementLabel(lastIntegration))
            {
                return this.LabelPrefix + this.IncrementLabel(lastIntegration.Label) + this.LabelPostfix;
            }
            else
            {
                return integrationResult.LastIntegration.Label;
            }
        }


        /// <summary>
        /// Read the contents from the prefix file, using the LabelPrefixsFileSearchPattern if specififed
        /// </summary>
        /// <returns></returns>
        private string GetPrefixFromFile()
        {
            string errorMessage;
            string prefix = null;

            if (!System.IO.File.Exists(this.LabelPrefixFile))
            {
                errorMessage = "File " + this.LabelPrefixFile + " does not exist";
                throw new ConfigurationException(errorMessage);
            }

            var fr = new System.IO.StreamReader(this.LabelPrefixFile);
            string line;

            while (!fr.EndOfStream)
            {
                line = fr.ReadLine();

                if (!string.IsNullOrEmpty(this.LabelPrefixsFileSearchPattern))
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(line, this.LabelPrefixsFileSearchPattern))
                    {
                        var m = System.Text.RegularExpressions.Regex.Match(line, this.LabelPrefixsFileSearchPattern);
                        prefix = m.Value;
                        continue;
                    }
                }
                else
                {
                    prefix = line;
                }
            }

            fr.Close();
            fr.Dispose();

            if (string.IsNullOrEmpty(prefix))
            {
                errorMessage = "No valid prefix data found in file : " + this.LabelPrefixFile;
                throw new ConfigurationException(errorMessage);
            }

            Log.Debug("Read prefix {0} from file : {1}",  prefix, this.LabelPrefixFile);
            return prefix;
        }



        private bool ShouldIncrementLabel(IntegrationSummary previousResult)
        {
            return previousResult.Status == IntegrationStatus.Success || this.IncrementOnFailed;
        }

        private string IncrementLabel(string label)
        {
            if (this.LabelPostfix.Length == 0 && this.LabelPrefix.Length > 0)
            {
                string numericLabel = Regex.Replace(label, @".*?(\d+$)", "$1");
                int newLabel = int.Parse(numericLabel, CultureInfo.CurrentCulture);
                newLabel++;
                return newLabel.ToString(this.LabelFormat, CultureInfo.CurrentCulture);

            }

            if (this.LabelPrefix.Length == 0 && this.LabelPostfix.Length >= 0)
            {
                string numericLabel = Regex.Replace(label, @"\D*?(\d{1,9}).*", "$1");
                int newLabel = int.Parse(numericLabel, CultureInfo.CurrentCulture);
                newLabel++;
                return newLabel.ToString(this.LabelFormat, CultureInfo.CurrentCulture);

            }

            // prefix and postfix are present, do some extra checking
            MatchCollection numericParts = Regex.Matches(label, @"\D*?(\d{1,9})\D*");

            if (numericParts.Count == 1)
            {
                int newLabel = int.Parse(numericParts[0].ToString(), CultureInfo.CurrentCulture);
                newLabel++;
                return newLabel.ToString(this.LabelFormat, CultureInfo.CurrentCulture);
            }


            // multiple numeric parts found, remove pre and postfix from the label
            // and scan for numeric parts
            // if only 1 part found, that should be the label
            // otherwise it is impossible to identify the label

            label = label.Replace(this.LabelPrefix, string.Empty);
            label = label.Replace(this.LabelPostfix, string.Empty);

            numericParts = Regex.Matches(label, @"\D*?(\d{1,9})\D*");

            if (numericParts.Count == 1)
            {
                int newLabel = int.Parse(numericParts[0].ToString(), CultureInfo.CurrentCulture);
                newLabel++;
                return newLabel.ToString(this.LabelFormat, CultureInfo.CurrentCulture);
            }

            throw new CruiseControlException("Unable to determine numeric part in label, pre and postfix may not contain multiple numeric parts");
        }
    }
}
