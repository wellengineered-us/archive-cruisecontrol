using System;
using System.Xml.Serialization;

namespace WellEngineered.CruiseControl.Remote.Messages
{
    /// <summary>
    /// A change password request message.
    /// </summary>
    [XmlRoot("changePasswordMessage")]
    [Serializable]
    public class ChangePasswordRequest
        : ServerRequest
    {
        #region Private fields
        private string oldPassword;
        private string newPassword;
        private string userName;
        #endregion

        #region Public properties
        #region OldPassword
        /// <summary>
        /// The old password
        /// </summary>
        [XmlAttribute("oldPassword")]
        public string OldPassword
        {
            get { return this.oldPassword; }
            set { this.oldPassword = value; }
        }
        #endregion

        #region NewPassword
        /// <summary>
        /// The new password
        /// </summary>
        [XmlAttribute("newPassword")]
        public string NewPassword
        {
            get { return this.newPassword; }
            set { this.newPassword = value; }
        }
        #endregion

        #region UserName
        /// <summary>
        /// The user name.
        /// </summary>
        [XmlAttribute("userName")]
        public string UserName
        {
            get { return this.userName; }
            set { this.userName = value; }
        }
        #endregion
        #endregion
    }
}
