using System;

using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface IActionInstantiator
	{
		ICruiseAction InstantiateAction(Type actionType);
	}
}