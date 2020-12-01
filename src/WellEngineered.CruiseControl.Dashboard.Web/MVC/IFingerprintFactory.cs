using System;

using WellEngineered.CruiseControl.WebDashboard.IO;

namespace WellEngineered.CruiseControl.WebDashboard.MVC
{
    public interface IFingerprintFactory
    {
        ConditionalGetFingerprint BuildFromRequest(IRequest request);
        ConditionalGetFingerprint BuildFromFileNames(params string[] filenames);
        ConditionalGetFingerprint BuildFromDate(DateTime date);
    }
}