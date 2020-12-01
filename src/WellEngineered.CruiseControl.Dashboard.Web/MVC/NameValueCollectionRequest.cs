using System;
using System.Collections.Specialized;

namespace WellEngineered.CruiseControl.WebDashboard.MVC
{
	// ToDo - test!!
	public class NameValueCollectionRequest : IRequest
	{
		private readonly NameValueCollection parameters;
	    private readonly NameValueCollection headers;
	    private readonly string path;
		private readonly string rawUrl;
		private readonly string applicationPath;

		public NameValueCollectionRequest(NameValueCollection parameters, NameValueCollection headers, string path, string rawUrl, string applicationPath)
		{
			this.parameters = parameters;
		    this.headers = headers;
		    this.path = path;
			this.rawUrl = rawUrl;
			this.applicationPath = applicationPath;
		}

		public string FindParameterStartingWith(string prefix)
		{
			foreach (string key in this.parameters.Keys)
			{
				if (key.StartsWith(prefix))
				{
					return key;
				}
			}
			return string.Empty;
		}

		public string GetText(string id)
		{
			string text = this.parameters[id];
			if (text == null || text == string.Empty)
			{
				return string.Empty;
			}
			else
			{
				return text;
			}
		}

		public bool GetChecked(string id)
		{
			string value = this.GetText(id);
			return (value != null && value == "on");
		}

		public int GetInt(string id, int defaultValue)
		{
			// To Do - something more sensible
			string text = this.GetText(id);
			if (text != null && text != string.Empty)
			{
				try
				{
					return int.Parse(text);
				}
				catch (FormatException)
				{
					// Todo - exception?
					return defaultValue;
				}
			}
			else
			{
				return defaultValue;
			}
		}

	    public string RawUrl
		{
			get { return this.rawUrl; }
		}

		public string FileNameWithoutExtension
		{
			get
			{
				int lastSlashIndex = this.path.LastIndexOf('/');
				if (lastSlashIndex == -1)
					lastSlashIndex = 0;

				int lastPeriod = this.path.LastIndexOf('.');
				if (lastPeriod == -1)
					lastPeriod = this.path.Length;

				return this.path.Substring(lastSlashIndex + 1, lastPeriod - lastSlashIndex - 1);
			}
		}

		public string[] SubFolders
		{
			get
			{
				string relativePath = this.path.Substring(this.applicationPath.Length + (this.applicationPath.EndsWith("/") ? 0 : 1));
				int lastSlashIndex = relativePath.LastIndexOf('/');
				if (lastSlashIndex < 0)
				{
					return new string[0];
				}

				return relativePath.Substring(0, lastSlashIndex).Split('/');
			}
		}

		public string ApplicationPath
		{
			get { return this.applicationPath; }
		}


	    public string IfModifiedSince
	    {
            get { return this.headers["If-Modified-Since"]; }
	    }

	    public string IfNoneMatch
	    {
            get { return this.headers["If-None-Match"]; }
	    }

        int refreshInterval;
        public int RefreshInterval
        {
            get
            {
                return this.refreshInterval; 
            }
            set
            {
                this.refreshInterval = value;
            }
        }
    }
}