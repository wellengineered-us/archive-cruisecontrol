using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WellEngineered.CruiseControl.Remote
{
	/// <summary>
	/// A snapshot of a particular integration queue and it's contents.
	/// </summary>
	[Serializable]
    [XmlRoot("queueSnapshot")]
	public class QueueSnapshot
	{
		private string queueName;
        private List<QueuedRequestSnapshot> queueRequests = new List<QueuedRequestSnapshot>();

        /// <summary>
        /// Initialise a new blank <see cref="QueueSnapshot"/>.
        /// </summary>
        public QueueSnapshot()
        {
        }

        /// <summary>
        /// Initialise a new populated <see cref="QueueSnapshot"/>.
        /// </summary>
        /// <param name="queueName"></param>
		public QueueSnapshot(string queueName)
		{
			this.queueName = queueName;
		}

        /// <summary>
        /// The name of the queue.
        /// </summary>
        [XmlAttribute("name")]
		public string QueueName
		{
			get { return this.queueName; }
            set { this.queueName = value; }
		}

        /// <summary>
        /// The current requests in the queue.
        /// </summary>
        [XmlElement("queueRequest")]
        public List<QueuedRequestSnapshot> Requests
		{
			get { return this.queueRequests; }
		}

        /// <summary>
        /// Whether there are any requests in the queue or not.
        /// </summary>
        [XmlIgnore]
        public bool IsEmpty
        {
            get { return this.queueRequests.Count == 0; }
        }
    }
}