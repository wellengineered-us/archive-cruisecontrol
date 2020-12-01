using System.IO;

using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.WebDashboard.IO;

using Microsoft.AspNetCore.Http;
using Microsoft.Win32;

namespace WellEngineered.CruiseControl.WebDashboard.MVC
{
    public class FileTransferResponse : IResponse
    {
        private IFileTransfer fileTransfer;
        private ConditionalGetFingerprint serverFingerprint;
        private string fileName;
        private string type;

        public FileTransferResponse(IFileTransfer fileTransfer, string fileName)
            : this(fileTransfer, fileName, null)
        {
        }

        public FileTransferResponse(IFileTransfer fileTransfer, string fileName, string type)
        {
            this.fileTransfer = fileTransfer;
            this.fileName = fileName;
            this.type = type;
        }

        public IFileTransfer FileTransfer { get { return this.fileTransfer; } }
        public string FileName { get { return this.fileName; } }
        public string FileType { get { return this.type; } }

        public void Process(HttpResponse response)
        {
            response.Headers.Add("Last-Modified", this.serverFingerprint.LastModifiedTime.ToString("r"));
            response.Headers.Add("ETag", this.serverFingerprint.ETag);
            response.Headers.Add("Cache-Control", "private, max-age=0");
            if (!string.IsNullOrEmpty(this.fileName))
            {
                response.Headers.Add(
                    "Content-Disposition",
                    "attachment; filename=\"" + Path.GetFileName(this.fileName) + "\"");
            }
            if (!string.IsNullOrEmpty(this.type))
            {
                response.ContentType = this.type;
            }
            else if (!string.IsNullOrEmpty(this.fileName))
            {
                var mimeType = this.GetMimeType(this.fileName);
                response.ContentType = mimeType;
            }
            else
            {
                response.ContentType = "application/octetstream";
            }

            this.fileTransfer.Download(response.GetOutputStream());
        }

        public ConditionalGetFingerprint ServerFingerprint
        {
            get { return this.serverFingerprint; }
            set { this.serverFingerprint = value; }
        }

        #region GetMimeType()
        /// <summary>
        /// Retrieves the mime type for a file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string GetMimeType(string filename)
        {
            var mimeType = "application/octetstream";
            var fileExtension = Path.GetExtension(filename).ToLower();
            var regkey = Registry.ClassesRoot.OpenSubKey(fileExtension);
            if ((regkey != null) && (regkey.GetValue("Content Type") != null))
            {
                mimeType = regkey.GetValue("Content Type").ToString();
            }
            return mimeType;
        }
        #endregion
    }
}