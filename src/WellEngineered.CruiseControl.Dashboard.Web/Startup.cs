using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using WellEngineered.CruiseControl.WebDashboard.MVC.ASPNET;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace WellEngineered.CruiseControl.WebDashboard
{
	public static class __Fixup
	{
		public static HttpContext GetCurrentHttpContext()
		{
			return null;
		}

		public static IDictionary GetCache(this HttpContext ctx)
		{
			return null;
		}

		public static void BinaryWrite(this HttpResponse response, byte[] data)
		{
		}

		public static void Write(this HttpResponse response, string value)
		{
		}
		public static Stream GetOutputStream(this HttpResponse response)
		{
			return null;
		}

		public static IDictionary<string, HttpCookie> GetCookies(this HttpRequest request)
		{
			return null;
		}

		public static IList<HttpCookie> GetCookies(this HttpResponse response)
		{
			return null;
		}

		public class HttpCookie
		{
			private string value;
			private bool httpOnly;
			private DateTime expires;

			public HttpCookie(string ccnetdashboard, string cookieData)
			{
			}

			public string Value
			{
				get
				{
					return this.value;
				}
				set
				{
					value = value;
				}
			}

			public bool HttpOnly
			{
				get
				{
					return this.httpOnly;
				}
				set
				{
					this.httpOnly = value;
				}
			}

			public DateTime Expires
			{
				get
				{
					return this.expires;
				}
				set
				{
					this.expires = value;
				}
			}
		}
	}

	public class Startup
	{
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<CookiePolicyOptions>(options =>
													{
														// This lambda determines whether user consent for non-essential cookies is needed for a given request.
														options.CheckConsentNeeded = context => true;
														options.MinimumSameSitePolicy = SameSiteMode.None;
													});

			services.AddMvc().AddSessionStateTempDataProvider();
			services.AddSession();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseSession();
			app.UseMiddleware<HttpHandler>();

			app.Run(async (context) =>
					{
						await context.Response.WriteAsync("Hello World!");
					});
		}
	}
}