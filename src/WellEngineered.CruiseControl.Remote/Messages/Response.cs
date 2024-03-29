using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace WellEngineered.CruiseControl.Remote.Messages
{
    /// <summary>
    /// The base level message for all responses.
    /// </summary>
    [XmlRoot("response")]
    [Serializable]
    public class Response
        : CommunicationsMessage
    {
        #region Private fields
        private List<ErrorMessage> errorMessages;
        private string requestIdentifier;
        private ResponseResult result = ResponseResult.Unknown;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a new instance of <see cref="Response"/>.
        /// </summary>
        public Response()
        {
            this.errorMessages = new List<ErrorMessage>();
        }

        /// <summary>
        /// Initialise a new instance of <see cref="Response"/> from a request.
        /// </summary>
        /// <param name="request">The request to use.</param>
        public Response(ServerRequest request)
            : this()
        {
            this.requestIdentifier = request.Identifier;
        }

        /// <summary>
        /// Initialise a new instance of <see cref="Response"/> from a response.
        /// </summary>
        /// <param name="response">The response to use.</param>
        public Response(Response response)
        {
            this.errorMessages = response.errorMessages;
            this.requestIdentifier = response.requestIdentifier;
            this.result = response.result;
            this.Timestamp = response.Timestamp;
        }
        #endregion

        #region Public properties
        #region ErrorMessage
        /// <summary>
        /// Any error messages.
        /// </summary>
        [XmlElement("error")]
        public List<ErrorMessage> ErrorMessages
        {
            get { return this.errorMessages; }
        }
        #endregion

        #region RequestIdentifier
        /// <summary>
        /// The identifier of the request that this response is for.
        /// </summary>
        [XmlAttribute("identifier")]
        public string RequestIdentifier
        {
            get { return this.requestIdentifier; }
            set { this.requestIdentifier = value; }
        }
        #endregion

        #region Result
        /// <summary>
        /// The outcome of the processing.
        /// </summary>
        [XmlAttribute("result")]
        public ResponseResult Result
        {
            get { return this.result; }
            set { this.result = value; }
        }
        #endregion
        #endregion

        #region Public methods
        #region Equals()
        /// <summary>
        /// Checks if this response is the same as another.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as Response;

            if (other != null)
            {
                return string.Equals(other.requestIdentifier, this.requestIdentifier, StringComparison.CurrentCulture) &&
                    DateTime.Equals(other.Timestamp, this.Timestamp);
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region GetHashCode()
        /// <summary>
        /// Returns the hash code for this response.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (this.requestIdentifier ?? string.Empty).GetHashCode() &
                this.Timestamp.GetHashCode();
        }
        #endregion

        #region ToString()
        /// <summary>
        /// Converts this request into a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            XmlSerializer serialiser = new XmlSerializer(this.GetType());
            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = UTF8Encoding.UTF8;
            settings.Indent = false;
            settings.OmitXmlDeclaration = true;
            XmlWriter writer = XmlWriter.Create(builder, settings);
            serialiser.Serialize(writer, this);
            return builder.ToString();
        }
        #endregion

        #region ConcatenateErrors()
        /// <summary>
        /// Concatenates all the error messages into one string.
        /// </summary>
        /// <returns></returns>
        public virtual string ConcatenateErrors()
        {
            List<string> errorMessages = new List<string>();
            foreach (ErrorMessage error in this.ErrorMessages)
            {
                errorMessages.Add(error.Message);
            }
            return string.Join(Environment.NewLine, errorMessages.ToArray());
        }
        #endregion
        #endregion
    }
}
