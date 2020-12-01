using System;
using System.IO;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.MVC;

namespace WellEngineered.CruiseControl.WebDashboard.IO
{
    public class FingerprintFactory : IFingerprintFactory
    {
        private readonly IVersionProvider versionProvider;
        private readonly IPhysicalApplicationPathProvider pathProvider;

        public FingerprintFactory(IVersionProvider versionProvider, IPhysicalApplicationPathProvider pathProvider)
        {
            this.versionProvider = versionProvider;
            this.pathProvider = pathProvider;
        }

        public ConditionalGetFingerprint BuildFromFileNames(params string[] filenames)
        {
            DateTime newestFileDate = DateTime.MinValue;
            foreach (string filename in filenames)
            {
                string fullFilePath = this.pathProvider.GetFullPathFor(filename);
                DateTime fileModifiedDate = File.GetLastWriteTimeUtc(fullFilePath);
                if (newestFileDate < fileModifiedDate)
                {
                    newestFileDate = fileModifiedDate;
                }
            }
            return this.BuildFromDate(newestFileDate);
        }

        public ConditionalGetFingerprint BuildFromRequest(IRequest request)
        {
            if (request.IfModifiedSince == null || request.IfNoneMatch == null)
            {
                return ConditionalGetFingerprint.NOT_AVAILABLE;
            }

            DateTime ifModifiedSince = DateTime.Parse(request.IfModifiedSince).ToUniversalTime();
            string ifNoneMatch = request.IfNoneMatch;

            return new ConditionalGetFingerprint(ifModifiedSince, ifNoneMatch);
        }

        public ConditionalGetFingerprint BuildFromDate(DateTime date)
        {
            return new ConditionalGetFingerprint(date, "\"" + this.versionProvider.GetVersion() + "\"");
        }
    }
}