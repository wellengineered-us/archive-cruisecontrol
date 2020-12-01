using WellEngineered.CruiseControl.WebDashboard.MVC;

namespace WellEngineered.CruiseControl.WebDashboard.IO
{
    public interface IConditionalGetFingerprintProvider
    {
        ConditionalGetFingerprint GetFingerprint(IRequest request);
    }
}