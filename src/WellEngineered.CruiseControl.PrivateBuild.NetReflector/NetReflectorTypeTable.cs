using System;
using System.Collections;
using System.IO;
using System.Reflection;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Serialisers;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Util;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector
{
	public class NetReflectorTypeTable : IEnumerable
	{
		public event InvalidNodeEventHandler InvalidNode;
		private readonly IInstantiator instantiator;
		private readonly IDictionary reflectorTypes = new Hashtable();

		public NetReflectorTypeTable() : this(new DefaultInstantiator())
		{}

		public NetReflectorTypeTable(IInstantiator instantiator)
		{
			this.instantiator = instantiator;
			this.InvalidNode = new InvalidNodeEventHandler(this.NullHandler);
		}

		public int Count
		{
			get { return this.reflectorTypes.Count; }
		}

		public IXmlTypeSerialiser this[string reflectorName]
		{
			get { return (IXmlTypeSerialiser) this.reflectorTypes[reflectorName]; }
		}

        public bool ContainsType(string reflectorName)
        {
            return this.reflectorTypes.Contains(reflectorName);
        }

		public void Add(Type type)
		{
			ReflectorTypeAttribute attribute = ReflectorTypeAttribute.GetAttribute(type);
			if (attribute == null) return;

			if (! this.reflectorTypes.Contains(attribute.Name))
			{
				IXmlSerialiser serialiser = attribute.CreateSerialiser(type, this.instantiator);
				this.reflectorTypes.Add(attribute.Name, serialiser);
			}
			else if (type != this[attribute.Name].Type)
			{
				throw new NetReflectorException(string.Format(@"Multiple types exist with the same ReflectorTypeAttribute name ""{0}"": ({1}, {2})",
				                                              attribute.Name, type, this[attribute.Name].Type));
			}
		}

		public void Add(Assembly assembly)
		{
			foreach (Type type in this.GetTypes(assembly))
			{
				this.Add(type);
			}
		}

		private Type[] GetTypes(Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				throw new NetReflectorTypeLoadException(assembly, e);
			}
		}

		public void Add(AppDomain appDomain)
		{
			foreach (Assembly assembly in appDomain.GetAssemblies())
			{
				this.Add(assembly);
			}
		}

		public void Add(string assemblyFilename)
		{
			this.Add(Assembly.LoadFrom(assemblyFilename));
		}

		public void Add(string path, string searchPattern)
		{
			foreach (string filename in Directory.GetFiles(path, searchPattern))
			{
				this.Add(filename);
			}
		}

		public static NetReflectorTypeTable CreateDefault()
		{
			NetReflectorTypeTable table = new NetReflectorTypeTable();
			SetupDefaultTable(table);
			return table;
		}

		public static NetReflectorTypeTable CreateDefault(IInstantiator instantiator)
		{
			NetReflectorTypeTable table = new NetReflectorTypeTable(instantiator);
			SetupDefaultTable(table);
			return table;
		}

		private static void SetupDefaultTable(NetReflectorTypeTable table)
		{
			table.Add(AppDomain.CurrentDomain);
		}

		public IEnumerator GetEnumerator()
		{
			return this.reflectorTypes.Values.GetEnumerator();
		}

		public void OnInvalidNode(InvalidNodeEventArgs args)
		{
			this.InvalidNode(args);
		}

		private void NullHandler(InvalidNodeEventArgs args)
		{}
	}
}