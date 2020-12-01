namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class GeneralAbsoluteLink : IAbsoluteLink
	{
		private readonly string text;
		private readonly string url;
		private string linkClass;

		public GeneralAbsoluteLink(string text) : this (text, string.Empty, string.Empty) { }

		public GeneralAbsoluteLink(string text, string url) : this (text, url, string.Empty) { }

		public GeneralAbsoluteLink(string text, string url, string linkClass)
		{
			this.text = text;
			this.url = url;
            this.linkClass = linkClass; 
		}
        
		public virtual string Text
		{
			get { return this.text; }
		}

		public virtual string Url
		{
			get { return this.url; }
		}

		public virtual string LinkClass
		{
			set { this.linkClass = value; }
			get { return this.linkClass; }
		}

        public override bool Equals(object obj)
        {
            GeneralAbsoluteLink other = obj as GeneralAbsoluteLink;

            if (other == null) return false;

            return other.LinkClass == this.linkClass &&
                   other.Text == this.Text &&
                   other.Url == this.Url;            
        }

        public override int GetHashCode()
        {
            return (this.LinkClass + this.Text + this.Url).GetHashCode();
        }
	}
}
