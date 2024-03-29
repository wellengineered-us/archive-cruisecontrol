using System;

using WellEngineered.CruiseControl.Remote.Messages;
//using System.Runtime.Remoting;

namespace WellEngineered.CruiseControl.Remote
{
    /// <summary>
    /// A server connection using .NET remoting.
    /// </summary>
    public class RemotingConnection
        : ServerConnectionBase, IServerConnection, IDisposable
    {
        #region Private fields
        private const string managerUri = "CruiseManager.rem";
        private const string serverClientUri = "CruiseServerClient.rem";
        private readonly Uri serverAddress;
        private IMessageProcessor client;
        private bool isBusy;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialises a new <see cref="RemotingConnection"/> to a remote server.
        /// </summary>
        /// <param name="serverAddress">The address of the remote server.</param>
        public RemotingConnection(string serverAddress)
            : this(new Uri(serverAddress))
        {
        }

        /// <summary>
        /// Initialises a new <see cref="RemotingConnection"/> to a remote server.
        /// </summary>
        /// <param name="serverAddress">The address of the remote server.</param>
        public RemotingConnection(Uri serverAddress)
        {
            UriBuilder builder = new UriBuilder(serverAddress);
            if (builder.Port == -1) builder.Port = 21234;
            this.serverAddress = new Uri(builder.Uri, "/CruiseManager.rem");
        }
        #endregion

        #region Public properties
        #region Type
        /// <summary>
        /// The type of connection.
        /// </summary>
        public string Type
        {
            get { return ".NET Remoting"; }
        }
        #endregion

        #region ServerName
        /// <summary>
        /// The name of the server that this connection is for.
        /// </summary>
        public string ServerName
        {
            get { return this.serverAddress.Host; }
        }
        #endregion

        #region IsBusy
        /// <summary>
        /// Is this connection busy performing an operation.
        /// </summary>
        public bool IsBusy
        {
            get { return this.isBusy; }
        }
        #endregion

        #region Address
        /// <summary>
        /// The address of the client.
        /// </summary>
        public virtual string Address
        {
            get { return this.serverAddress.AbsoluteUri; }
        }
        #endregion
        #endregion

        #region Public methods
        #region SendMessage()
        /// <summary>
        /// Sends a message via HTTP.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual Response SendMessage(string action, ServerRequest request)
        {
            // Initialise the connection and send the message
            try
            {
                this.InitialiseRemoting();
                this.FireRequestSending(action, request);
                Response result = this.client.ProcessMessage(action, request);
                this.FireResponseReceived(action, result);
                return result;
            }
            catch (Exception error)
            {
                // Replace the original exception with a communications exception
                throw new CommunicationsException(error.Message);
            }
        }
        #endregion

        #region SendMessageAsync()
        /// <summary>
        /// Sends a message to a remote server asychronously.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="request">The request to send to the server.</param>
        public virtual void SendMessageAsync(string action, ServerRequest request)
        {
            this.SendMessageAsync(action, request, null);
        }

        /// <summary>
        /// Sends a message to a remote server asychronously.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="userState">Any user state data.</param>
        /// <remarks>
        /// This operation will still be done in a synchronous mode.
        /// </remarks>
        public virtual void SendMessageAsync(string action, ServerRequest request, object userState)
        {
            if (this.isBusy) throw new InvalidOperationException();

            try
            {
                this.isBusy = true;
                this.InitialiseRemoting();
                Response result = this.client.ProcessMessage(action, request);

                if (this.SendMessageCompleted != null)
                {
                    MessageReceivedEventArgs args = new MessageReceivedEventArgs(result, null, false, userState);
                    this.SendMessageCompleted(this, args);
                }
            }
            catch (Exception error)
            {
                if (this.SendMessageCompleted != null)
                {
                    MessageReceivedEventArgs args = new MessageReceivedEventArgs(null, error, false, userState);
                    this.SendMessageCompleted(this, args);
                }
            }
            finally
            {
                this.isBusy = false;
            }
        }
        #endregion

        #region CancelAsync()
        /// <summary>
        /// Cancels an asynchronous operation.
        /// </summary>
        public virtual void CancelAsync()
        {
            this.CancelAsync(null);
        }

        /// <summary>
        /// Cancels an asynchronous operation.
        /// </summary>
        /// <param name="userState"></param>
        public void CancelAsync(object userState)
        {
            // .NET remoting operations are not asynchronous, therefore can't cancel anything
        }
        #endregion

        #region Dispose()
        /// <summary>
        /// Disposes the .NET remoting client.
        /// </summary>
        public virtual void Dispose()
        {
            this.client = null;
        }
        #endregion
        #endregion

        #region Public events
        #region SendMessageCompleted
        /// <summary>
        /// A SendMessageAsync has completed.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> SendMessageCompleted;
        #endregion
        #endregion

        #region Private methods
        #region InitialiseRemoting()
        /// <summary>
        /// Initialises the client connection.
        /// </summary>
        private void InitialiseRemoting()
        {
            if (this.client == null)
            {
                // Handle both old and new style connections
                var actualUri = this.serverAddress.AbsoluteUri;
                if (actualUri.EndsWith(managerUri, StringComparison.OrdinalIgnoreCase))
                {
                    actualUri = actualUri.Substring(0, actualUri.Length - managerUri.Length) + serverClientUri;
                }

                // Initialise the actual client
                //client = RemotingServices.Connect(typeof(IMessageProcessor),
                    //actualUri) as IMessageProcessor;
            }
        }
        #endregion
        #endregion
    }
}
