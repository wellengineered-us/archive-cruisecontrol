

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.SourceControl.Mercurial
{
	/// <summary>
	/// Build a Mercurial URL.
	/// </summary>
	/// <title>Mercurial Issue Tracker URL Builder</title>
	/// <version>1.5</version>
	/// <example>
	/// <code>
	/// &lt;webUrlBuilder type="hgweb"&gt;
	/// &lt;url&gt;http://hg.mycompany.com/hgwebdir.cgi/myproject/&lt;/url&gt;
	/// &lt;/webUrlBuilder&gt;
	/// </code>
	/// </example>
	[ReflectorType("hgweb")]
	public class HgWebUrlBuilder : IModificationUrlBuilder
	{
		/// <summary>
		/// The base URL to use.
		/// </summary>
		/// <version>1.5</version>
		/// <default>n/a</default>
		[ReflectorProperty("url")]
		public string Url { get; set; }

		/// <summary>
		/// Setups the modification.
		/// </summary>
		/// <param name="modifications">The modifications.</param>
		/// <remarks></remarks>
		public void SetupModification(Modification[] modifications)
		{
			foreach (Modification modification in modifications)
			{
				modification.Url = this.Url + "rev/" + modification.Version;
			}
		}
	}
}
