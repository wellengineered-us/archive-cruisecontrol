using System;
using System.Collections;
using System.Xml;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector.Util
{
	public class XmlElementList : XmlNodeList
	{
		private ArrayList list = new ArrayList();

		public XmlElementList(XmlNodeList nodes)
		{
			this.FillList(nodes);
		}

		private void FillList(XmlNodeList nodes)
		{
			foreach (XmlNode node in nodes)
			{
				if (node.NodeType == XmlNodeType.Element)
				{
					this.list.Add(node);
				}
			}
		}

		public override int Count
		{
			get { return this.list.Count; }
		}

		public override IEnumerator GetEnumerator()
		{
			return this.list.GetEnumerator();
		}

		public override XmlNode Item(int index)
		{
			return (XmlNode)this.list[index];
		}

		public static XmlElementList Create(XmlNodeList nodes)
		{
			return new XmlElementList(nodes);
		}
	}
}
