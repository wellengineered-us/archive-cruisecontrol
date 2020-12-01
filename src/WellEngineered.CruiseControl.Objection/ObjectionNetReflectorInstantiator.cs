using System;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Util;

namespace WellEngineered.CruiseControl.Objection
{
	public class ObjectionNetReflectorInstantiator : IInstantiator
	{
		private readonly ObjectSource objectSource;

		public ObjectionNetReflectorInstantiator(ObjectSource objectSource)
		{
			this.objectSource = objectSource;
		}

		public object Instantiate(Type type)
		{
			return this.objectSource.GetByType(type);
		}
	}
}
