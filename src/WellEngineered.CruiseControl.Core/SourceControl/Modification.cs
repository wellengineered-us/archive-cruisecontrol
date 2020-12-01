using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using WellEngineered.CruiseControl.Core.Util;

namespace WellEngineered.CruiseControl.Core.SourceControl
{
    /// <summary>
    /// Value object representing the data associated with a source control modification.
    /// </summary>
    [XmlRoot("modification")]
    public class Modification : IComparable
    {
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public string Type = "unknown";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public string FileName;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public string FolderName;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public DateTime ModifiedTime;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public string UserName;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public string ChangeNumber;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public string Version = string.Empty;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public string Comment;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public string Url;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public string IssueUrl;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public string EmailAddress;

        /// <summary>
        /// Toes the XML.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ToXml()
        {
            StringWriter writer = new StringWriter();
            this.ToXml(new XmlTextWriter(writer));
            return writer.ToString();
        }

        /// <summary>
        /// Toes the XML.	
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <remarks></remarks>
        public void ToXml(XmlWriter writer)
        {
            writer.WriteStartElement("modification");
            writer.WriteAttributeString("type", this.Type);
            writer.WriteElementString("filename", this.FileName);
            writer.WriteElementString("project", this.FolderName);
            writer.WriteElementString("date", DateUtil.FormatDate(this.ModifiedTime));
            writer.WriteElementString("user", this.UserName);
            writer.WriteElementString("comment", this.Comment);
            writer.WriteElementString("changeNumber", this.ChangeNumber);
            if (!string.IsNullOrEmpty(this.Version)) writer.WriteElementString("version", this.Version);
            XmlUtil.WriteNonNullElementString(writer, "url", this.Url);
            XmlUtil.WriteNonNullElementString(writer, "issueUrl", this.IssueUrl);
            XmlUtil.WriteNonNullElementString(writer, "email", this.EmailAddress);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Compares to.	
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public int CompareTo(Object o)
        {
            Modification modification = (Modification)o;
            return this.ModifiedTime.CompareTo(modification.ModifiedTime);
        }

        /// <summary>
        /// Gets the hash code.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// Equalses the specified obj.	
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override bool Equals(object obj)
        {
            return ReflectionUtil.ReflectionEquals(this, obj);
        }

        /// <summary>
        /// Toes the string.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string ToString()
        {
            return ReflectionUtil.ReflectionToString(this);
        }

        /// <summary>
        /// Retrieves the change number of the last modification.
        /// </summary>
        /// <param name="modifications">The modifications to check.</param>
        /// <returns>The last change number if there are any changes, null otherwise.</returns>
        /// <remarks>
        /// Since ChangeNumbers are no longer numbers, this will return null if there are no 
        /// modifications.
        /// </remarks>
        public static string GetLastChangeNumber(Modification[] modifications)
        {
            var lastModification = new Modification
            {
                ModifiedTime = DateTime.MinValue,
                ChangeNumber = null
            };
            foreach (Modification modification in modifications)
            {
                if (modification.ModifiedTime > lastModification.ModifiedTime)
                {
                    lastModification = modification;
                }
            }
            return lastModification.ChangeNumber;
        }
    }
}