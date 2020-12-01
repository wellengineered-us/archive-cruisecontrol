using System.Collections;

namespace WellEngineered.CruiseControl.WebDashboard.MVC.View
{
	public class VelocityViewGeneratorWithTransformer : IVelocityViewGenerator
	{
		private readonly IVelocityTransformer velocityTransformer;

		public VelocityViewGeneratorWithTransformer(IVelocityTransformer velocityTransformer)
		{
			this.velocityTransformer = velocityTransformer;
		}

		public HtmlFragmentResponse GenerateView(string templateName, Hashtable velocityContext)
		{
			return new HtmlFragmentResponse(this.velocityTransformer.Transform(templateName, velocityContext));
		}
	}
}
