using System;
using System.Threading;

namespace WellEngineered.CruiseControl.PrivateBuild.EdtFtpNet.Net.Ftp
{
    internal class FTPSemaphore
    {
        private object syncLock = new object();
        private long count = 0;
        private int waitCount = 0;

        internal FTPSemaphore(int initCount)
        {
            this.count = initCount;
        }

        internal long Count
        {
            get { return this.count; }
            set 
            {
                if (value<0)
                    throw new ArgumentOutOfRangeException("count must be non-negative");
                if (value==this.count)
                    return;
                lock (this.syncLock)
                {
                    if (this.count < value)
                        for (; this.count < value; this.count++)
                            Monitor.Pulse(this.syncLock);
                    else // count > value
                        this.count = value;
                }
            }
        }

        internal int NumWaiting
        {
            get { return this.waitCount; }
        }

        internal bool WaitOne(int timeoutMillis)
        {
            lock (this.syncLock)
            {
                bool acquiredLock = false;
                if (this.count > 0)
                    acquiredLock = true;
                else
                    try
                    {
                        this.waitCount++;
                        if (timeoutMillis <= 0)
                        {
                            Monitor.Wait(this.syncLock);
                            acquiredLock = true;
                        }
                        else if (Monitor.Wait(this.syncLock, timeoutMillis))
                            acquiredLock = true;
                    }
                    finally
                    {
                        this.waitCount--;
                    }
                if (acquiredLock)
                    this.count--;
                return acquiredLock;
            }
        }

        internal void Release()
        {
            lock (this.syncLock)
            {
                this.count++;
                Monitor.Pulse(this.syncLock);
            }
        }
    }
}
