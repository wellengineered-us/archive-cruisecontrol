using System;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector.Util
{
	public interface IInstantiator
	{
		object Instantiate(Type type);
	}
}
