using System;
using System.Globalization;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.SourceControl
{
    /// <summary>
    /// Generates a URL for WebSVN.
    /// </summary>
    /// <title>WebSVN URL Builder</title>
    /// <version>1.4</version>
    /// <example>
    /// <code>
    /// &lt;issueUrlBuilder type="websvn"&gt;
    /// &lt;url&gt;http://jira.public.thoughtworks.org/browse/CCNET-{0}&lt;/url&gt;
    /// &lt;/issueUrlBuilder&gt;
    /// </code>
    /// </example>
    /// <key name="type">
    /// <description>The type of URL builder.</description>
    /// <value>websvn</value>
    /// </key>
	[ReflectorType("websvn")]
	public class WebSVNUrlBuilder : IModificationUrlBuilder
	{
		private string _url;

        /// <summary>
        /// The base URL.
        /// </summary>
        /// <version>1.4</version>
        /// <default>n/a</default>
		[ReflectorProperty("url")] 
		public string Url 
		{
			get { return this._url; }
			set { this._url = value; }
		}

        /// <summary>
        /// Setups the modification.	
        /// </summary>
        /// <param name="modifications">The modifications.</param>
        /// <remarks></remarks>
		public void SetupModification(Modification[] modifications) 
		{
			foreach( Modification mod in modifications ) 
			{
				mod.Url = String.Format( CultureInfo.CurrentCulture, this._url, mod.FolderName + "/" + mod.FileName, mod.ChangeNumber );
			}
		}
	}
}
