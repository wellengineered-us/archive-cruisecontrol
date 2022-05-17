using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using WellEngineered.CruiseControl.Remote.Security;

namespace WellEngineered.CruiseControl.Remote.Messages
{
    /// <summary>
    /// The response containing a list of security diagnostics.
    /// </summary>
    [XmlRoot("diagnoseSecurityResponse")]
    [Serializable]
    public class DiagnoseSecurityResponse
        : Response
    {
        #region Private fields
        private List<SecurityCheckDiagnostics> diagnostics = new List<SecurityCheckDiagnostics>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a new instance of <see cref="DiagnoseSecurityResponse"/>.
        /// </summary>
        public DiagnoseSecurityResponse()
            : base()
        {
        }

        /// <summary>
        /// Initialise a new instance of <see cref="DiagnoseSecurityResponse"/> from a request.
        /// </summary>
        /// <param name="request">The request to use.</param>
        public DiagnoseSecurityResponse(ServerRequest request)
            : base(request)
        {
        }

        /// <summary>
        /// Initialise a new instance of <see cref="DiagnoseSecurityResponse"/> from a response.
        /// </summary>
        /// <param name="response">The response to use.</param>
        public DiagnoseSecurityResponse(Response response)
            : base(response)
        {
        }
        #endregion

        #region Public properties
        #region Data
        /// <summary>
        /// The diagnostics.
        /// </summary>
        [XmlElement("diagnosis")]
        public List<SecurityCheckDiagnostics> Diagnostics
        {
            get { return this.diagnostics; }
            set { this.diagnostics = value; }
        }
        #endregion
        #endregion
    }
}
