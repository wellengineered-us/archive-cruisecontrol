using System;

using WellEngineered.CruiseControl.Remote.Messages;

namespace WellEngineered.CruiseControl.Remote
{
    /// <summary>
    /// A base class for sever connections.
    /// </summary>
    public abstract class ServerConnectionBase
    {
        #region Public events
        #region RequestSending
        /// <summary>
        /// A request message is being sent.
        /// </summary>
        public event EventHandler<CommunicationsEventArgs> RequestSending;
        #endregion

        #region ResponseReceived
        /// <summary>
        /// A response message has been received.
        /// </summary>
        public event EventHandler<CommunicationsEventArgs> ResponseReceived;
        #endregion
        #endregion

        #region Protected methods
        #region FireRequestSending
        /// <summary>
        /// Fires the <see cref="RequestSending"/> event.
        /// </summary>
        /// <param name="action">The action that is being sent.</param>
        /// <param name="request">The request that is being sent.</param>
        protected virtual void FireRequestSending(string action, ServerRequest request)
        {
            if (this.RequestSending != null)
            {
                this.RequestSending(this, new CommunicationsEventArgs(action, request));
            }
        }
        #endregion

        #region FireResponseReceived
        /// <summary>
        /// Fires the <see cref="ResponseReceived"/> event.
        /// </summary>
        /// <param name="action">The action that the response is for.</param>
        /// <param name="response">The response that was received.</param>
        protected virtual void FireResponseReceived(string action, Response response)
        {
            if (this.ResponseReceived != null)
            {
                this.ResponseReceived(this, new CommunicationsEventArgs(action, response));
            }
        }
        #endregion
        #endregion
    }
}
