using System;

namespace WellEngineered.CruiseControl.Objection
{
	public interface TypeToTypeMap
	{
		Type this[Type baseType] {get; set;}
	}
}