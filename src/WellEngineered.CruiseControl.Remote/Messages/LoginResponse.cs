using System;
using System.Xml.Serialization;

namespace WellEngineered.CruiseControl.Remote.Messages
{
    /// <summary>
    /// The response from a login request.
    /// </summary>
    [XmlRoot("loginResponse")]
    [Serializable]
    public class LoginResponse
        : Response
    {
        #region Private fields
        private string sessionToken;
        private string displayName;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a new instance of <see cref="LoginResponse"/>.
        /// </summary>
        public LoginResponse()
            : base()
        {
        }

        /// <summary>
        /// Initialise a new instance of <see cref="LoginResponse"/> from a request.
        /// </summary>
        /// <param name="request">The request to use.</param>
        public LoginResponse(ServerRequest request)
            : base(request)
        {
        }

        /// <summary>
        /// Initialise a new instance of <see cref="LoginResponse"/> from a response.
        /// </summary>
        /// <param name="response">The response to use.</param>
        public LoginResponse(Response response)
            : base(response)
        {
        }
        #endregion

        #region Public properties
        #region SessionToken
        /// <summary>
        /// The token for the new session.
        /// </summary>
        [XmlAttribute("sessionToken")]
        public string SessionToken
        {
            get { return this.sessionToken; }
            set { this.sessionToken = value; }
        }
        #endregion

        #region DisplayName
        /// <summary>
        /// The display name of the user.
        /// </summary>
        [XmlAttribute("displayName")]
        public string DisplayName
        {
            get { return this.displayName; }
            set { this.displayName = value; }
        }
        #endregion
        #endregion
    }
}
