/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements.  See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership.  The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License.  You may obtain a copy of the License at
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing,
* software distributed under the License is distributed on an
* "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
* KIND, either express or implied.  See the License for the
* specific language governing permissions and limitations
* under the License.    
*/

using System.Collections;

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Commons.Collections;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Resource
{
	/// <summary> Default implementation of the resource cache for the default
    /// ResourceManager.  The cache uses a <i>least recently used</i> (LRU)
    /// algorithm, with a maximum size specified via the
    /// <code>resource.manager.cache.size</code> property (idenfied by the
    /// {@link
    /// org.apache.velocity.runtime.RuntimeConstants#RESOURCE_MANAGER_DEFAULTCACHE_SIZE}
    /// constant).  This property Get be set to <code>0</code> or less for
    /// a greedy, unbounded cache (the behavior from pre-v1.5).
    /// 
    /// </summary>
    /// <author>  <a href="mailto:geirm@apache.org">Geir Magnusson Jr.</a>
    /// </author>
    /// <author>  <a href="mailto:dlr@finemaltcoding.com">Daniel Rall</a>
    /// </author>
    /// <version>  $Id: ResourceCacheImpl.java 685385 2008-08-12 23:59:06Z nbubna $
    /// </version>
    public class ResourceCacheImpl : IResourceCache
    {
        public ResourceCacheImpl()
        {
            this.InitBlock();
        }
        private void InitBlock()
        {
            this.cache = Hashtable.Synchronized(new Hashtable(512, 0.5f));
        }
        /// <summary> Cache storage, assumed to be thread-safe.</summary>
        protected internal System.Collections.IDictionary cache;

        /// <summary> Runtime services, generally initialized by the
        /// <code>Initialize()</code> method.
        /// </summary>
        protected internal IRuntimeServices rsvc = null;

        /// <seealso cref="org.apache.velocity.runtime.resource.ResourceCache.Initialize(org.apache.velocity.runtime.RuntimeServices)">
        /// </seealso>
        public virtual void Initialize(IRuntimeServices rs)
        {
            this.rsvc = rs;

            int maxSize = this.rsvc.GetInt(NVelocity.Runtime.RuntimeConstants.RESOURCE_MANAGER_DEFAULTCACHE_SIZE, 89);
            if (maxSize > 0)
            {
                // Create a whole new Map here to avoid hanging on to a
                // handle to the unsynch'd LRUMap for our lifetime.
                LRUMap lruCache = LRUMap.Synchronized(new LRUMap(maxSize));
                lruCache.AddAll(this.cache);
                this.cache = lruCache;
            }
            this.rsvc.Log.Debug("ResourceCache: initialized (" + this.GetType() + ") with " + this.cache.GetType() + " cache map.");
        }

        /// <seealso cref="org.apache.velocity.runtime.resource.ResourceCache.Get(object)">
        /// </seealso>
        public virtual Resource Get(object key)
        {
            return (Resource)this.cache[key];
        }

        /// <seealso cref="org.apache.velocity.runtime.resource.ResourceCache.Put(object, Resource)">
        /// </seealso>
        public virtual Resource Put(object key, Resource resource)
        {
            object tempObject = this.cache[key];
            this.cache[key] = resource;
            return (Resource)tempObject;
        }

        /// <seealso cref="org.apache.velocity.runtime.resource.ResourceCache.remove(object)">
        /// </seealso>
        public virtual Resource Remove(object key)
        {
            object tempObject = this.cache[key];
            this.cache.Remove(key);
            return (Resource)tempObject;
        }

        /// <seealso cref="org.apache.velocity.runtime.resource.ResourceCache.enumerateKeys()">
        /// </seealso>
        public virtual IEnumerator EnumerateKeys()
        {
            return this.cache.Keys.GetEnumerator();
        }
    }
}