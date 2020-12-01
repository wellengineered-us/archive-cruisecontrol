using System;
using System.Collections;
using System.Collections.Generic;

using WellEngineered.CruiseControl.Objection;
//using System.Web.Caching;
using Cache = System.Collections.IDictionary;

namespace WellEngineered.CruiseControl.WebDashboard.MVC.ASPNET
{
	public class CachedTypeMap : TypeToTypeMap
	{
		private readonly Cache cache;
		private readonly string cacheKey;

		public CachedTypeMap(Cache cache, string cacheKey)
		{
			this.cache = cache;
			this.cacheKey = cacheKey;
		}

		public CachedTypeMap(IDictionary<string, object> value)
		{
		}

		public Type this[Type baseType]
		{
			get
			{
				IDictionary cachedDictionary = this.GetCachedDictionary();
				return (Type) cachedDictionary[baseType];
			}
			set 
			{
				IDictionary cachedDictionary = this.GetCachedDictionary();
				cachedDictionary[baseType] = value;
			}
		}

		private IDictionary GetCachedDictionary()
		{
			if(this.cache[this.cacheKey] == null)
			{
				this.cache[this.cacheKey] = Hashtable.Synchronized(new Hashtable());
			}
			return (IDictionary) this.cache[this.cacheKey];
		}
	}
}