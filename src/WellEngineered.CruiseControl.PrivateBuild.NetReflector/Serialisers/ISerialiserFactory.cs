using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Util;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector.Serialisers
{
	public interface ISerialiserFactory
	{
		IXmlMemberSerialiser Create(ReflectorMember memberInfo, ReflectorPropertyAttribute attribute);
	}
}