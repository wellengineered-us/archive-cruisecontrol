using System;
using System.Collections;
using System.Xml;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector.Generators
{
	public class XsdTypes
	{
		private Hashtable map = new Hashtable();

		public XsdTypes()
		{
			this.map[typeof (string)] = "string";
			this.map[typeof (int)] = "integer";
			this.map[typeof (byte)] = "byte";
			this.map[typeof (decimal)] = "decimal";
			this.map[typeof (double)] = "double";
			this.map[typeof (bool)] = "boolean";
			this.map[typeof (float)] = "float";
			this.map[typeof (long)] = "long";
			this.map[typeof (short)] = "short";
			this.map[typeof (DateTime)] = "dateTime";
		}

		private string this[Type type]
		{
			get
			{
				object value = this.map[type];
				if (value == null) throw new NetReflectorException(string.Format("Unable to find Xsd type for {0}", type));
				return value.ToString();
			}
		}

		public XmlQualifiedName ConvertToSchemaTypeName(Type type)
		{
			return new XmlQualifiedName(this[type].ToString(), "http://www.w3.org/2001/XMLSchema");
		}
	}
}