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

using System.Collections.Generic;

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.App.Event;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Resource;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Util.Introspection;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Context
{
	/// <summary> This is an abstract internal-use-only context implementation to be
    /// used as a subclass for other internal-use-only contexts that wrap
    /// other internal-use-only contexts.
    /// 
    /// We use this context to make it easier to chain an existing context
    /// as part of a new context implementation.  It just delegates everything
    /// to the inner/parent context. Subclasses then only need to override
    /// the methods relevant to them.
    /// 
    /// </summary>
    /// <author>  Nathan Bubna
    /// </author>
    /// <version>  $Id: ChainedInternalContextAdapter.java 685724 2008-08-13 23:12:12Z nbubna $
    /// </version>
    /// <since> 1.6
    /// </since>
    public abstract class ChainedInternalContextAdapter : IInternalContextAdapter
    {
        /// <summary> Return the inner / user context.</summary>
        /// <returns> The inner / user context.
        /// </returns>
        virtual public IContext InternalUserContext
        {
            get
            {
                return this.innerContext.InternalUserContext;
            }

        }
        /// <seealso cref="org.apache.velocity.context.InternalWrapperContext.getBaseContext()">
        /// </seealso>
        virtual public IInternalContextAdapter BaseContext
        {
            get
            {
                return this.innerContext.BaseContext;
            }

        }
        /// <seealso cref="org.apache.velocity.context.Context.getKeys()">
        /// </seealso>
        virtual public object[] Keys
        {
            get
            {
                return this.innerContext.Keys;
            }

        }
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.getCurrentTemplateName()">
        /// </seealso>
        virtual public string CurrentTemplateName
        {
            get
            {
                return this.innerContext.CurrentTemplateName;
            }

        }
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.getTemplateNameStack()">
        /// </seealso>
        virtual public object[] TemplateNameStack
        {
            get
            {
                return this.innerContext.TemplateNameStack;
            }

        }
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.getCurrentMacroName()">
        /// </seealso>
        virtual public string CurrentMacroName
        {
            get
            {
                return this.innerContext.CurrentMacroName;
            }

        }
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.getCurrentMacroCallDepth()">
        /// </seealso>
        virtual public int CurrentMacroCallDepth
        {
            get
            {
                return this.innerContext.CurrentMacroCallDepth;
            }

        }
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.getMacroNameStack()">
        /// </seealso>
        virtual public object[] MacroNameStack
        {
            get
            {
                return this.innerContext.MacroNameStack;
            }

        }

        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.getAllowRendering()">
        /// </seealso>
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.setAllowRendering(boolean)">
        /// </seealso>
        virtual public bool AllowRendering
        {
            get
            {
                return this.innerContext.AllowRendering;
            }

            set
            {
                this.innerContext.AllowRendering = value;
            }

        }

        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.getMacroLibraries()">
        /// </seealso>
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.setMacroLibraries(List{T})">
        /// </seealso>
        virtual public System.Collections.IList MacroLibraries
        {
            get
            {
                return this.innerContext.MacroLibraries;
            }

            set
            {
                this.innerContext.MacroLibraries = value;
            }

        }
        /// <seealso cref="org.apache.velocity.context.InternalEventContext.getEventCartridge()">
        /// </seealso>
        virtual public EventCartridge EventCartridge
        {
            get
            {
                return this.innerContext.EventCartridge;
            }

        }

        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.getCurrentResource()">
        /// </seealso>
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.setCurrentResource(Resource)">
        /// </seealso>
        virtual public Resource CurrentResource
        {
            get
            {
                return this.innerContext.CurrentResource;
            }

            set
            {
                this.innerContext.CurrentResource = value;
            }

        }
        /// <summary>the parent context </summary>
        protected internal IInternalContextAdapter innerContext = null;

        /// <summary> CTOR, wraps an ICA</summary>
        /// <param name="inner">context
        /// </param>
        public ChainedInternalContextAdapter(IInternalContextAdapter inner)
        {
            this.innerContext = inner;
        }

        /// <summary> Retrieves from parent context.
        /// 
        /// </summary>
        /// <param name="key">name of item to Get
        /// </param>
        /// <returns>  stored object or null
        /// </returns>
        public virtual object Get(string key)
        {
            return this.innerContext.Get(key);
        }

        /// <summary> Put method also stores values in parent context
        /// 
        /// </summary>
        /// <param name="key">name of item to set
        /// </param>
        /// <param name="value">object to set to key
        /// </param>
        /// <returns> old stored object
        /// </returns>
        public virtual object Put(string key, object value)
        {
            /*
            * just Put in the local context
            */
            return this.innerContext.Put(key, value);
        }

        /// <seealso cref="org.apache.velocity.context.Context.containsKey(object)">
        /// </seealso>
        public virtual bool ContainsKey(object key)
        {
            return this.innerContext.ContainsKey(key);
        }

        /// <seealso cref="org.apache.velocity.context.Context.remove(object)">
        /// </seealso>
        public virtual object Remove(object key)
        {
            return this.innerContext.Remove(key);
        }

        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.pushCurrentTemplateName(string)">
        /// </seealso>
        public virtual void PushCurrentTemplateName(string s)
        {
            this.innerContext.PushCurrentTemplateName(s);
        }

        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.popCurrentTemplateName()">
        /// </seealso>
        public virtual void PopCurrentTemplateName()
        {
            this.innerContext.PopCurrentTemplateName();
        }

        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.pushCurrentMacroName(string)">
        /// </seealso>
        public virtual void PushCurrentMacroName(string s)
        {
            this.innerContext.PushCurrentMacroName(s);
        }

        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.popCurrentMacroName()">
        /// </seealso>
        public virtual void PopCurrentMacroName()
        {
            this.innerContext.PopCurrentMacroName();
        }

        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.icacheGet(object)">
        /// </seealso>
        public virtual IntrospectionCacheData ICacheGet(object key)
        {
            return this.innerContext.ICacheGet(key);
        }

        /// <seealso cref="org.apache.velocity.context.InternalWrapperContext.localPut(string,object)">
        /// </seealso>
        public virtual object LocalPut(string key, object value)
        {
            return this.innerContext.Put(key, value);
        }

        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.icachePut(object, IntrospectionCacheData)">
        /// </seealso>
        public virtual void ICachePut(object key, IntrospectionCacheData o)
        {
            this.innerContext.ICachePut(key, o);
        }

        /// <seealso cref="org.apache.velocity.context.InternalEventContext.attachEventCartridge(org.apache.velocity.app.event.EventCartridge)">
        /// </seealso>
        public virtual EventCartridge AttachEventCartridge(EventCartridge ec)
        {
            return this.innerContext.AttachEventCartridge(ec);
        }
    }
}
