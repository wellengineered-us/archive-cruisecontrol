using System;
using System.Globalization;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.SourceControl
{
    /// <summary>
    /// Generates a URL for ViewCVS.
    /// </summary>
    /// <title>ViewCVS URL Builder</title>
    /// <version>1.0</version>
    /// <example>
    /// <code>
    /// &lt;issueUrlBuilder type="defaultIssueTracker"&gt;
    /// &lt;url&gt;http://jira.public.thoughtworks.org/browse/CCNET-{0}&lt;/url&gt;
    /// &lt;/issueUrlBuilder&gt;
    /// </code>
    /// </example>
	[ReflectorType("viewcvs")]
	public class ViewCVSUrlBuilder : IModificationUrlBuilder
	{
		private string _url;

        /// <summary>
        /// The base URL.
        /// </summary>
        /// <version>1.0</version>
        /// <default>n/a</default>
		[ReflectorProperty("url")] 
		public string Url
		{
			get { return this._url; }
			set
			{
				this._url = value;
				if (!this._url.EndsWith("/"))
					this._url += "/";
				this._url += "{0}";
			}
		}

        /// <summary>
        /// Setups the modification.	
        /// </summary>
        /// <param name="modifications">The modifications.</param>
        /// <remarks></remarks>
		public void SetupModification(Modification[] modifications)
		{
			foreach (Modification mod in modifications)
			{
				mod.Url = String.Format(CultureInfo.CurrentCulture, this._url, mod.FolderName.Length == 0 ? mod.FileName : mod.FolderName + "/" + mod.FileName);
			}
		}
	}
}