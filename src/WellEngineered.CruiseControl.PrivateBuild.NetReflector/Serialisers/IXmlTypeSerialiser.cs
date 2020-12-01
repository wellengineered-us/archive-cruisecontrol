using System;
using System.Collections;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector.Serialisers
{
	public interface IXmlTypeSerialiser : IXmlSerialiser
	{
		Type Type { get; }
		ReflectorTypeAttribute Attribute { get; }
		IEnumerable MemberSerialisers { get; }
	}
}
