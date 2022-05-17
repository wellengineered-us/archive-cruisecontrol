using System.Reflection;
using System.Threading.Tasks;

using WellEngineered.CruiseControl.Objection;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;

using Microsoft.AspNetCore.Http;
//using System.Web.SessionState;

namespace WellEngineered.CruiseControl.WebDashboard.MVC.ASPNET
{
	/// <summary>
	/// IRequiresSessionState used to be able to access session variables in the HttpContext.
	/// </summary>
	public class HttpHandler //: IHttpHandler, IRequiresSessionState
	{
		private const string RESOLVED_TYPE_MAP = "ResolvedTypeMap";
		private const string CruiseObjectSourceInitializerName = "CruiseObjectSourceInitializer";

		public void ProcessRequest(HttpContext context)
		{
			CruiseObjectSourceInitializer sourceSetup = (CruiseObjectSourceInitializer)(object)context.Session.GetString(CruiseObjectSourceInitializerName);
			ObjectSource objectSource = null;
			if (sourceSetup == null)
			{
				ObjectionStore objectionStore = new ObjectionStore(
					new CachingImplementationResolver(new NMockAwareImplementationResolver(), new CachedTypeMap(context.GetCache(), RESOLVED_TYPE_MAP)),
					new MaxLengthConstructorSelectionStrategy());
				sourceSetup = new CruiseObjectSourceInitializer(objectionStore);
				sourceSetup.SetupObjectSourceForFirstRequest(context);
				context.Session.SetString(CruiseObjectSourceInitializerName, sourceSetup.ToString());
			}
			objectSource = sourceSetup.UpdateObjectSourceForRequest(context);

			context.Response.Headers.Add("X-CCNet-Version",
				string.Format(System.Globalization.CultureInfo.CurrentCulture, "CruiseControl.NET/{0}", Assembly.GetExecutingAssembly().GetName().Version));
			Assembly.GetExecutingAssembly().GetName().Version.ToString();

			IResponse response = ((RequestController)objectSource.GetByType(typeof(RequestController))).Do();
			response.Process(context.Response);
		}

		/// <summary>
		/// The Handler itself does not contain any member and can thus be reused.
		/// (at least for the moment being)
		/// </summary>
		public bool IsReusable
		{
			get { return true; }
		}



		private readonly RequestDelegate next;

		public HttpHandler(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			// Do some request logic here.
			this.ProcessRequest(context);

			await this.next.Invoke(context).ConfigureAwait(false);

			// Do some response logic here.
		}
	}
}
