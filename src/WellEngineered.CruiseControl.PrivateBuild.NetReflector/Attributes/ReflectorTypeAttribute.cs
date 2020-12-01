using System;
using System.Xml;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Serialisers;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Util;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class ReflectorTypeAttribute : Attribute, IReflectorAttribute
	{
		private string name;
		private string description;

		public ReflectorTypeAttribute(string name)
		{
			this.name = name;
		}

		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		public string Description
		{
			get { return this.description; }
			set { this.description = value; }
		}

        public Type Extends { get; set; }

        public bool HasCustomFactory
        {
            get { return false; }
        }

		public IXmlSerialiser CreateSerialiser(Type type)
		{
			return new XmlTypeSerialiser(type, this);
		}

		public IXmlSerialiser CreateSerialiser(Type type, IInstantiator instantiator)
		{
			return new XmlTypeSerialiser(type, this, instantiator);
		}

		public void Write(XmlWriter writer, object target)
		{
			this.CreateSerialiser(target.GetType()).Write(writer, target);
		}

		public static ReflectorTypeAttribute GetAttribute(object target)
		{
			return GetAttribute(target.GetType());
		}

		public static ReflectorTypeAttribute GetAttribute(Type type)
		{
			object[] attributes = type.GetCustomAttributes(typeof(ReflectorTypeAttribute), false);
			return (attributes.Length == 0) ? null : (ReflectorTypeAttribute)attributes[0];
		}
	}
}
