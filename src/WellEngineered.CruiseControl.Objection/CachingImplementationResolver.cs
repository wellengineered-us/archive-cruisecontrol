using System;

namespace WellEngineered.CruiseControl.Objection
{
	public class CachingImplementationResolver : ImplementationResolver
	{
		private readonly ImplementationResolver decoratoredResolver;
		private readonly TypeToTypeMap resolvedTypeCache;

		public CachingImplementationResolver(ImplementationResolver decoratoredResolver, TypeToTypeMap resolvedTypeCache)
		{
			this.decoratoredResolver = decoratoredResolver;
			this.resolvedTypeCache = resolvedTypeCache;
		}

		public Type ResolveImplementation(Type baseType)
		{
			Type resolvedType = this.resolvedTypeCache[baseType];
			if (resolvedType == null)
			{
				resolvedType = this.AllowOneThreadPerAppDomainToDoResolution(baseType);
			}
			return resolvedType;
		}

		private Type AllowOneThreadPerAppDomainToDoResolution(Type baseType)
		{
			Type resolvedType;
			lock (baseType)
			{
				resolvedType = this.resolvedTypeCache[baseType];
				if (resolvedType == null)
				{
					resolvedType = this.decoratoredResolver.ResolveImplementation(baseType);
					this.resolvedTypeCache[baseType] = resolvedType;
				}
			}
			return resolvedType;
		}
	}
}