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

using System;
using System.Collections;
using System.IO;

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Context;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Exception;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser.Node;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Util.Introspection;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Directive
{
	/// <summary> Foreach directive used for moving through arrays,
    /// or objects that provide an Iterator.
    /// 
    /// </summary>
    /// <author>  <a href="mailto:jvanzyl@apache.org">Jason van Zyl</a>
    /// </author>
    /// <author>  <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
    /// </author>
    /// <author>  Daniel Rall
    /// </author>
    /// <version>  $Id: Foreach.java 730367 2008-12-31 10:29:21Z byron $
    /// </version>
    public class Foreach : Directive
    {
        /// <summary> Return type of this directive.</summary>
        /// <returns> The type of this directive.
        /// </returns>
        public override int Type
        {
            get
            {
                return DirectiveType.BLOCK;
            }

        }
        /// <summary> A special context to use when the foreach iterator returns a null.  This
        /// is required since the standard context may not support nulls.
        /// All puts and gets are passed through, except for the foreach iterator key.
        /// </summary>
        /// <since> 1.5
        /// </since>
        protected internal class NullHolderContext : ChainedInternalContextAdapter
        {
            private string loopVariableKey = "";
            private bool active = true;

            /// <summary> Create the context as a wrapper to be used within the foreach</summary>
            /// <param name="key">the reference used in the foreach
            /// </param>
            /// <param name="context">the parent context
            /// </param>
            internal NullHolderContext(string key, IInternalContextAdapter context)
                : base(context)
            {
                if (key != null)
                    this.loopVariableKey = key;
            }

            /// <summary> Get an object from the context, or null if the key is equal to the loop variable</summary>
            /// <seealso cref="org.apache.velocity.context.InternalContextAdapter.get(string)">
            /// </seealso>
            /// <exception cref="MethodInvocationException">passes on potential exception from reference method call
            /// </exception>
            public override object Get(string key)
            {
                return (this.active && this.loopVariableKey.Equals(key)) ? null : base.Get(key);
            }

            /// <seealso cref="org.apache.velocity.context.InternalContextAdapter.put(string key, object value)">
            /// </seealso>
            public override object Put(string key, object value)
            {
                if (this.loopVariableKey.Equals(key) && (value == null))
                {
                    this.active = true;
                }

                return base.Put(key, value);
            }

            /// <summary> Allows callers to explicitly put objects in the local context.
            /// Objects added to the context through this method always end up
            /// in the top-level context of possible wrapped contexts.
            /// 
            /// </summary>
            /// <param name="key">name of item to set.
            /// </param>
            /// <param name="value">object to set to key.
            /// </param>
            /// <seealso cref="org.apache.velocity.context.InternalWrapperContext.localPut(String, Object)">
            /// </seealso>
            public override object LocalPut(string key, object value)
            {
                return this.Put(key, value);
            }

            /// <summary> Remove an object from the context</summary>
            /// <seealso cref="org.apache.velocity.context.InternalContextAdapter.remove(object key)">
            /// </seealso>
            public override object Remove(object key)
            {
                if (this.loopVariableKey.Equals(key))
                {
                    this.active = false;
                }
                return base.Remove(key);
            }
        }

        /// <summary> Return name of this directive.</summary>
        /// <returns> The name of this directive.
        /// </returns>
        public override string Name
        {
            get
            {
                return "foreach";
            }
        }

        /// <summary> The name of the variable to use when placing
        /// the counter value into the context. Right
        /// now the default is $velocityCount.
        /// </summary>
        private string counterName;

        /// <summary> What value to start the loop counter at.</summary>
        private int counterInitialValue;

        /// <summary> The maximum number of times we're allowed to loop.</summary>
        private int maxNbrLoops;

        /// <summary> Whether or not to throw an Exception if the iterator is null.</summary>
        private bool skipInvalidIterator;

        /// <summary> The reference name used to access each
        /// of the elements in the list object. It
        /// is the $item in the following:
        /// 
        /// #foreach ($item in $list)
        /// 
        /// This can be used class wide because
        /// it is immutable.
        /// </summary>
        private string elementKey;

        /// <summary>  immutable, so create in init</summary>
        protected internal Info uberInfo;

        /// <summary>  simple init - init the tree and get the elementKey from
        /// the AST
        /// </summary>
        /// <param name="rs">
        /// </param>
        /// <param name="context">
        /// </param>
        /// <param name="node">
        /// </param>
        /// <throws>  TemplateInitException </throws>
        public override void Init(IRuntimeServices rs, IInternalContextAdapter context, INode node)
        {
            base.Init(rs, context, node);

            this.counterName = this.rsvc.GetString(RuntimeConstants.COUNTER_NAME);
            this.counterInitialValue = this.rsvc.GetInt(RuntimeConstants.COUNTER_INITIAL_VALUE);
            this.maxNbrLoops = this.rsvc.GetInt(RuntimeConstants.MAX_NUMBER_LOOPS, int.MaxValue);
            if (this.maxNbrLoops < 1)
            {
                this.maxNbrLoops = int.MaxValue;
            }
            this.skipInvalidIterator = this.rsvc.GetBoolean(RuntimeConstants.SKIP_INVALID_ITERATOR, true);

            if (this.rsvc.GetBoolean(RuntimeConstants.RUNTIME_REFERENCES_STRICT, false))
            {
                // If we are in strict mode then the default for skipInvalidItarator
                // is true.  However, if the property is explicitly set, then honor the setting.
                this.skipInvalidIterator = this.rsvc.GetBoolean(RuntimeConstants.SKIP_INVALID_ITERATOR, false);
            }

            /*
            *  this is really the only thing we can do here as everything
            *  else is context sensitive
            */
            SimpleNode sn = (SimpleNode)node.GetChild(0);

            if (sn is ASTReference)
            {
                this.elementKey = ((ASTReference)sn).RootString;
            }
            else
            {
                /*
                * the default, error-prone way which we'll remove
                *  TODO : remove if all goes well
                */
                this.elementKey = sn.FirstToken.Image.Substring(1);
            }

            /*
            * make an uberinfo - saves new's later on
            */

            this.uberInfo = new Info(this.TemplateName, this.Line, this.Column);
        }

        /// <summary> Extension hook to allow subclasses to control whether loop vars
        /// are set locally or not. So, those in favor of VELOCITY-285, can
        /// make that happen easily by overriding this and having it use
        /// context.localPut(k,v). See VELOCITY-630 for more on this.
        /// </summary>
        protected internal virtual void Put(IInternalContextAdapter context, string key, object value)
        {
            context.Put(key, value);
        }

        /// <summary>  renders the #foreach() block</summary>
        /// <param name="context">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <param name="node">
        /// </param>
        /// <returns> True if the directive rendered successfully.
        /// </returns>
        /// <throws>  IOException </throws>
        /// <throws>  MethodInvocationException </throws>
        /// <throws>  ResourceNotFoundException </throws>
        /// <throws>  ParseErrorException </throws>
        public override bool Render(IInternalContextAdapter context, TextWriter writer, INode node)
        {
            /*
            *  do our introspection to see what our collection is
            */

            object listObject = node.GetChild(2).Value(context);

            if (listObject == null)
                return false;

            IEnumerator i = null;

            try
            {
                i = this.rsvc.Uberspect.GetIterator(listObject, this.uberInfo);
            }
            /**
            * pass through application level runtime exceptions
            */
            catch (SystemException e)
            {
                throw e;
            }
            catch (System.Exception ee)
            {
                string msg = "Error getting iterator for #foreach at " + this.uberInfo;
                this.rsvc.Log.Error(msg, ee);
                throw new VelocityException(msg, ee);
            }

            if (i == null)
            {
                if (this.skipInvalidIterator)
                {
                    return false;
                }
                else
                {
                    INode pnode = node.GetChild(2);

                    System.String msg = "#foreach parameter " + pnode.Literal + " at " + Log.Log.FormatFileString(pnode) + " is of type " + listObject.GetType().FullName + " and is either of wrong type or cannot be iterated.";
                    this.rsvc.Log.Error(msg);
                    throw new VelocityException(msg);
                }
            }

            int counter = this.counterInitialValue;
            bool maxNbrLoopsExceeded = false;

            /*
            *  save the element key if there is one, and the loop counter
            */
            object o = context.Get(this.elementKey);
            object savedCounter = context.Get(this.counterName);

            /*
            * Instantiate the null holder context if a null value
            * is returned by the foreach iterator.  Only one instance is
            * created - it's reused for every null value.
            */
            NullHolderContext nullHolderContext = null;


            while (!maxNbrLoopsExceeded && i.MoveNext())
            {
                // TODO: JDK 1.5+ -> Integer.valueOf()
                this.Put(context, this.counterName, (object)counter);

                object value = i.Current;

                this.Put(context, this.elementKey, value);

                try
                {
                    /*
                    * If the value is null, use the special null holder context
                    */
                    if (value == null)
                    {
                        if (nullHolderContext == null)
                        {
                            // lazy instantiation
                            nullHolderContext = new NullHolderContext(this.elementKey, context);
                        }
                        node.GetChild(3).Render(nullHolderContext, writer);
                    }
                    else
                    {
                        node.GetChild(3).Render(context, writer);
                    }
                }
                catch (Break.BreakException ex)
                {
                    // encountered #break directive inside #foreach loop
                    break;
                }

                counter++;

                // Determine whether we're allowed to continue looping.
                // ASSUMPTION: counterInitialValue is not negative!
                maxNbrLoopsExceeded = (counter - this.counterInitialValue) >= this.maxNbrLoops;
            }

            /*
            * restores the loop counter (if we were nested)
            * if we have one, else just removes
            */

            if (savedCounter != null)
            {
                context.Put(this.counterName, savedCounter);
            }
            else
            {
                context.Remove(this.counterName);
            }


            /*
            *  restores element key if exists
            *  otherwise just removes
            */

            if (o != null)
            {
                context.Put(this.elementKey, o);
            }
            else
            {
                context.Remove(this.elementKey);
            }

            return true;
        }
    }
}