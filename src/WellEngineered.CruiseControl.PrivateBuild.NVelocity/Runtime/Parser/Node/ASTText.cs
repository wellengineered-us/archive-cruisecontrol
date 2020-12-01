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

using System.IO;

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Context;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser.Node
{
	/// <summary> </summary>
    public class ASTText : SimpleNode
    {
        private string text;

        /// <param name="id">
        /// </param>
        public ASTText(int id)
            : base(id)
        {
        }

        /// <param name="p">
        /// </param>
        /// <param name="id">
        /// </param>
        public ASTText(Parser p, int id)
            : base(p, id)
        {
        }

        /// <seeIParserVisitorime.Paser.Node.IParserVisitor, System.Object)">
        /// </seealso>
        public override object Accept(IParserVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <seealso cref="SimpleNode.Init(org.apache.velocity.conobjecttextAdapter, java.lang.Object)">
        /// </seealso>
        public override object Init(IInternalContextAdapter context, object data)
        {
            Token t = this.FirstToken;

            this.text = NodeUtils.TokenLiteral(t);

            return data;
        }

        /// <seealso cref="SimpleNode.render(org.apache.velocity.context.InternalContextAdapter, java.io.Writer)">
        /// </seealso>
        public override bool Render(IInternalContextAdapter context, TextWriter writer)
        {
            if (context.AllowRendering)
            {
                writer.Write(this.text);
            }
            return true;
        }
    }
}