// edtFTPnet
// 
// Copyright (C) 2008 Enterprise Distributed Technologies Ltd
// 
// www.enterprisedt.com
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
// 
// Bug fixes, suggestions and comments should posted on 
// http://www.enterprisedt.com/forums/index.php
// 
// Change Log:
// 
// $Log: RollingFileAppender.cs,v $
// Revision 1.4  2010-12-06 00:25:54  bruceb
// tweaks only
//
// Revision 1.3  2010-03-18 02:53:28  bruceb
// lock access and change check test
//
// Revision 1.2  2009-02-27 06:56:29  bruceb
// added FileName and rename log to logger
//
// Revision 1.1  2008-12-11 04:44:52  bruceb
// added RollingFileAppender
//
//
//

using System;
using System.IO;
using System.Text;

namespace WellEngineered.CruiseControl.PrivateBuild.EdtFtpNet.Util.Deebug
{    
	/// <summary>  
	/// Rolling file appender that moves the log file to
    /// a backup file once it exceeds a certain size.
	/// </summary>
	/// <author>       Bruce Blackshaw
	/// </author>
	/// <version>      $Revision: 1.4 $
	/// </version>
	public class RollingFileAppender : FileAppender
	{
        private const long DEFAULT_MAXSIZE = 10 * 1024 * 1024;

        private const int CHECK_THRESHOLD_BYTES = 1024 * 5;

		/// <summary> Destination</summary>
        private long maxFileSize = DEFAULT_MAXSIZE;

        /// <summary>
        /// record of bytes written in this check cycle
        /// </summary>
        private int thresholdBytesWritten = 0;

        /// <summary>
        /// Maximum number of backup files
        /// </summary>
        private int maxSizeRollBackups = 1;
		
		/// <summary>Constructor</summary>
		/// <param name="fileName">name of file to log to</param>
        /// <param name="maxFileSize">maximum size of log file in bytes</param>
        public RollingFileAppender(string fileName, long maxFileSize)
            : base(fileName)
		{
            this.maxFileSize = maxFileSize;
            this.CheckSizeForRollover();
		}

        /// <summary>Constructor</summary>
        /// <param name="fileName">name of file to log to</param>
        /// <remarks>
        /// Default maximum size of logfile is 10MB
        ///</remarks>
        public RollingFileAppender(string fileName)
            : base(fileName)
        {
            this.CheckSizeForRollover();
        }

        /// <summary>
        /// Set the maximum number of backup files to retain. If this is set to
        /// zero, the log file will be truncated when it reaches the MaxSize. Backup
        /// files are called FileName.1, FileName.2 etc. This value can't be negative.
        /// The default is 1 backup file.
        /// </summary>
        public int MaxSizeRollBackups 
        {
            get { return this.maxSizeRollBackups; }
            set { this.maxSizeRollBackups = value >= 0 ? value: 0; }
        }

        /// <summary>
        /// Maximum size of log file in bytes
        /// </summary>
        public long MaxFileSize
        {
            get { return this.maxFileSize; }
            set { this.maxFileSize = value; }
        }

        /// <summary>
        /// Check if files should be rolled over
        /// </summary>
        private void CheckForRollover()
        {
            // only check every so often to enhance performance
            if (this.thresholdBytesWritten < CHECK_THRESHOLD_BYTES)
                return;

            this.thresholdBytesWritten = 0;

            this.CheckSizeForRollover();
        }

        /// <summary>
        /// Check if the file's size means it should be rolled over
        /// </summary>
        private void CheckSizeForRollover()
        {
            try
            {
                // check if bigger
                if (this.fileStream.Length > this.maxFileSize)
                {
                    this.Rollover();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Failed to rollover log files (" + ex.Message + ")");
            }
        }

        /// <summary>
        /// Rollover the log files
        /// </summary>
        private void Rollover()
        {
            this.Close();
            FileInfo master = new FileInfo(this.FileName);
            if (this.maxSizeRollBackups == 0)
                master.Delete();
            else // roll all files
            {
                // delete highest if exists
                FileInfo f = new FileInfo(this.FileName + "." + this.maxSizeRollBackups);
                if (f.Exists)
                    f.Delete();
                for (int i = this.maxSizeRollBackups-1; i > 0; i--)
                {
                    f = new FileInfo(this.FileName + "." + i);
                    if (f.Exists)
                        f.MoveTo(this.FileName + "." + (i + 1));
                }
                master.MoveTo(this.FileName + ".1");
            }
            this.Open();
        }

		
		/// <summary> 
		/// Log a message
		/// </summary>
		/// <param name="msg"> message to log
		/// </param>
		public override void Log(string msg)
		{
            lock (this)
            {
                if (!this.closed)
                {
                    this.CheckForRollover();
                    this.logger.WriteLine(msg);
                    this.thresholdBytesWritten += msg.Length;
                    this.logger.Flush();
                }
                else
                {
                    System.Console.WriteLine(msg);
                }
            }
		}
		
		/// <summary> 
		/// Log a stack trace
		/// </summary>
		/// <param name="t"> throwable object
		/// </param>
		public override void Log(Exception t)
		{
            StringBuilder msg = new StringBuilder(t.GetType().FullName);
            msg.Append(": ").Append(t.Message);
            lock (this)
            {
                if (!this.closed)
                {
                    this.CheckForRollover();
                    string line = msg.ToString();
                    this.logger.WriteLine(line);
                    this.thresholdBytesWritten += line.Length;
                    line = t.StackTrace.ToString();
                    this.logger.WriteLine(line);
                    this.thresholdBytesWritten += line.Length;
                    this.logger.Flush();
                }
                else
                {
                    System.Console.WriteLine(msg.ToString());
                }
            }
		}
	}
}
