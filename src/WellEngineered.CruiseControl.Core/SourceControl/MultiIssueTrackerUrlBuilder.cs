using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.SourceControl
{
    /// <summary>
    /// This issue tracker allows a combination of the other issuetrackers.
    /// </summary>
    /// <title>Multi Issue Tracker URL Builder</title>
    /// <version>1.0</version>
    /// <example>
    /// <code>
    /// &lt;issueUrlBuilder type="multiIssueTracker"&gt;
    /// &lt;issueTrackers&gt;
    /// &lt;defaultIssueTracker&gt;
    /// &lt;url&gt;http://jira.public.thoughtworks.org/browse/CCNET-{0}&lt;/url&gt;
    /// &lt;/defaultIssueTracker&gt;
    /// &lt;regexIssueTracker&gt;
    /// &lt;find&gt;^.*(CCNET-\d*).*$&lt;/find&gt;
    /// &lt;replace&gt;http://jira.public.thoughtworks.org/browse/$1&lt;/replace&gt;
    /// &lt;/regexIssueTracker&gt;
    /// &lt;/issueTrackers&gt;
    /// &lt;/issueUrlBuilder&gt;
    /// </code>
    /// </example>
    [ReflectorType("multiIssueTracker")]
    public class MultiIssueTrackerUrlBuilder : IModificationUrlBuilder
    {

        private IModificationUrlBuilder[] _issueTrackers;

        /// <summary>
        /// The issue trackers to combine.
        /// </summary>
        /// <version>1.0</version>
        /// <default>n/a</default>
        [ReflectorProperty("issueTrackers", Required = true)]
        public IModificationUrlBuilder[] IssueTrackers
        {
            get
            {
                if (this._issueTrackers == null)
                    this._issueTrackers = new IModificationUrlBuilder[0];

                return this._issueTrackers;
            }

            set { this._issueTrackers = value; }
        }

        /// <summary>
        /// Setups the modification.	
        /// </summary>
        /// <param name="modifications">The modifications.</param>
        /// <remarks></remarks>
        public void SetupModification(Modification[] modifications)
        {            
            foreach (IModificationUrlBuilder modificationUrlBuilder in this._issueTrackers)
            {
                modificationUrlBuilder.SetupModification(modifications);          
            }            
        }

    }
}
