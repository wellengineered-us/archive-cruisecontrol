using System;

namespace WellEngineered.CruiseControl.Objection
{
	public interface DecoratableByType
	{
		DecoratableByType Decorate(Type type);
	}
}
