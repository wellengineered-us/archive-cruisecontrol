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
using System.Collections.Specialized;

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.App.Event;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Resource;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Util.Introspection;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Context
{
	/// <summary>  class to encapsulate the 'stuff' for internal operation of velocity.
    /// We use the context as a thread-safe storage : we take advantage of the
    /// fact that it's a visitor  of sorts  to all nodes (that matter) of the
    /// AST during Init() and render().
    /// Currently, it carries the template name for namespace
    /// support, as well as node-local context data introspection caching.
    /// 
    /// Note that this is not a public class.  It is for package access only to
    /// keep application code from accessing the internals, as AbstractContext
    /// is derived from this.
    /// 
    /// </summary>
    /// <author>  <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
    /// </author>
    /// <version>  $Id: InternalContextBase.java 679861 2008-07-25 17:17:50Z nbubna $
    /// </version>
    public class InternalContextBase : IInternalHousekeepingContext, IInternalEventContext
    {
        /// <summary>  Get the current template name
        /// 
        /// </summary>
        /// <returns> String current template name
        /// </returns>
        virtual public string CurrentTemplateName
        {
            get
            {
                if ((this.templateNameStack.Count == 0))
                    return "<undef>";
                else
                {
                    return this.templateNameStack.Peek();
                }
            }

        }
        /// <summary>  Get the current template name stack
        /// 
        /// </summary>
        /// <returns> Object[] with the template name stack contents.
        /// </returns>
        virtual public object[] TemplateNameStack
        {
            get
            {
                return this.templateNameStack.ToArray();
            }

        }
        /// <summary>  Get the current macro name
        /// 
        /// </summary>
        /// <returns> String current macro name
        /// </returns>
        virtual public string CurrentMacroName
        {
            get
            {
                if ((this.macroNameStack.Count == 0))
                {
                    return "<undef>";
                }
                else
                {
                    return (string)this.macroNameStack.Peek();
                }
            }

        }
        /// <summary>  Get the current macro call depth
        /// 
        /// </summary>
        /// <returns> int current macro call depth
        /// </returns>
        virtual public int CurrentMacroCallDepth
        {
            get
            {
                return this.macroNameStack.Count;
            }

        }
        /// <summary>  Get the current macro name stack
        /// 
        /// </summary>
        /// <returns> Object[] with the macro name stack contents.
        /// </returns>
        virtual public object[] MacroNameStack
        {
            get
            {
                return this.macroNameStack.ToArray();
            }

        }
        //UPGRADE_NOTE: ��Ӧ�� javadoc ע�ͱ��ϲ���Ӧ������и����Է��� .NET �ĵ�Լ���� "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.getCurrentResource()">
        /// </seealso>
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.setCurrentResource(Resource)">
        /// </seealso>
        virtual public Resource CurrentResource
        {
            get
            {
                return this.currentResource;
            }

            set
            {
                this.currentResource = value;
            }

        }
        //UPGRADE_NOTE: ��Ӧ�� javadoc ע�ͱ��ϲ���Ӧ������и����Է��� .NET �ĵ�Լ���� "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.getAllowRendering()">
        /// </seealso>
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.setAllowRendering(boolean)">
        /// </seealso>
        virtual public bool AllowRendering
        {
            get
            {
                return this.allowRendering;
            }

            set
            {
                this.allowRendering = value;
            }

        }
        //UPGRADE_NOTE: ��Ӧ�� javadoc ע�ͱ��ϲ���Ӧ������и����Է��� .NET �ĵ�Լ���� "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.getMacroLibraries()">
        /// </seealso>
        /// <seealso cref="org.apache.velocity.context.InternalHousekeepingContext.setMacroLibraries(List)">
        /// </seealso>
        virtual public System.Collections.IList MacroLibraries
        {
            get
            {
                return this.macroLibraries;
            }

            set
            {
                this.macroLibraries = value;
            }

        }
        /// <seealso cref="org.apache.velocity.context.InternalEventContext.getEventCartridge()">
        /// </seealso>
        virtual public EventCartridge EventCartridge
        {
            get
            {
                return this.eventCartridge;
            }

        }

        /// <summary>  cache for node/context specific introspection information</summary>
        //UPGRADE_TODO: Class��java.util.HashMap����ת��Ϊ���в�ͬ��Ϊ�� 'System.Collections.Hashtable'�� "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilHashMap'"
        private HybridDictionary introspectionCache = new HybridDictionary(33);

        /// <summary>  Template name stack. The stack top contains the current template name.</summary>
        private System.Collections.Generic.Stack<string> templateNameStack = new Stack<string>();

        /// <summary>  Velocimacro name stack. The stack top contains the current macro name.</summary>
        private System.Collections.Generic.Stack<string> macroNameStack = new System.Collections.Generic.Stack<string>();

        /// <summary>  EventCartridge we are to carry.  Set by application</summary>
        private EventCartridge eventCartridge = null;

        /// <summary>  Current resource - used for carrying encoding and other
        /// information down into the rendering process
        /// </summary>
        private Resource currentResource = null;

        /// <summary>  Is rendering allowed?  Defaults to true, can be changed by #stop directive.</summary>
        private bool allowRendering = true;

        /// <summary>  List for holding the macro libraries. Contains the macro library
        /// template name as strings.
        /// </summary>
        private System.Collections.IList macroLibraries = null;

        /// <summary>  set the current template name on top of stack
        /// 
        /// </summary>
        /// <param name="s">current template name
        /// </param>
        public virtual void PushCurrentTemplateName(string s)
        {
            this.templateNameStack.Push(s);
        }

        /// <summary>  remove the current template name from stack</summary>
        public virtual void PopCurrentTemplateName()
        {
            this.templateNameStack.Pop();
        }

        /// <summary>  set the current macro name on top of stack
        /// 
        /// </summary>
        /// <param name="s">current macro name
        /// </param>
        public virtual void PushCurrentMacroName(string s)
        {
            this.macroNameStack.Push(s);
        }

        /// <summary>  remove the current macro name from stack</summary>
        public virtual void PopCurrentMacroName()
        {
            this.macroNameStack.Pop();
        }

        /// <seealso cref="IntrospectionCacheData)">
        /// object if exists for the key
        /// 
        /// </seealso>
        /// <param name="key"> key to find in cache
        /// </param>
        /// <returns> cache object
        /// </returns>
        public virtual IntrospectionCacheData ICacheGet(object key)
        {
            //UPGRADE_TODO: ������java.util.HashMap.Get����ת��Ϊ���в�ͬ��Ϊ�� 'System.Collections.Hashtable.Item'�� "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilHashMapget_javalangObject'"
            return (IntrospectionCacheData)this.introspectionCache[key];
        }

        /// <seealso cref="IntrospectionCacheData)">
        /// element in the cache for specified key
        /// 
        /// </seealso>
        /// <param name="key"> key
        /// </param>
        /// <param name="instance"> IntrospectionCacheData object to place in cache
        /// </param>
        public virtual void ICachePut(object key, IntrospectionCacheData o)
        {
            this.introspectionCache[key] = o;
        }


        /// <seealso cref="org.apache.velocity.context.InternalEventContext.attachEventCartridge(org.apache.velocity.app.event.EventCartridge)">
        /// </seealso>
        public virtual EventCartridge AttachEventCartridge(EventCartridge ec)
        {
            EventCartridge temp = this.eventCartridge;

            this.eventCartridge = ec;

            return temp;
        }
    }
}