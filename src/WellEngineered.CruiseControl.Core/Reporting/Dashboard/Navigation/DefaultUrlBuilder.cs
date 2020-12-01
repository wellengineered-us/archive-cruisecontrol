using System;

namespace WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation
{
    /// <summary>
    /// 	
    /// </summary>
	public class DefaultUrlBuilder : IUrlBuilder
	{
		private string extension;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public static readonly string DEFAULT_EXTENSION = "aspx";

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultUrlBuilder" /> class.	
        /// </summary>
        /// <remarks></remarks>
		public DefaultUrlBuilder()
		{
			this.extension = DEFAULT_EXTENSION;
		}

        /// <summary>
        /// Gets or sets the extension.	
        /// </summary>
        /// <value>The extension.</value>
        /// <remarks></remarks>
		public string Extension
		{
			set { this.extension = value; }
			get { return this.extension; }
		}

        /// <summary>
        /// Builds the URL.	
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string BuildUrl(string action)
		{
			return this.BuildUrl(action, null, null);
		}

        /// <summary>
        /// Builds the URL.	
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="queryString">The query string.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string BuildUrl(string action, string queryString)
		{
			return this.BuildUrl(action, queryString, null);
		}

		/// <summary>
		/// Assumes that the path, queryString and action have been safely url encoded.
		/// Instead use a parameter collection and url builder can take care of encoding.
		/// </summary>
		public string BuildUrl(string action, string queryString, string path)
		{
			string url = string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0}{1}.{2}", this.CalculatePath(path), action, this.extension);
			if (!string.IsNullOrEmpty(queryString))
			{
				url += string.Format(System.Globalization.CultureInfo.CurrentCulture,"?{0}", queryString);
			}
			return url;
		}

		private string CalculatePath(string path)
		{
			if (path == null || (path.Trim() != null && path.Trim().Length == 0))
			{
				return string.Empty;
			}
			else
			{
				return (path.EndsWith("/") ? path : path + "/");
			}
		}
	}
}