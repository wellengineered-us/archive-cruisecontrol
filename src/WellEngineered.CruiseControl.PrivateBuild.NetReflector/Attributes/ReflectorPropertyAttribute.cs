using System;
using System.Reflection;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Serialisers;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Util;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class 
		ReflectorPropertyAttribute : Attribute, IReflectorAttribute
	{
		private string name;
		private string description;
		private bool required = true;
		private Type instanceType;
		private string instanceTypeKey;
		private ISerialiserFactory factory = new DefaultSerialiserFactory();

		public ReflectorPropertyAttribute(string name)
		{
			this.name = name;
		}

		public ReflectorPropertyAttribute(string name, Type factoryType) : this(name)
		{
			this.name = name;
            this.HasCustomFactory = true;
			this.factory = (ISerialiserFactory) Activator.CreateInstance(factoryType);
		}

        public bool HasCustomFactory { get; private set; }

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

		public bool Required
		{
			get { return this.required; }
			set { this.required = value; }
		}

		public Type InstanceType
		{
			get { return this.instanceType; }
			set { this.instanceType = value; }
		}

		public string InstanceTypeKey
		{
			get { return this.instanceTypeKey; }
			set { this.instanceTypeKey = value; }
		}

		public virtual IXmlSerialiser CreateSerialiser(ReflectorMember member)
		{
			return this.factory.Create(member, this); 
		}

		public static ReflectorPropertyAttribute GetAttribute(MemberInfo member)
		{
			object[] attributes = member.GetCustomAttributes(typeof(ReflectorPropertyAttribute), false);
			return (attributes.Length == 0) ? null : (ReflectorPropertyAttribute)attributes[0];
		}
	}
}