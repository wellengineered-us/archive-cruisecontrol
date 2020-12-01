using System.Xml;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector.Serialisers
{
	public interface IXmlSerialiser
	{
		void Write(XmlWriter writer, object target);
		object Read(XmlNode node, NetReflectorTypeTable table);
	}
}