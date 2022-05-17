using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using WellEngineered.CruiseControl.Remote.Security;

namespace WellEngineered.CruiseControl.Remote.Messages
{
    /// <summary>
    /// The response containing user details.
    /// </summary>
    [XmlRoot("listUsersResponse")]
    [Serializable]
    public class ListUsersResponse
        : Response
    {
        #region Private fields
        private List<UserDetails> users = new List<UserDetails>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a new instance of <see cref="ListUsersResponse"/>.
        /// </summary>
        public ListUsersResponse()
            : base()
        {
        }

        /// <summary>
        /// Initialise a new instance of <see cref="ListUsersResponse"/> from a request.
        /// </summary>
        /// <param name="request">The request to use.</param>
        public ListUsersResponse(ServerRequest request)
            : base(request)
        {
        }

        /// <summary>
        /// Initialise a new instance of <see cref="ListUsersResponse"/> from a response.
        /// </summary>
        /// <param name="response">The response to use.</param>
        public ListUsersResponse(Response response)
            : base(response)
        {
        }
        #endregion

        #region Public properties
        #region Users
        /// <summary>
        /// The users.
        /// </summary>
        [XmlElement("user")]
        public List<UserDetails> Users
        {
            get { return this.users; }
            set { this.users = value; }
        }
        #endregion
        #endregion
    }
}
