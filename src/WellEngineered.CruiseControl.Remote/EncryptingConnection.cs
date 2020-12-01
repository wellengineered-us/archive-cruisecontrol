using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using WellEngineered.CruiseControl.Remote.Messages;

namespace WellEngineered.CruiseControl.Remote
{
    /// <summary>
    /// A server connection that will encrypt any transmitted data.
    /// </summary>
    public class EncryptingConnection
        : ServerConnectionBase, IServerConnection, IDisposable
    {
        #region Private fields
        private IServerConnection innerConnection;
        private byte[] cryptoKey = new byte[0];
        private byte[] cryptoIv = new byte[0];
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a new <see cref="EncryptingConnection"/>.
        /// </summary>
        /// <param name="innerConnection">The connection for sending messages.</param>
        public EncryptingConnection(IServerConnection innerConnection)
        {
            this.innerConnection = innerConnection;
            innerConnection.SendMessageCompleted += this.PassOnSendMessageCompleted;
            innerConnection.RequestSending += this.PassOnRequestSending;
            innerConnection.ResponseReceived += this.PassOnResponseReceived;
        }
        #endregion

        #region Properties
        #region Type
        /// <summary>
        /// The type of connection.
        /// </summary>
        public string Type
        {
            get { return this.innerConnection.Type; }
        }

        #region Address
        /// <summary>
        /// The address of the client.
        /// </summary>
        public virtual string Address
        {
            get { return this.innerConnection.Address; }
        }
        #endregion
        #endregion

        #region ServerName
        /// <summary>
        /// The name of the server that this connection is for.
        /// </summary>
        public string ServerName
        {
            get { return this.innerConnection.ServerName; }
        }
        #endregion

        #region IsBusy
        /// <summary>
        /// Is this connection busy performing an operation.
        /// </summary>
        public bool IsBusy
        {
            get { return this.innerConnection.IsBusy; }
        }
        #endregion
        #endregion

        #region Methods
        #region SendMessage()
        /// <summary>
        /// Sends a message to a remote server.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="request">The request to send to the server.</param>
        /// <returns>The response from the server.</returns>
        public Response SendMessage(string action, ServerRequest request)
        {
            // Make sure there is a password
            if ((this.cryptoKey.Length == 0) || (this.cryptoIv.Length == 0)) this.InitialisePassword();

            // Generate the encrypted request
            var encryptedRequest = new EncryptedRequest();
            encryptedRequest.Action = action;
            var crypto = new RijndaelManaged();
            crypto.Key = this.cryptoKey;
            crypto.IV = this.cryptoIv;
            encryptedRequest.EncryptedData = EncryptMessage(crypto, request.ToString());

            // Send the request
            var response = this.innerConnection.SendMessage("ProcessSecureRequest", encryptedRequest);
            var encryptedResponse = response as EncryptedResponse;

            // Generate the actual response
            if ((response.Result == ResponseResult.Success) && (encryptedResponse != null))
            {
                var data = DecryptMessage(crypto, encryptedResponse.EncryptedData);
                response = XmlConversionUtil.ProcessResponse(data);
            }
            return response;
        }
        #endregion

        #region SendMessageAsync()
        /// <summary>
        /// Sends a message to a remote server asychronously.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="request">The request to send to the server.</param>
        public void SendMessageAsync(string action, ServerRequest request)
        {
            this.SendMessageAsync(action, request, null);
        }

        /// <summary>
        /// Sends a message to a remote server asychronously.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="userState">Any user state data.</param>
        public void SendMessageAsync(string action, ServerRequest request, object userState)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region CancelAsync()
        /// <summary>
        /// Cancels an asynchronous operation.
        /// </summary>
        public void CancelAsync()
        {
            this.CancelAsync(null);
        }

        /// <summary>
        /// Cancels an asynchronous operation.
        /// </summary>
        /// <param name="userState"></param>
        public void CancelAsync(object userState)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Dispose()
        /// <summary>
        /// Disposes the .NET remoting client.
        /// </summary>
        public virtual void Dispose()
        {
            var disposable = this.innerConnection as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
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
        #region InitialisePassword()
        /// <summary>
        /// Initialise the password.
        /// </summary>
        private void InitialisePassword()
        {
            try
            {
                // Request the public key
                var publicKeyRequest = new ServerRequest();
                var publicKeyResponse = this.innerConnection.SendMessage("RetrievePublicKey", publicKeyRequest);
                if (publicKeyResponse.Result == ResponseResult.Failure)
                {
                    throw new CommunicationsException("Server does not export a public key: " + publicKeyResponse.ConcatenateErrors());
                }

                // Generate a password 
                var crypto = new RijndaelManaged();
                crypto.KeySize = 128;
                crypto.GenerateKey();
                crypto.GenerateIV();
                this.cryptoKey = crypto.Key;
                this.cryptoIv = crypto.IV;
                
                // Encrypt the password
                var passwordKey = Convert.ToBase64String(this.cryptoKey);
                var passwordIv = Convert.ToBase64String(this.cryptoIv);
                var provider = new RSACryptoServiceProvider();
                provider.FromXmlString((publicKeyResponse as DataResponse).Data);
                var encryptedPasswordKey = Convert.ToBase64String(
                    provider.Encrypt(
                        UTF8Encoding.UTF8.GetBytes(passwordKey), false));
                var encryptedPasswordIv = Convert.ToBase64String(
                    provider.Encrypt(
                        UTF8Encoding.UTF8.GetBytes(passwordIv), false));

                // Send the password to the server
                var loginRequest = new LoginRequest(encryptedPasswordKey);
                loginRequest.AddCredential(LoginRequest.PasswordCredential, encryptedPasswordIv);
                var loginResponse = this.innerConnection.SendMessage("InitialiseSecureConnection", loginRequest);
                if (loginResponse.Result == ResponseResult.Failure)
                {
                    throw new CommunicationsException("Server did not allow the connection to be secured: " + loginResponse.ConcatenateErrors());
                }
            }
            catch
            {
                // Reset the password on any exception
                this.cryptoIv = new byte[0];
                this.cryptoKey = new byte[0];
                throw;
            }
        }
        #endregion

        #region EncryptMessage()
        private static string EncryptMessage(RijndaelManaged crypto, string message)
        {
            var encryptStream = new MemoryStream();
            var encrypt = new CryptoStream(encryptStream, 
                crypto.CreateEncryptor(), 
                CryptoStreamMode.Write);

            var dataToEncrypt = Encoding.UTF8.GetBytes(message);
            encrypt.Write(dataToEncrypt, 0, dataToEncrypt.Length); 
            encrypt.FlushFinalBlock();
            encrypt.Close();

            var data = Convert.ToBase64String(encryptStream.ToArray());
            return data;
        }
        #endregion

        #region DecryptMessage()
        private static string DecryptMessage(RijndaelManaged crypto, string message)
        {
            var inputStream = new MemoryStream(Convert.FromBase64String(message));
            string data;
            using (var decryptionStream = new CryptoStream(inputStream,
                crypto.CreateDecryptor(),
                CryptoStreamMode.Read))
            {
                using (var reader = new StreamReader(decryptionStream))
                {
                    data = reader.ReadToEnd();
                }
            }
            return data;
        }
        #endregion

        #region PassOnSendMessageCompleted()
        /// <summary>
        /// Passes on the <see cref="SendMessageCompleted"/> event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void PassOnSendMessageCompleted(object sender, MessageReceivedEventArgs args)
        {
            if (this.SendMessageCompleted != null)
            {
                this.SendMessageCompleted(this, args);
            }
        }
        #endregion

        #region PassOnRequestSending()
        /// <summary>
        /// Passes on the RequestSending event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void PassOnRequestSending(object sender, CommunicationsEventArgs args)
        {
            this.FireRequestSending(args.Action, args.Message as ServerRequest);
        }
        #endregion

        #region PassOnResponseReceived()
        /// <summary>
        /// Passes on the ResponseReceived event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void PassOnResponseReceived(object sender, CommunicationsEventArgs args)
        {
            this.FireResponseReceived(args.Action, args.Message as Response);
        }
        #endregion
        #endregion
    }
}
