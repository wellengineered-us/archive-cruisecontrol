using System.Net;
using System.Net.Mail;

using WellEngineered.CruiseControl.Core.Util;

namespace WellEngineered.CruiseControl.Core.Publishers
{
    /// <summary>
    /// 	
    /// </summary>
    public class EmailGateway
    {
        private string mailhostUsername = null;
        private PrivateString mailhostPassword = null;
        private SmtpClient smtpServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailGateway" /> class.	
        /// </summary>
        /// <remarks></remarks>
        public EmailGateway()
        {
            this.smtpServer = new SmtpClient();
        }

        /// <summary>
        /// Gets or sets the mail host.	
        /// </summary>
        /// <value>The mail host.</value>
        /// <remarks></remarks>
        public virtual string MailHost
        {
            get { return this.smtpServer.Host; }
            set { this.smtpServer.Host = value; }
        }

        /// <summary>
        /// Gets or sets the mail port.	
        /// </summary>
        /// <value>The mail port.</value>
        /// <remarks></remarks>
        public virtual int MailPort
        {
            get { return this.smtpServer.Port; }
            set { this.smtpServer.Port = value; }
        }

        /// <summary>
        /// Gets or sets the use SSL.	
        /// </summary>
        /// <value>The use SSL.</value>
        /// <remarks></remarks>
        public bool UseSSL
        {
            get { return this.smtpServer.EnableSsl ; }
            set { this.smtpServer.EnableSsl = value; }
        }

        /// <summary>
        /// Gets or sets the mail host username.	
        /// </summary>
        /// <value>The mail host username.</value>
        /// <remarks></remarks>
        public string MailHostUsername
        {
            get { return this.mailhostUsername; }
            set { this.mailhostUsername = value; }
        }

        /// <summary>
        /// Gets or sets the mail host password.	
        /// </summary>
        /// <value>The mail host password.</value>
        /// <remarks></remarks>
        public PrivateString MailHostPassword
        {
            get { return this.mailhostPassword; }
            set { this.mailhostPassword = value; }
        }

        /// <summary>
        /// Sends the specified mail message.	
        /// </summary>
        /// <param name="mailMessage">The mail message.</param>
        /// <remarks></remarks>
        public virtual void Send(MailMessage mailMessage)
        {
            if (this.MailHostUsername != null && this.MailHostPassword != null)
            {
                this.smtpServer.Credentials = new NetworkCredential(this.mailhostUsername, this.mailhostPassword.PrivateValue);
            }
            this.smtpServer.Send(mailMessage);
        }
    }
}