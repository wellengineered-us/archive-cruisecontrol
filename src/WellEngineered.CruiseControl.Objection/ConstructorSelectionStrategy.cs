using System;
using System.Reflection;

namespace WellEngineered.CruiseControl.Objection
{
	public interface ConstructorSelectionStrategy
	{
		ConstructorInfo GetConstructor(Type type);
	}
}
