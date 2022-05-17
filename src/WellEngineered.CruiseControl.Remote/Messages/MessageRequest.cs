using System;
using System.Xml.Serialization;

namespace WellEngineered.CruiseControl.Remote.Messages
{
    /// <summary>
    /// A message for passing a message to the server.
    /// </summary>
    [XmlRoot("messageMessage")]
    [Serializable]
    public class MessageRequest
        : ProjectRequest
    {       
        private string message;
        private Message.MessageKind kind ;

        /// <summary>
        /// The message being passed.
        /// </summary>
        [XmlElement("message")]
        public string Message
        {
            get { return this.message; }
            set { this.message = value; }
        }

        /// <summary>
        /// The kind of message
        /// </summary>
        [XmlElement("kind")]
        public Message.MessageKind Kind
        {
            get { return this.kind; }
            set { this.kind = value; }
        }
    }
}
