using System;

using WellEngineered.CruiseControl.Core.Util;

namespace WellEngineered.CruiseControl.WebDashboard.IO
{
    public class ConditionalGetFingerprint
    {
        public readonly static ConditionalGetFingerprint NOT_AVAILABLE = new ConditionalGetFingerprint(DateTime.MinValue, "\"NOT AVAILABLE\"");

        private readonly DateTime lastModifiedTime;
        private readonly string eTag;

        public ConditionalGetFingerprint(DateTime ifModifiedSince, string ifNoneMatch)
        {
            this.lastModifiedTime = ifModifiedSince;
            this.eTag = ifNoneMatch;
        }

        public ConditionalGetFingerprint Combine(ConditionalGetFingerprint other)
        {
            if (this == NOT_AVAILABLE || other == NOT_AVAILABLE) return NOT_AVAILABLE;
            if (this.eTag != other.eTag) throw new UncombinableFingerprintException(this.eTag, other.eTag);

            DateTime newerModificationTime = DateUtil.MaxDate(this.lastModifiedTime, other.lastModifiedTime);
            return new ConditionalGetFingerprint(newerModificationTime, this.eTag);
        }

        public override bool Equals(object obj)
        {
            if (this == NOT_AVAILABLE || obj == NOT_AVAILABLE) return false;
            if (this == obj) return true;
            ConditionalGetFingerprint conditionalGetFingerprint = obj as ConditionalGetFingerprint;
            if (conditionalGetFingerprint == null) return false;
        	
            return 
                this.lastModifiedTime.Equals(conditionalGetFingerprint.lastModifiedTime) &&
                this.eTag.Equals(conditionalGetFingerprint.eTag);
        }

        public override int GetHashCode()
        {
            return this.lastModifiedTime.GetHashCode() + 29*this.eTag.GetHashCode();
        }

        public DateTime LastModifiedTime
        {
            get { return this.lastModifiedTime; }
        }

        public string ETag
        {
            get { return this.eTag; }
        }
    }
}