using System;
using System.Xml.Serialization;

namespace WellEngineered.CruiseControl.Remote.Messages
{
    /// <summary>
    /// An encrypted response.
    /// </summary>
    [XmlRoot("encryptedResponse")]
    [Serializable]
    public class EncryptedResponse
        : Response
    {
        #region Private fields
        private string encryptedData;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a new instance of <see cref="EncryptedResponse"/>.
        /// </summary>
        public EncryptedResponse()
            : base()
        {
        }

        /// <summary>
        /// Initialise a new instance of <see cref="EncryptedResponse"/> from a request.
        /// </summary>
        /// <param name="request">The request to use.</param>
        public EncryptedResponse(ServerRequest request)
            : base(request)
        {
        }

        /// <summary>
        /// Initialise a new instance of <see cref="EncryptedResponse"/> from a response.
        /// </summary>
        /// <param name="response">The response to use.</param>
        public EncryptedResponse(Response response)
            : base(response)
        {
        }
        #endregion

        #region Public properties
        #region EncryptedData
        /// <summary>
        /// The encrypted data.
        /// </summary>
        [XmlElement("data")]
        public string EncryptedData
        {
            get { return this.encryptedData; }
            set { this.encryptedData = value; }
        }
        #endregion
        #endregion
    }
}
