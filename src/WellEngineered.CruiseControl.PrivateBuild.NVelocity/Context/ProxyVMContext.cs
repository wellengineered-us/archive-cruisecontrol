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
using System.Collections.Generic;
using System.Linq;

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Exception;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser.Node;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Context
{
	/// <summary> Context for Velocity macro arguments.
    /// 
    /// This special context combines ideas of earlier VMContext and VMProxyArgs
    /// by implementing routing functionality internally. This significantly
    /// reduces memory allocation upon macro invocations.
    /// Since the macro AST is now shared and RuntimeMacro directive is used,
    /// the earlier implementation of precalculating VMProxyArgs would not work.
    /// 
    /// See <a href="http://issues.apache.org/jira/browse/VELOCITY-607">Issue 607</a>
    /// for more Info on this class.
    /// </summary>
    /// <author>  <a href="mailto:wyla@removeme.sci.fi">Jarkko Viinamaki</a>
    /// </author>
    /// <version>  $Id$
    /// </version>
    /// <since> 1.6
    /// </since>
    public class ProxyVMContext : ChainedInternalContextAdapter
    {
        /// <seealso cref="org.apache.velocity.context.Context.getKeys()">
        /// </seealso>
        public override object[] Keys
        {
            get
            {
                if ((this.localcontext.Count == 0))
                {
                    object[] vmproxyhashKeys = new object[this.vmproxyhash.Count];
                    this.vmproxyhash.Keys.CopyTo(vmproxyhashKeys, 0);

                    return vmproxyhashKeys;
                }
                else if ((this.vmproxyhash.Count == 0))
                {
                    object[] localcontextKeys = new object[this.localcontext.Count];
                    this.localcontext.Keys.CopyTo(localcontextKeys, 0);

                    return localcontextKeys;
                }

                HashSet<object> keys = new HashSet<object>();

                foreach (object o in this.localcontext.Keys)
                {
                    keys.Add(o);
                }

                foreach (object o in this.vmproxyhash.Keys)
                {
                    keys.Add(o);
                }

                return keys.ToArray();
            }

        }

        internal Hashtable vmproxyhash = new Hashtable(8, 0.8f);

        /// <summary>container for any local or constant macro arguments. Size must be power of 2. </summary>
        internal IDictionary localcontext = new Hashtable(8, 0.8f);

        /// <summary>support for local context scope feature, where all references are local </summary>
        private bool localContextScope;

        /// <summary>needed for writing Log entries. </summary>
        private IRuntimeServices rsvc;

        /// <param name="inner">Velocity context for processing
        /// </param>
        /// <param name="rsvc">RuntimeServices provides logging reference
        /// </param>
        /// <param name="localContextScope">if true, all references are set to be local
        /// </param>
        public ProxyVMContext(IInternalContextAdapter inner, IRuntimeServices rsvc, bool localContextScope)
            : base(inner)
        {

            this.localContextScope = localContextScope;
            this.rsvc = rsvc;
        }

        /// <summary> Used to Put Velocity macro arguments into this context. 
        /// 
        /// </summary>
        /// <param name="context">rendering context
        /// </param>
        /// <param name="macroArgumentName">name of the macro argument that we received
        /// </param>
        /// <param name="literalMacroArgumentName">".literal.$"+macroArgumentName
        /// </param>
        /// <param name="argumentValue">parameters value of the macro argument
        /// 
        /// </param>
        /// <throws>  MethodInvocationException </throws>
        public virtual void AddVMProxyArg(IInternalContextAdapter context, string macroArgumentName, string literalMacroArgumentName, INode argumentValue)
        {
            if (this.IsConstant(argumentValue))
            {
                this.localcontext[macroArgumentName] = argumentValue.Value(context);
            }
            else
            {
                this.vmproxyhash[macroArgumentName] = argumentValue;
                this.localcontext[literalMacroArgumentName] = argumentValue;
            }
        }

        /// <summary> AST nodes that are considered constants can be directly
        /// saved into the context. Dynamic values are stored in
        /// another argument hashmap.
        /// 
        /// </summary>
        /// <param name="node">macro argument as AST node
        /// </param>
        /// <returns> true if the node is a constant value
        /// </returns>
        private bool IsConstant(INode node)
        {
            switch (node.Type)
            {

                case ParserTreeConstants.JJTINTEGERRANGE:
                case ParserTreeConstants.JJTREFERENCE:
                case ParserTreeConstants.JJTOBJECTARRAY:
                case ParserTreeConstants.JJTMAP:
                case ParserTreeConstants.JJTSTRINGLITERAL:
                case ParserTreeConstants.JJTTEXT:
                    return (false);

                default:
                    return (true);

            }
        }

        /// <summary> Impl of the Context.Put() method.
        /// 
        /// </summary>
        /// <param name="key">name of item to set
        /// </param>
        /// <param name="value">object to set to key
        /// </param>
        /// <returns> old stored object
        /// </returns>
        public override object Put(string key, object value)
        {
            return this.Put(key, value, this.localContextScope);
        }

        /// <summary> Allows callers to explicitly Put objects in the local context, no matter what the
        /// velocimacro.context.local setting says. Needed e.g. for loop variables in foreach.
        /// 
        /// </summary>
        /// <param name="key">name of item to set.
        /// </param>
        /// <param name="value">object to set to key.
        /// </param>
        /// <returns> old stored object
        /// </returns>
        public override object LocalPut(string key, object value)
        {
            return this.Put(key, value, true);
        }

        /// <summary> Internal Put method to select between local and global scope.
        /// 
        /// </summary>
        /// <param name="key">name of item to set
        /// </param>
        /// <param name="value">object to set to key
        /// </param>
        /// <param name="forceLocal">True forces the object into the local scope.
        /// </param>
        /// <returns> old stored object
        /// </returns>
        protected internal virtual object Put(string key, object value, bool forceLocal)
        {
            INode astNode = (INode)this.vmproxyhash[key];

            if (astNode != null)
            {
                if (astNode.Type == ParserTreeConstants.JJTREFERENCE)
                {
                    ASTReference ref_Renamed = (ASTReference)astNode;

                    if (ref_Renamed.GetNumChildren() > 0)
                    {
                        ref_Renamed.SetValue(this.innerContext, value);
                        return null;
                    }
                    else
                    {
                        return this.innerContext.Put(ref_Renamed.RootString, value);
                    }
                }
            }

            object tempObject;
            tempObject = this.localcontext[key];
            this.localcontext[key] = value;
            object old = tempObject;
            if (!forceLocal)
            {
                old = base.Put(key, value);
            }
            return old;
        }

        /// <summary> Implementation of the Context.Get() method.  First checks
        /// localcontext, then arguments, then global context.
        /// 
        /// </summary>
        /// <param name="key">name of item to Get
        /// </param>
        /// <returns> stored object or null
        /// </returns>
        public override object Get(string key)
        {
            object o = this.localcontext[key];
            if (o != null)
            {
                return o;
            }

            INode astNode = (INode)this.vmproxyhash[key];

            if (astNode != null)
            {
                int type = astNode.Type;

                // if the macro argument (astNode) is a reference, we need to Evaluate it
                // in case it is a multilevel node
                if (type == NVelocity.Runtime.Parser.ParserTreeConstants.JJTREFERENCE)
                {
                    ASTReference ref_Renamed = (ASTReference)astNode;

                    if (ref_Renamed.GetNumChildren() > 0)
                    {
                        return ref_Renamed.Execute((object)null, this.innerContext);
                    }
                    else
                    {
                        object obj = this.innerContext.Get(ref_Renamed.RootString);
                        if (obj == null && ref_Renamed.strictRef)
                        {
                            if (!this.innerContext.ContainsKey(ref_Renamed.RootString))
                            {
                                throw new MethodInvocationException("Parameter '" + ref_Renamed.RootString + "' not defined", null, key, ref_Renamed.TemplateName, ref_Renamed.Line, ref_Renamed.Column);
                            }
                        }
                        return obj;
                    }
                }
                else if (type == NVelocity.Runtime.Parser.ParserTreeConstants.JJTTEXT)
                {
                    // this really shouldn't happen. text is just a throwaway arg for #foreach()
                    try
                    {
                        System.IO.StringWriter writer = new System.IO.StringWriter();
                        astNode.Render(this.innerContext, writer);

                        return writer.ToString();
                    }
                    catch (System.SystemException e)
                    {
                        throw e;
                    }
                    catch (System.Exception e)
                    {
                        string msg = "ProxyVMContext.Get() : error rendering reference";
                        this.rsvc.Log.Error(msg, e);
                        throw new VelocityException(msg, e);
                    }
                }
                else
                {
                    // use value method to render other dynamic nodes
                    return astNode.Value(this.innerContext);
                }
            }

            return base.Get(key);
        }

        /// <seealso cref="org.apache.velocity.context.Context.containsKey(object)">
        /// </seealso>
        public override bool ContainsKey(object key)
        {
            return this.vmproxyhash.Contains(key) || this.localcontext.Contains(key) || base.ContainsKey(key);
        }

        /// <seealso cref="org.apache.velocity.context.Context.remove(object)">
        /// </seealso>
        public override object Remove(object key)
        {
            object tempObject;
            tempObject = this.localcontext[key];
            this.localcontext.Remove(key);
            object loc = tempObject;
            object tempObject2;
            tempObject2 = this.vmproxyhash[key];
            this.vmproxyhash.Remove(key);
            object arg = tempObject2;
            object glo = null;
            if (!this.localContextScope)
            {
                glo = base.Remove(key);
            }
            if (loc != null)
            {
                return loc;
            }
            return glo;
        }
    }
}