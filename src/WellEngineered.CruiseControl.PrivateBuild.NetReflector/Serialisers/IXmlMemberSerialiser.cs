using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Util;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector.Serialisers
{
	public interface IXmlMemberSerialiser : IXmlSerialiser
	{
		ReflectorPropertyAttribute Attribute { get; }
		ReflectorMember ReflectorMember { get; }
		void SetValue(object instance, object value);
	}
}