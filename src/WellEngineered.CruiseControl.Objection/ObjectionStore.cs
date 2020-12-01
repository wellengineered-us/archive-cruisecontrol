using System;
using System.Collections;
using System.Reflection;

namespace WellEngineered.CruiseControl.Objection
{
	public class ObjectionStore : ObjectSource, ObjectionManager
	{
		private ImplementationResolver implementationResolver;
		private ConstructorSelectionStrategy constructorSelectionStrategy;
		private Hashtable typedInstances;
		private Hashtable implementationTypes;
		private Hashtable dependencyImplementations;
		private Hashtable namedTypes;
		private Hashtable dependencyImplementationsForNames;
		private Hashtable namedInstances;

		public ObjectionStore() : this (
			new NMockAwareImplementationResolver(),
			new MaxLengthConstructorSelectionStrategy()) 
		{ }

		public ObjectionStore(ImplementationResolver implementationResolver, ConstructorSelectionStrategy constructorSelectionStrategy)
		{
			this.implementationResolver = implementationResolver;
			this.constructorSelectionStrategy = constructorSelectionStrategy;
			this.typedInstances = new Hashtable();
			this.implementationTypes = new Hashtable();
			this.dependencyImplementations = new Hashtable();
			this.namedTypes = new Hashtable();
			this.dependencyImplementationsForNames = new Hashtable();
			this.namedInstances = new Hashtable();
			this.AddInstanceForType(typeof(ObjectSource), this);
			this.AddInstanceForType(typeof(ObjectionManager), this);
			this.AddInstanceForType(typeof(ObjectionStore), this);
		}

		public void AddInstanceForType(Type type, object instance)
		{
			this.typedInstances[type] = instance;
		}

		public void AddInstance(params object[] objects)
		{
			foreach (object o in objects)
			{
				this.AddInstanceForType(o.GetType(), o);
			}
		}

		public void SetImplementationType(Type parentType, Type implementationType)
		{
			this.implementationTypes[parentType] = implementationType;
		}

		public DecoratableByType AddTypeForName(string name, Type type)
		{
			this.namedTypes[name] = new ObjectionType(type);
			return (DecoratableByType) this.namedTypes[name];
		}

		public DecoratableByType AddInstanceForName(string name, object instance)
		{
			this.namedInstances[name] = new ObjectionObject(instance);
			return (DecoratableByType) this.namedInstances[name];
		}

		public void SetDependencyImplementationForName(string name, Type dependencyType, Type implementationType)
		{
			if (this.dependencyImplementationsForNames[name] == null)
				this.dependencyImplementationsForNames[name] = new Hashtable();

			((Hashtable) this.dependencyImplementationsForNames[name])[dependencyType] = implementationType;
		}

		public void SetDependencyImplementationForType(Type typeWithDependency, Type dependencyParentType, Type dependencyImplementationType)
		{
			if (this.dependencyImplementations[typeWithDependency] == null)
				this.dependencyImplementations[typeWithDependency] = new Hashtable();

			((Hashtable) this.dependencyImplementations[typeWithDependency])[dependencyParentType] = dependencyImplementationType;
		}

		public object GetByType(Type type)
		{
			return this.GiveObjectByType(type, null);
		}

		public object GetByName(string name)
		{
			ObjectionObject namedInstance = this.namedInstances[name] as ObjectionObject;
			if (namedInstance == null)
			{
				ObjectionType namedType = (ObjectionType) this.namedTypes[name];
			
				if (namedType == null)
					throw new ApplicationException("Unknown object name : " + name);
				else
					return this.Instantiate(namedType, name);
			}
			else
			{
				if (namedInstance.Decorator != null)
				{
					return this.Instantiate(namedInstance.Decorator, name, namedInstance.Instance);
				}
				else
				{
					return namedInstance.Instance;
				}
			}
		}

		private object GiveObjectByType(Type type, string name)
		{
			object result = this.typedInstances[type];
			if (result == null)
			{
				result = this.Instantiate(type, name);
			}
			return result;
		}

		private object Instantiate(ObjectionType ObjectionType, string id, params object[] args)
		{
			object baseObject = this.Instantiate(ObjectionType.Type, id, args);

			ObjectionType decorator = ObjectionType.Decorator;
			if (decorator != null)
				return this.Instantiate(decorator, id, baseObject);
			else
				return baseObject;
		}

		private object Instantiate(Type type, string identifier, params object[] args)
		{
			if (this.implementationTypes[type] != null)
			{
				return this.Instantiate((Type) this.implementationTypes[type], identifier, args);
			}
			if (type.IsInterface)
			{
				this.implementationTypes[type] = this.implementationResolver.ResolveImplementation(type);
				return this.Instantiate((Type) this.implementationTypes[type], identifier, args);
			}

			ArrayList arguments = new ArrayList();
			ConstructorInfo constructor = this.constructorSelectionStrategy.GetConstructor(type);

			if (constructor == null)
			{
				throw new Exception("Unable to select constructor for Type " + type.FullName);
			}

			int paramCount = 0;
			foreach (ParameterInfo parameter in constructor.GetParameters())
			{
				if (paramCount < args.Length)
				{
					arguments.Add(args[paramCount]);
				}
				else
				{
					try
					{
						Type dependencyType = this.OverrideWithSpecifiedDependencyImplementationIfNecessary(type, parameter.ParameterType, identifier);
						arguments.Add(this.GiveObjectByType(dependencyType, identifier));
					}
					catch (Exception e)
					{
						throw new Exception("Failed to instantiate Type " + type.FullName, e);
					}
				}
				paramCount++;
			}

			try
			{
				return constructor.Invoke((object[]) arguments.ToArray(typeof (object)));	
			}
			catch (Exception e)
			{
				string message = "Unable to instante " + type.FullName + " using the following args: ";
				foreach (object arg in arguments)
				{
					message += arg.ToString();
					message += ", ";
				}
				throw new Exception(message, e);
			}			
		}

		private Type OverrideWithSpecifiedDependencyImplementationIfNecessary(Type typeBeingInstantiated, Type staticDependencyType, string identifier)
		{
			Hashtable dependencyTypes;
			if (identifier != null)
			{
				dependencyTypes = (Hashtable) this.dependencyImplementationsForNames[identifier];
				if (dependencyTypes != null && dependencyTypes[staticDependencyType] != null)
				{
					return (Type) dependencyTypes[staticDependencyType];
				}			
			}
			dependencyTypes = (Hashtable) this.dependencyImplementations[typeBeingInstantiated];
			if (dependencyTypes != null && dependencyTypes[staticDependencyType] != null)
			{
				return (Type) dependencyTypes[staticDependencyType];
			}
			return staticDependencyType;
		}
	}
}
