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
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser.Node;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Visitor
{
	/// <summary> This is the base class for all visitors.
    /// For each AST node, this class will provide
    /// a bare-bones method for traversal.
    /// 
    /// </summary>
    /// <author>  <a href="mailto:jvanzyl@apache.org">Jason van Zyl</a>
    /// </author>
    /// <author>  <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
    /// </author>
    /// <version>  $Id: BaseVisitor.java 685685 2008-08-13 21:43:27Z nbubna $
    /// </version>
    public abstract class BaseVisitor : IParserVisitor
    {
        /// <param name="writer">
        /// </param>
        virtual public System.IO.TextWriter Writer
        {
            set
            {
                this.writer = value;
            }

        }
        /// <param name="context">
        /// </param>
        virtual public IInternalContextAdapter Context
        {
            set
            {
                this.context = value;
            }

        }
        /// <summary>Context used during traversal </summary>
        protected internal IInternalContextAdapter context;

        /// <summary>Writer used as the output sink </summary>
        protected internal System.IO.TextWriter writer;

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(SimpleNode, object)">
        /// </seealso>
        public virtual object Visit(SimpleNode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTprocess, object)">
        /// </seealso>
        public virtual object Visit(ASTprocess node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTExpression, object)">
        /// </seealso>
        public virtual object Visit(ASTExpression node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTAssignment, object)">
        /// </seealso>
        public virtual object Visit(ASTAssignment node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTOrNode, object)">
        /// </seealso>
        public virtual object Visit(ASTOrNode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTAndNode, object)">
        /// </seealso>
        public virtual object Visit(ASTAndNode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTEQNode, object)">
        /// </seealso>
        public virtual object Visit(ASTEQNode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTNENode, object)">
        /// </seealso>
        public virtual object Visit(ASTNENode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTLTNode, object)">
        /// </seealso>
        public virtual object Visit(ASTLTNode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTGTNode, object)">
        /// </seealso>
        public virtual object Visit(ASTGTNode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTLENode, object)">
        /// </seealso>
        public virtual object Visit(ASTLENode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTGENode, object)">
        /// </seealso>
        public virtual object Visit(ASTGENode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTAddNode, object)">
        /// </seealso>
        public virtual object Visit(ASTAddNode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTSubtractNode, object)">
        /// </seealso>
        public virtual object Visit(ASTSubtractNode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTMulNode, object)">
        /// </seealso>
        public virtual object Visit(ASTMulNode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTDivNode, object)">
        /// </seealso>
        public virtual object Visit(ASTDivNode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTModNode, object)">
        /// </seealso>
        public virtual object Visit(ASTModNode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTNotNode, object)">
        /// </seealso>
        public virtual object Visit(ASTNotNode node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTIntegerLiteral, object)">
        /// </seealso>
        public virtual object Visit(ASTIntegerLiteral node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTFloatingPointLiteral, object)">
        /// </seealso>
        /// <since> 1.5
        /// </since>
        public virtual object Visit(ASTFloatingPointLiteral node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTStringLiteral, object)">
        /// </seealso>
        public virtual object Visit(ASTStringLiteral node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTIdentifier, object)">
        /// </seealso>
        public virtual object Visit(ASTIdentifier node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTMethod, object)">
        /// </seealso>
        public virtual object Visit(ASTMethod node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTReference, object)">
        /// </seealso>
        public virtual object Visit(ASTReference node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTTrue, object)">
        /// </seealso>
        public virtual object Visit(ASTTrue node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTFalse, object)">
        /// </seealso>
        public virtual object Visit(ASTFalse node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTBlock, object)">
        /// </seealso>
        public virtual object Visit(ASTBlock node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTText, object)">
        /// </seealso>
        public virtual object Visit(ASTText node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTIfStatement, object)">
        /// </seealso>
        public virtual object Visit(ASTIfStatement node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTElseStatement, object)">
        /// </seealso>
        public virtual object Visit(ASTElseStatement node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTElseIfStatement, object)">
        /// </seealso>
        public virtual object Visit(ASTElseIfStatement node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTComment, object)">
        /// </seealso>
        public virtual object Visit(ASTComment node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTObjectArray, object)">
        /// </seealso>
        public virtual object Visit(ASTObjectArray node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTWord, object)">
        /// </seealso>
        public virtual object Visit(ASTWord node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTSetDirective, object)">
        /// </seealso>
        public virtual object Visit(ASTSetDirective node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTDirective, object)">
        /// </seealso>
        public virtual object Visit(ASTDirective node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTEscapedDirective, object)">
        /// </seealso>
        /// <since> 1.5
        /// </since>
        public virtual object Visit(ASTEscapedDirective node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTEscape, object)">
        /// </seealso>
        /// <since> 1.5
        /// </since>
        public virtual object Visit(ASTEscape node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTMap, object)">
        /// </seealso>
        /// <since> 1.5
        /// </since>
        public virtual object Visit(ASTMap node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTIntegerRange, object)">
        /// </seealso>
        /// <since> 1.5
        /// </since>
        public virtual object Visit(ASTIntegerRange node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.ParserVisitor.visit(ASTStop, object)">
        /// </seealso>
        /// <since> 1.5
        /// </since>
        public virtual object Visit(ASTStop node, object data)
        {
            data = node.ChildrenAccept(this, data);
            return data;
        }
    }
}