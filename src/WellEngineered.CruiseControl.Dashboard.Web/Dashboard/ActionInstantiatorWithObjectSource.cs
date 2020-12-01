using System;

using WellEngineered.CruiseControl.Objection;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class ActionInstantiatorWithObjectSource : IActionInstantiator
	{
		private readonly ObjectSource objectSource;

		public ActionInstantiatorWithObjectSource(ObjectSource objectSource)
		{
			this.objectSource = objectSource;
		}

		public ICruiseAction InstantiateAction(Type actionType)
		{
			return (ICruiseAction) this.objectSource.GetByType(actionType);
		}
	}
}