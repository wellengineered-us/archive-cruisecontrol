using System;

namespace WellEngineered.CruiseControl.Objection
{
	public class ObjectionObject : DecoratableByType
	{
		private readonly object instance;
		private ObjectionType decorator;

		public ObjectionObject(object instance)
		{
			this.instance = instance;
		}

		public DecoratableByType Decorate(Type type)
		{
			this.decorator = new ObjectionType(type);
			return this.decorator;
		}

		public object Instance
		{
			get { return this.instance; }
		}

		public ObjectionType Decorator
		{
			get { return this.decorator; }
		}
	}
}
	