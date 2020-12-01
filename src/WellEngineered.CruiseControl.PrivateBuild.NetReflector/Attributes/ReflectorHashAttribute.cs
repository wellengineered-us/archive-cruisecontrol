using System;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Serialisers;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Util;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public sealed class ReflectorHashAttribute : ReflectorPropertyAttribute
	{
		private string key;

		public ReflectorHashAttribute(string name) : base(name)
		{}

		public ReflectorHashAttribute(string name, string key) : this(name)
		{
			this.key = key;
		}

		public string Key
		{
			get { return this.key; }
			set { this.key = value; }
		}

		public override IXmlSerialiser CreateSerialiser(ReflectorMember member)
		{
			return new XmlDictionarySerialiser(member, this);
		}
	}
}