using System;

using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.WebDashboard.ServerConnection
{
	public class CruiseServerException : CruiseControlException
	{
		private readonly string serverName;
		private readonly string url;
		private readonly Exception baseException;

		public CruiseServerException(string serverName, string url, Exception e) : base(e.Message, e)
		{
			this.baseException = e;
			this.serverName = serverName;
			this.url = url;
		}

		public override string Message
		{
			get { return this.baseException.Message; }
		}

		public string ServerName
		{
			get { return this.serverName; }
		}

		public string Url
		{
			get { return this.url; }
		}
	}
}
