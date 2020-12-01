using System;
using System.Globalization;
using System.Text.RegularExpressions;

using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.Label
{
    /// <summary>
    /// Some source control systems (e.g., AccuRev) have a concept of a "change number", which the Last Change Labeller can use to build a
    /// label. The following configuration would prefix all labels with the string 'Foo-1-', so the build of change number 213 would be
    /// labelled 'Foo-1-213'
    /// </summary>
    /// <title>Last Change Labeller</title>
    /// <version>1.3</version>
    /// <example>
    /// <code>
    /// &lt;labeller type="lastChangeLabeller"&gt;
    /// &lt;prefix&gt;Foo-1-&lt;/prefix&gt;
    /// &lt;/labeller&gt;
    /// </code>
    /// </example>
    [ReflectorType("lastChangeLabeller")]
    public class LastChangeLabeller
        : LabellerBase
    {
        private const int INITIAL_SUFFIX_NUMBER = 1;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LastChangeLabeller"/> class.
        /// </summary>
        public LastChangeLabeller()
        {
            this.LabelPrefix = string.Empty;
            this.AllowDuplicateSubsequentLabels = true; 
        }
        #endregion

        /// <summary>
        /// The string to be prepended onto the last change number.
        /// </summary>
        /// <version>1.3</version>
        /// <default>None</default>
        [ReflectorProperty("prefix", Required = false)]
        public string LabelPrefix { get; set; }

        /// <summary>
        /// Controls whether duplicate subsequent labels are permitted or not. If true, duplicate labels are left
        /// intact. If false, the label will always be suffixed with ".n", where "n" is incremented for each
        /// successive duplication. Defaults to true.
        /// </summary>
        /// <version>1.3</version>
        /// <default>true</default>
        [ReflectorProperty("allowDuplicateSubsequentLabels", Required = false)]
        public bool AllowDuplicateSubsequentLabels { get; set; }
        
        /// <summary>
        /// Generate a label string from the last change number.
        /// If there is no valid change number (e.g. for a forced build without modifications),
        /// then the last integration label is used.
        /// </summary>
        /// <param name="resultFromThisBuild">IntegrationResult object for the current build</param>
        /// <returns>the new label</returns>
        public override string Generate(IIntegrationResult resultFromThisBuild)
        {

            int changeNumber = 0;
            if (int.TryParse(resultFromThisBuild.LastChangeNumber, out changeNumber))
            {
                Log.Debug(
                    string.Format(System.Globalization.CultureInfo.CurrentCulture,"LastChangeNumber retrieved - {0}",
                    changeNumber));
            }
            else
            {
                Log.Debug("LastChangeNumber defaulted to 0");
            }

            IntegrationSummary lastIntegration = resultFromThisBuild.LastIntegration;

            string firstSuffix = this.AllowDuplicateSubsequentLabels ? string.Empty : "." + INITIAL_SUFFIX_NUMBER.ToString(CultureInfo.CurrentCulture);

            if (changeNumber != 0)
            {
                return this.LabelPrefix + changeNumber + firstSuffix;
            }
            else if (lastIntegration.IsInitial() || lastIntegration.Label == null)
            {
                return this.LabelPrefix + "unknown" + firstSuffix;
            }
            else if (!this.AllowDuplicateSubsequentLabels)
            {
                return this.IncrementLabel(lastIntegration.Label);
            }
            else
            {
                return lastIntegration.Label;
            }
        }

        private string IncrementLabel(string label)
        {
            int current = 0;
            Match match = Regex.Match(label, @"(.*\d+)\.(\d+)$");
            if (match.Success && match.Groups.Count >= 3)
            {
                current = Int32.Parse(match.Groups[2].Value, CultureInfo.CurrentCulture);
                label = match.Groups[1].Value;
            }
            return string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0}.{1}", label, current + 1);
        }
    }
}