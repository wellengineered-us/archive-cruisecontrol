using System;
using System.Collections.Generic;

namespace WellEngineered.CruiseControl.Remote
{
    /// <summary>
    /// A request to integrate a project (start a build).
    /// </summary>
	[Serializable]
	public class IntegrationRequest
	{
        /// <summary>
        /// A null request.
        /// </summary>
		public static IntegrationRequest NullRequest = new IntegrationRequest(BuildCondition.NoBuild, "NullRequest", null);
		private readonly BuildCondition buildCondition;
		private readonly string source;
        private readonly DateTime requestTime;
        private Dictionary<string, string> parameterValues = new Dictionary<string,string>();

        /// <summary>
        /// Initialise a new <see cref="IntegrationRequest"/>.
        /// </summary>
        /// <param name="buildCondition"></param>
        /// <param name="source"></param>
        /// <param name="userName"></param>
		public IntegrationRequest(BuildCondition buildCondition, string source, string userName)
		{
			this.buildCondition = buildCondition;
			this.source = source;
            this.requestTime = DateTime.Now;
            this.UserName = userName;
		}

        /// <summary>
        /// The build condition.
        /// </summary>
		public BuildCondition BuildCondition
		{
			get { return this.buildCondition; }
		}

        /// <summary>
        /// The source of the request.
        /// </summary>
		public string Source
		{
			get { return this.source; }
		}

        /// <summary>
        /// The user who triggered the build.
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// The build parameters to use.
        /// </summary>
        public Dictionary<string, string> BuildValues
        {
            get { return this.parameterValues; }
            set { this.parameterValues = value; }
        }

        /// <summary>
        /// The time the request was generated.
        /// </summary>
        public DateTime RequestTime
        {
            get { return this.requestTime; }
        }

        /// <summary>
        /// Checks if two <see cref="IntegrationRequest"/> instances are the same.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
		public override bool Equals(object obj)
		{
			IntegrationRequest other = obj as IntegrationRequest;
			return other != null && other.BuildCondition == this.BuildCondition && other.Source == this.Source;
		}

        /// <summary>
        /// Retrieves the hashcode for this request.
        /// </summary>
        /// <returns></returns>
		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

        /// <summary>
        /// Converts this request into a string.
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
            if (!string.IsNullOrEmpty(this.UserName))
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0} triggered a build ({1}) from {2}", this.UserName, this.BuildCondition, this.Source);
            }
            else
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture,"Build ({1}) triggered from {0}", this.Source, this.BuildCondition);
            }
		}

        /// <summary>
        /// Should the results of a failed source control exception be published?
        /// </summary>
        public bool PublishOnSourceControlException { get; set; }
	}
}