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

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.App.Event;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Context;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Util.Introspection;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser.Node
{
	/// <summary> Node for the #set directive
    /// 
    /// </summary>
    /// <author>  <a href="mailto:jvanzyl@apache.org">Jason van Zyl</a>
    /// </author>
    /// <author>  <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
    /// </author>
    /// <version>  $Id: ASTSetDirective.java 720228 2008-11-24 16:58:33Z nbubna $
    /// </version>
    public class ASTSetDirective : SimpleNode
    {
        /// <summary>  returns the ASTReference that is the LHS of the set statememt
        /// 
        /// </summary>
        /// <returns> left hand side of #set statement
        /// </returns>
        private ASTReference LeftHandSide
        {
            get
            {
                return (ASTReference)this.GetChild(0);
            }

        }
        /// <summary>  returns the RHS Node of the set statement
        /// 
        /// </summary>
        /// <returns> right hand side of #set statement
        /// </returns>
        private INode RightHandSide
        {
            get
            {
                return this.GetChild(1);
            }

        }
        private string leftReference = "";
        private INode right = null;
        private ASTReference left = null;
        internal bool logOnNull = false;
        private bool allowNull = false;
        private bool isInitialized;

        /// <summary>  This is really immutable after the Init, so keep one for this node</summary>
        protected internal Info uberInfo;

        /// <summary> Indicates if we are running in strict reference mode.</summary>
        protected internal bool strictRef = false;

        /// <param name="id">
        /// </param>
        public ASTSetDirective(int id)
            : base(id)
        {
        }

        /// <param name="p">
        /// </param>
        /// <param name="id">
        /// </param>
        public ASTSetDirective(Parser p, int id)
            : base(p, id)
        {
        }

        /// <seeIParserVisitorime.Paser.Node.IParserVisitor, System.Object)">
        /// </seealso>
        public override object Accept(IParserVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>  simple Init.  We can Get the RHS and LHS as the the tree structure is static</summary>
        /// <param name="context">
        /// </param>
        /// <param name="data">
        /// </param>
        /// <returns> Init result.
        /// </returns>
        public override object Init(IInternalContextAdapter context, object data)
        {
            lock (this)
            {
                /** This method is synchronized to prevent double initialization or initialization while rendering **/

                if (!this.isInitialized)
                {
                    /*
                    *  Init the tree correctly
                    */

                    base.Init(context, data);

                    this.uberInfo = new Info(this.TemplateName, this.Line, this.Column);

                    this.right = this.RightHandSide;
                    this.left = this.LeftHandSide;

                    this.logOnNull = this.rsvc.GetBoolean(NVelocity.Runtime.RuntimeConstants.RUNTIME_LOG_REFERENCE_LOG_INVALID, true);
                    this.allowNull = this.rsvc.GetBoolean(NVelocity.Runtime.RuntimeConstants.SET_NULL_ALLOWED, false);
                    this.strictRef = this.rsvc.GetBoolean(NVelocity.Runtime.RuntimeConstants.RUNTIME_REFERENCES_STRICT, false);
                    if (this.strictRef)
                        this.allowNull = true; // strictRef implies allowNull

                    /*
                    *  grab this now.  No need to redo each time
                    */
                    this.leftReference = this.left.FirstToken.Image.Substring(1);

                    this.isInitialized = true;
                }

                return data;
            }
        }

        /// <summary>   puts the value of the RHS into the context under the key of the LHS</summary>
        /// <param name="context">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <returns> True if rendering was sucessful.
        /// </returns>
        /// <throws>  IOException </throws>
        /// <throws>  MethodInvocationException </throws>
        public override bool Render(IInternalContextAdapter context, System.IO.TextWriter writer)
        {
            /*
            *  Get the RHS node, and its value
            */

            object value = this.right.Value(context);

            /*
            * it's an Error if we don't have a value of some sort AND
            * it is not allowed by configuration
            */

            if (!this.allowNull)
            {
                if (value == null)
                {
                    /*
                    *  first, are we supposed to say anything anyway?
                    */
                    if (this.logOnNull)
                    {
                        bool doit = EventHandlerUtil.ShouldLogOnNullSet(this.rsvc, context, this.left.Literal, this.right.Literal);

                        if (doit && this.rsvc.Log.DebugEnabled)
                        {
                            this.rsvc.Log.Debug("RHS of #set statement is null. Context will not be modified. " + Log.Log.FormatFileString(this));
                        }
                    }

                    string rightReference = null;
                    if (this.right is ASTExpression)
                    {
                        rightReference = ((ASTExpression)this.right).LastToken.Image;
                    }
                    EventHandlerUtil.InvalidSetMethod(this.rsvc, context, this.leftReference, rightReference, this.uberInfo);

                    return false;
                }
            }

            if (value == null && !this.strictRef)
            {
                string rightReference = null;
                if (this.right is ASTExpression)
                {
                    rightReference = ((ASTExpression)this.right).LastToken.Image;
                }
                EventHandlerUtil.InvalidSetMethod(this.rsvc, context, this.leftReference, rightReference, this.uberInfo);

                /*
                * if RHS is null, remove simple LHS from context
                * or call setValue() with a null value for complex LHS
                */
                if (this.left.GetNumChildren() == 0)
                {
                    context.Remove(this.leftReference);
                }
                else
                {
                    this.left.SetValue(context, (object)null);
                }

                return false;
            }
            else
            {
                /*
                *  if the LHS is simple, just punch the value into the context
                *  otherwise, use the setValue() method do to it.
                *  Maybe we should always use setValue()
                */

                if (this.left.GetNumChildren() == 0)
                {
                    context.Put(this.leftReference, value);
                }
                else
                {
                    this.left.SetValue(context, value);
                }
            }

            return true;
        }
    }
}