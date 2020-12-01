using System;

using WellEngineered.CruiseControl.Objection;
using WellEngineered.CruiseControl.WebDashboard.Dashboard.Actions;
using WellEngineered.CruiseControl.WebDashboard.IO;

namespace WellEngineered.CruiseControl.WebDashboard.MVC.Cruise
{
    // ToDo - test untested bits!
    public class CruiseActionFactory : IActionFactory
    {
        private readonly ObjectSource objectSource;

        public CruiseActionFactory(ObjectSource objectSource)
        {
            this.objectSource = objectSource;
        }

        public IAction Create(IRequest request)
        {
            string actionName = request.FileNameWithoutExtension;

            IAction action = this.CreateHandler(actionName) as IAction;
            if (action == null)
            {
                return new UnknownActionAction();
            }
            return action;
        }


        public IConditionalGetFingerprintProvider CreateFingerprintProvider(IRequest request)
        {
            try
            {
                IConditionalGetFingerprintProvider fingerprintProvider =
                    this.CreateHandler(request.FileNameWithoutExtension + "_CONDITIONAL_GET_FINGERPRINT_CHAIN") as
                    IConditionalGetFingerprintProvider;
                return fingerprintProvider;
            }
            catch (ApplicationException)
            {
                return null;
            }
        }

        private object CreateHandler(string actionName)
        {
            var actionNameLower = actionName.ToLowerInvariant();

            // Can probably do something clever with this in CruiseObjectSourceInitialiser
            if (actionName == string.Empty || actionNameLower == "default")
            {
                return this.objectSource.GetByType(typeof (DefaultAction));
            }

            object action = this.objectSource.GetByName(actionNameLower);

            return action;
        }
    }
}