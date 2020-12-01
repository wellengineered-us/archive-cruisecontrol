// edtFTPnet
// 
// Copyright (C) 2009 Enterprise Distributed Technologies Ltd
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
// $Log: BandwidthThrottler.cs,v $
// Revision 1.2  2009-11-04 06:11:32  hans
// Added cancellation.
//
// Revision 1.1  2009-03-10 05:29:18  bruceb
// bandwidth throttling
//
//
//
//

using System;
using System.Threading;

using WellEngineered.CruiseControl.PrivateBuild.EdtFtpNet.Util.Deebug;

namespace WellEngineered.CruiseControl.PrivateBuild.EdtFtpNet.Net.Ftp
{

    /// <summary>  
    /// Helps throttle bandwidth for transfers
    /// </summary>
    /// <author>       
    /// Bruce Blackshaw
    /// </author>
    /// <version>      
    /// $Revision: 1.2 $
    /// </version>
    public class BandwidthThrottler
    {
        private Logger log = Logger.GetLogger("BandwidthThrottler");
        private DateTime lastTime = DateTime.MinValue;
        private long lastBytes = 0;
        private int thresholdBytesPerSec = -1;
        private bool cancel = false;

        public BandwidthThrottler(int thresholdBytesPerSec)
        {
            this.thresholdBytesPerSec = thresholdBytesPerSec;
        }

        public int Threshold
        {
            get { return this.thresholdBytesPerSec; }
            set { this.thresholdBytesPerSec = value; }
        }

        public void ThrottleTransfer(long bytesSoFar)
        {
            DateTime time = DateTime.Now;
            long diffBytes = bytesSoFar - this.lastBytes;
            long diffTime = (long)((time - this.lastTime).TotalMilliseconds);
            if (diffTime == 0)
                return;

            double rate = ((double)diffBytes / (double)diffTime) * 1000.0;
            this.log.Debug("rate={0}", rate);

            while (rate > this.thresholdBytesPerSec && !this.cancel)
            {
                this.log.Log(Level.ALL, "Sleeping to decrease transfer rate (rate = {0} bytes/s)", rate);
                Thread.Sleep(100);
                diffTime = (long)((DateTime.Now - this.lastTime).TotalMilliseconds);
                rate = ((double)diffBytes / (double)diffTime) * 1000.0;
            }
            this.lastTime = time;
            this.lastBytes = bytesSoFar;
        }

        public void Reset()
        {
            this.lastTime = DateTime.Now;
            this.lastBytes = 0;
            this.cancel = false;
        }

        public void Cancel()
        {
            this.cancel = true;
        }
    }
}
