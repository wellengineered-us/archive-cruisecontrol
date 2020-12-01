using System;

namespace WellEngineered.CruiseControl.Objection
{
	public class ObjectionType : DecoratableByType
	{
		private readonly Type type;
		private ObjectionType decorator;

		public ObjectionType(Type type)
		{
			this.type = type;
		}

		public DecoratableByType Decorate(Type type)
		{
			this.decorator = new ObjectionType(type);
			return this.decorator;
		}

		public Type Type
		{
			get { return this.type; }
		}

		public ObjectionType Decorator
		{
			get { return this.decorator; }
		}
	}
}
