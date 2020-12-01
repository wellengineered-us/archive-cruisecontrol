using System;
using System.ComponentModel;
using System.Xml.Serialization;

using WellEngineered.CruiseControl.Remote.Security;

namespace WellEngineered.CruiseControl.Remote.Messages
{
    /// <summary>
    /// A request message for reading the audit log.
    /// </summary>
    [XmlRoot("readAuditMessage")]
    [Serializable]
    public class ReadAuditRequest
        : ServerRequest
    {
        #region Private fields
        private int startRecord/* = 0*/;
        private int numberOfRecords = int.MaxValue;
        private AuditFilterBase filter;
        #endregion

        #region Public properties
        #region StartRecord
        /// <summary>
        /// The starting record number.
        /// </summary>
        [XmlAttribute("start")]
        [DefaultValue(0)]
        public int StartRecord
        {
            get { return this.startRecord; }
            set { this.startRecord = value; }
        }
        #endregion

        #region NumberOfRecords
        /// <summary>
        /// The number of records to read.
        /// </summary>
        [XmlAttribute("number")]
        [DefaultValue(int.MaxValue)]
        public int NumberOfRecords
        {
            get { return this.numberOfRecords; }
            set { this.numberOfRecords = value; }
        }
        #endregion

        #region Filter
        /// <summary>
        /// The filter to apply.
        /// </summary>
        [XmlElement("filter")]
        public AuditFilterBase Filter
        {
            get { return this.filter; }
            set { this.filter = value; }
        }
        #endregion
        #endregion
    }
}
