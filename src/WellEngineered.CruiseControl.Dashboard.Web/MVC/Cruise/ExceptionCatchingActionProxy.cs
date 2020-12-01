using System;
using System.Collections;

using WellEngineered.CruiseControl.WebDashboard.MVC.View;

namespace WellEngineered.CruiseControl.WebDashboard.MVC.Cruise
{
	public class ExceptionCatchingActionProxy : IAction
	{
		private readonly IAction proxiedAction;
		private readonly IVelocityViewGenerator velocityViewGenerator;

		public ExceptionCatchingActionProxy(IAction proxiedAction, IVelocityViewGenerator velocityViewGenerator)
		{
			this.proxiedAction = proxiedAction;
			this.velocityViewGenerator = velocityViewGenerator;
		}

		public IResponse Execute(IRequest request)
		{
			try
			{
				return this.proxiedAction.Execute(request);	
			}
			catch (Exception e)
			{
				Hashtable velocityContext = new Hashtable();
				velocityContext["exception"] = e;
				return this.velocityViewGenerator.GenerateView(@"ActionException.vm", velocityContext);
			}
		}
	}

}
