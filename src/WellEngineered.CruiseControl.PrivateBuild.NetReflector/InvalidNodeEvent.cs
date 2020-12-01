using System;
using System.Xml;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector
{
	public delegate void InvalidNodeEventHandler(InvalidNodeEventArgs args);

	public class InvalidNodeEventArgs : EventArgs
	{
		public readonly XmlNode Node;
		public readonly string Message;

		public InvalidNodeEventArgs(XmlNode node, string message)
		{
			this.Node = node;
			this.Message = message;
		}

		public override string ToString()
		{
			return string.Format("{0}.  Node={1}", this.Message, this.Node.OuterXml);
		}
	}
}