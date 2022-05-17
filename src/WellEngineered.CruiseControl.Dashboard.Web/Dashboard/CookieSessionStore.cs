using System;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.MVC;

using Microsoft.AspNetCore.Http;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	/// <summary>
	/// Use a cookie as the store for the session token.
	/// </summary>
	[ReflectorType("cookieStore")]
	public class CookieSessionStore
		: ISessionStore, ISessionRetriever, ISessionStorer
	{
		#region Public methods
		#region RetrieveStorer()
		/// <summary>
		/// Retrieve the session storer.
		/// </summary>
		/// <returns>Returns an object that will store a session token.</returns>
		public ISessionStorer RetrieveStorer()
		{
			return this;
		}
		#endregion

		#region RetrieveRetriever()
		/// <summary>
		/// Retrieve the session retriever.
		/// </summary>
		/// <returns>Returns an object that will retrieve a session token.</returns>
		public ISessionRetriever RetrieveRetriever()
		{
			return this;
		}
		#endregion

		#region SessionToken
		/// <summary>
		/// Stores the session token in a cookie or deletes the cookie.
		/// </summary>
		public void StoreSessionToken(string sessionToken)
		{
			__Fixup.GetCurrentHttpContext().Session.SetString("CCNetSessionToken", sessionToken);
		}
		#endregion

		#region RetrieveSessionToken()
		/// <summary>
		/// Retrieve the session token.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public string RetrieveSessionToken(IRequest request)
		{
			return (String)__Fixup.GetCurrentHttpContext().Session.GetString("CCNetSessionToken");
		}
		#endregion

        #region DisplayName
        /// <summary>
        /// Stores the display name in a cookie or deletes the cookie.
        /// </summary>
        public void StoreDisplayName(string displayName)
        {
	        __Fixup.GetCurrentHttpContext().Session.SetString("CCNetDisplayName", displayName);
		}
        #endregion

        #region RetrieveDisplayName()
        /// <summary>
        /// Retrieve the display name.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string RetrieveDisplayName(IRequest request)
        {
            return (String)__Fixup.GetCurrentHttpContext().Session.GetString("CCNetDisplayName");
        }
        #endregion

		#endregion
	}
}
