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

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Context;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Directive;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Util;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser.Node
{
	/// <summary>  Handles <code>arg1  != arg2</code>
    /// 
    /// This operator requires that the LHS and RHS are both of the
    /// same Class OR both are subclasses of java.lang.Number
    /// 
    /// </summary>
    /// <author>  <a href="mailto:wglass@forio.com">Will Glass-Husain</a>
    /// </author>
    /// <author>  <a href="mailto:pero@antaramusic.de">Peter Romianowski</a>
    /// </author>
    public class ASTNENode : SimpleNode
    {
        /// <param name="id">
        /// </param>
        public ASTNENode(int id)
            : base(id)
        {
        }

        /// <param name="p">
        /// </param>
        /// <param name="id">
        /// </param>
        public ASTNENode(Parser p, int id)
            : base(p, id)
        {
        }

        /// <seeIParserVisitorime.Paser.Node.IParserVisitor, System.Object)">
        /// </seealso>
        public override object Accept(IParserVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <seealso cref="Runtime.Directive.Evaluate">
        /// </seealso>
        public override bool Evaluate(IInternalContextAdapter context)
        {
            object left = this.GetChild(0).Value(context);
            object right = this.GetChild(1).Value(context);

            /*
            *  convert to Number if applicable
            */
            if (left is ITemplateNumber)
            {
                left = ((ITemplateNumber)left).AsNumber;
            }
            if (right is ITemplateNumber)
            {
                right = ((ITemplateNumber)right).AsNumber;
            }

            /*
            * If comparing Numbers we do not care about the Class.
            */
            if (left is System.ValueType && right is System.ValueType)
            {
                return !left.Equals(right);
            }

            /**
            * if both are not null, then assume that if one class
            * is a subclass of the other that we should use the equals operator
            */
            if (left != null && right != null && (left.GetType().IsAssignableFrom(right.GetType()) || right.GetType().IsAssignableFrom(left.GetType())))
            {
                return !left.Equals(right);
            }

            /*
            * Ok, time to Compare string values
            */

            left = (left == null) ? null : left.ToString();

            right = (right == null) ? null : right.ToString();

            if (left == null && right == null)
            {
                if (this.log.DebugEnabled)
                {
                    this.log.Debug("Both right (" + this.GetLiteral(false) + " and left " + this.GetLiteral(true) + " sides of '!=' operation returned null." + "If references, they may not be in the context." + this.GetLocation(context));
                }
                return false;
            }
            else if (left == null || right == null)
            {
                if (this.log.DebugEnabled)
                {
                    this.log.Debug((left == null ? "Left" : "Right") + " side (" + this.GetLiteral(left == null) + ") of '!=' operation has null value. If it is a " + "reference, it may not be in the context or its " + "toString() returned null. " + this.GetLocation(context));
                }
                return true;
            }
            else
            {
                return !left.Equals(right);
            }
        }

        private string GetLiteral(bool left)
        {
            return this.GetChild(left ? 0 : 1).Literal;
        }

        /// <seealso cref="SimpleNode.Value(NVelocity.Context.IInternalContextAdapter)">
        /// </seealso>
        public override object Value(IInternalContextAdapter context)
        {
            bool val = this.Evaluate(context);

            return val ? true : false;
        }
    }
}