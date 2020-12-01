using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class FarmLink : GeneralAbsoluteLink
	{
		private readonly string action;
		private readonly IUrlBuilder urlBuilder;
		public readonly string absoluteUrl;

		public FarmLink(IUrlBuilder urlBuilder, string text, string action)
			: base (text)
		{
			this.urlBuilder = urlBuilder;
			this.action = action;
		}

		public override string Url
		{
			get { return this.urlBuilder.BuildUrl(this.action); }
		}
	}
}
