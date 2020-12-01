using System;

namespace WellEngineered.CruiseControl.Objection
{
	public interface ImplementationResolver
	{
		Type ResolveImplementation(Type baseType);
	}
}
