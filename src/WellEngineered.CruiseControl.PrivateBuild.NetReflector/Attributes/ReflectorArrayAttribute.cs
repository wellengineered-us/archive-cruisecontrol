using System;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public sealed class ReflectorArrayAttribute : ReflectorPropertyAttribute
	{
		public ReflectorArrayAttribute(string name) : base(name)
		{}
	}
}