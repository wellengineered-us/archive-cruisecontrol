using System;

namespace WellEngineered.CruiseControl.Objection
{
	public interface ObjectSource
	{
		object GetByType(Type type);
		object GetByName(string name);
	}
}
