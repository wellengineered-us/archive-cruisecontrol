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
using System.Text;

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Context;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Directive;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser.Node
{
	/// <summary> </summary>
    public class SimpleNode : INode
    {
        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.getLastToken()">
        /// </seealso>
        public virtual Token LastToken
        {
            get
            {
                return this.last;
            }

        }
        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.getType()">
        /// </seealso>
        public virtual int Type
        {
            get
            {
                return this.id;
            }

        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.getInfo()">
        /// </seealso>
        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.setInfo(int)">
        /// </seealso>
        public virtual int Info
        {
            get
            {
                return this.info;
            }

            set
            {
                this.info = value;
            }

        }
        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.getLine()">
        /// </seealso>
        public virtual int Line
        {
            get
            {
                return this.first.BeginLine;
            }

        }
        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.getColumn()">
        /// </seealso>
        public virtual int Column
        {
            get
            {
                return this.first.BeginColumn;
            }

        }

        public virtual string TemplateName
        {
            get
            {
                return this.templateName;
            }

        }

        protected internal IRuntimeServices rsvc = null;


        protected internal Log.Log log = null;


        protected internal INode parent;


        protected internal INode[] children;


        protected internal int id;

        // TODO - It seems that this field is only valid when parsing, and should not be kept around.    
        protected internal Parser parser;


        protected internal int info; // added


        public bool state;


        protected internal bool invalid = false;


        protected internal Token first;


        protected internal Token last;

        protected internal string templateName;

        /// <param name="i">
        /// </param>
        public SimpleNode(int i)
        {
            this.id = i;
        }

        /// <param name="p">
        /// </param>
        /// <param name="i">
        /// </param>
        public SimpleNode(Parser p, int i)
            : this(i)
        {
            this.parser = p;
            this.templateName = this.parser.currentTemplateName;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.jjtOpen()">
        /// </seealso>
        public virtual void Open()
        {
            this.first = this.parser.getToken(1); // added
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.jjtClose()">
        /// </seealso>
        public virtual void Close()
        {
            this.last = this.parser.getToken(0); // added
        }

        /// <param name="t">
        /// </param>
        public virtual void SetFirstToken(Token t)
        {
            this.first = t;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.getFirstToken()">
        /// </seealso>
        public virtual Token FirstToken
        {
            get
            {
                return this.first;
            }
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.jjtGetParent()">
        /// </seealso>
        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.jjtSetParent(NVelocity.Runtime.Paser.Node.Node)">
        /// </seealso>
        public virtual INode Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.jjtAddChild(NVelocity.Runtime.Paser.Node.Node, int)">
        /// </seealso>
        public virtual void AddChild(INode n, int i)
        {
            if (this.children == null)
            {
                this.children = new INode[i + 1];
            }
            else if (i >= this.children.Length)
            {
                INode[] c = new INode[i + 1];
                Array.Copy(this.children, 0, c, 0, this.children.Length);
                this.children = c;
            }
            this.children[i] = n;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.jjtGetChild(int)">
        /// </seealso>
        public virtual INode GetChild(int i)
        {
            return this.children[i];
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.jjtGetNumChildren()">
        /// </seealso>
        public virtual int GetNumChildren()
        {
            return (this.children == null) ? 0 : this.children.Length;
        }


        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.jjtAccept(NVelocity.Runtime.Paser.Node.ParserVisitor, object)">
        /// </seealso>
        public virtual object Accept(IParserVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }


        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.childrenAccept(NVelocity.Runtime.Paser.Node.ParserVisitor, object)">
        /// </seealso>
        public virtual object ChildrenAccept(IParserVisitor visitor, object data)
        {
            if (this.children != null)
            {
                for (int i = 0; i < this.children.Length; ++i)
                {
                    this.children[i].Accept(visitor, data);
                }
            }
            return data;
        }

        /* You can override these two methods in subclasses of SimpleNode to
        customize the way the node appears when the tree is dumped.  If
        your output uses more than one line you should override
        toString(String), otherwise overriding toString() is probably all
        you need to do. */

        //    public String toString()
        // {
        //    return ParserTreeConstants.jjtNodeName[id];
        // }
        /// <param name="prefix">
        /// </param>
        /// <returns> String representation of this node.
        /// </returns>
        public virtual string ToString(string prefix)
        {
            return prefix + this.ToString();
        }

        /// <summary> Override this method if you want to customize how the node dumps
        /// out its children.
        /// 
        /// </summary>
        /// <param name="prefix">
        /// </param>
        public virtual void Dump(string prefix)
        {
            if (this.children != null)
            {
                for (int i = 0; i < this.children.Length; ++i)
                {
                    SimpleNode n = (SimpleNode)this.children[i];
                    if (n != null)
                    {
                        n.Dump(prefix + " ");
                    }
                }
            }
        }

        /// <summary> Return a string that tells the current location of this node.</summary>
        protected internal virtual string GetLocation(IInternalContextAdapter context)
        {
            return Log.Log.FormatFileString(this);
        }

        // All additional methods

        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.literal()">
        /// </seealso>
        public virtual string Literal
        {
            get
            {

                // if we have only one string, just return it and avoid
                // buffer allocation. VELOCITY-606
                if (this.first == this.last)
                {
                    return NodeUtils.TokenLiteral(this.first);
                }

                Token t = this.first;
                StringBuilder sb = new StringBuilder(NodeUtils.TokenLiteral(t));
                while (t != this.last)
                {
                    t = t.Next;
                    sb.Append(NodeUtils.TokenLiteral(t));
                }
                return sb.ToString();
            }
        }

        /// <throws>  TemplateInitException  </throws>
        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.Init(org.apache.velocity.context.InternalContextAdapter, object)">
        /// </seealso>
        public virtual object Init(IInternalContextAdapter context, object data)
        {
            /*
            * hold onto the RuntimeServices
            */

            this.rsvc = (IRuntimeServices)data;
            this.log = this.rsvc.Log;

            int i, k = this.GetNumChildren();

            for (i = 0; i < k; i++)
            {
                this.GetChild(i).Init(context, data);
            }

            return data;
        }

        /// <seealso cref="Runtime.Directive.Evaluate">
        /// </seealso>
        public virtual bool Evaluate(IInternalContextAdapter context)
        {
            return false;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.value(org.apache.velocity.context.InternalContextAdapter)">
        /// </seealso>
        public virtual object Value(IInternalContextAdapter context)
        {
            return null;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.render(org.apache.velocity.context.InternalContextAdapter, java.io.Writer)">
        /// </seealso>
        public virtual bool Render(IInternalContextAdapter context, System.IO.TextWriter writer)
        {
            int i, k = this.GetNumChildren();

            for (i = 0; i < k; i++)
                this.GetChild(i).Render(context, writer);

            return true;
        }

        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.Execute(object, org.apache.velocity.context.InternalContextAdapter)">
        /// </seealso>
        public virtual object Execute(object o, IInternalContextAdapter context)
        {
            return null;
        }



        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.setInvalid()">
        /// </seealso>
        /// <seealso cref="NVelocity.Runtime.Paser.Node.Node.isInvalid()">
        /// </seealso>
        public virtual bool IsInvalid
        {
            get
            {
                return this.invalid;
            }
            set
            {
                this.invalid = true;
            }
        }

        /// <since> 1.5
        /// </since>
        public override string ToString()
        {
            StringBuilder tokens = new StringBuilder();

            for (Token t = this.FirstToken; t != null; )
            {
                tokens.Append("[").Append(t.Image).Append("]");
                if (t.Next != null)
                {
                    if (t.Equals(this.LastToken))
                    {
                        break;
                    }
                    else
                    {
                        tokens.Append(", ");
                    }
                }
                t = t.Next;
            }

            return new StringBuilder().AppendFormat("id:{0}", this.Type).AppendFormat("info:{0}", this.Info).AppendFormat("invalid:{0}", this.IsInvalid).AppendFormat("children:{0}", this.GetNumChildren()).AppendFormat("tokens:{0}", tokens).ToString();
        }
    }
}