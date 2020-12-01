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
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Exception;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser.Node;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Directive
{
	/// <summary> Directive that puts an unrendered AST block in the context
    /// under the specified key, postponing rendering until the
    /// reference is used and rendered.
    /// 
    /// </summary>
    /// <author>  Andrew Tetlaw
    /// </author>
    /// <author>  Nathan Bubna
    /// </author>
    /// <version>  $Id: Define.java 686842 2008-08-18 18:29:31Z nbubna $
    /// </version>
    public class Define : Directive
    {
        /// <summary> Return type of this directive.</summary>
        override public int Type
        {
            get
            {
                return NVelocity.Runtime.Directive.DirectiveType.BLOCK;
            }

        }
        private string key;
        private INode block;
        private Log.Log log;
        private int maxDepth;
        private string definingTemplate;

        /// <summary> Return name of this directive.</summary>
        public override string Name
        {
            get
            {
                return "define";
            }
        }

        /// <summary>  simple Init - Get the key</summary>
        public override void Init(IRuntimeServices rs, IInternalContextAdapter context, INode node)
        {
            base.Init(rs, context, node);

            this.log = rs.Log;

            /*
            * default max depth of two is used because intentional recursion is
            * unlikely and discouraged, so make unintentional ones end fast
            */
            this.maxDepth = rs.GetInt(NVelocity.Runtime.RuntimeConstants.DEFINE_DIRECTIVE_MAXDEPTH, 2);

            /*
            * first token is the name of the block. We don't even check the format,
            * just assume it looks like this: $block_name. Should we check if it has
            * a '$' or not?
            */
            this.key = node.GetChild(0).FirstToken.Image.Substring(1);

            /**
            * No checking is done. We just grab the second child node and assume
            * that it's the block!
            */
            this.block = node.GetChild(1);

            /**
            * keep tabs on the template this came from
            */
            this.definingTemplate = context.CurrentTemplateName;
        }

        /// <summary> directive.render() simply makes an instance of the Block inner class
        /// and places it into the context as indicated.
        /// </summary>
        public override bool Render(IInternalContextAdapter context, System.IO.TextWriter writer, INode node)
        {
            /* Put a Block instance into the context,
            * using the user-defined key, for later inline rendering.
            */
            context.Put(this.key, new Block(context, this));
            return true;
        }

        /// <summary> Creates a string identifying the source and location of the block
        /// definition, and the current template being rendered if that is
        /// different.
        /// </summary>
        protected internal virtual string Id(IInternalContextAdapter context)
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder(100).Append("block $").Append(this.key).Append(" (defined in ").Append(this.definingTemplate).Append(" [line ").Append(this.Line).Append(", column ").Append(this.Column).Append("])");

            if (!context.CurrentTemplateName.Equals(this.definingTemplate))
            {
                str.Append(" used in ").Append(context.CurrentTemplateName);
            }

            return str.ToString();
        }

        /// <summary> parameters class placed in the context, holds the context and writer
        /// being used for the render, as well as the parent (which already holds
        /// everything else we need).
        /// </summary>
        public class Block : IRenderable
        {
            private IInternalContextAdapter context;
            private Define parent;
            private int depth;

            public Block(IInternalContextAdapter context, Define parent)
            {
                this.context = context;
                this.parent = parent;
            }

            /// <summary> </summary>
            public virtual bool Render(IInternalContextAdapter context, System.IO.TextWriter writer)
            {
                try
                {
                    this.depth++;
                    if (this.depth > this.parent.maxDepth)
                    {
                        /* this is only a Debug message, as recursion can
                        * happen in quasi-innocent situations and is relatively
                        * harmless due to how we handle it here.
                        * this is more to help anyone nuts enough to intentionally
                        * use recursive block definitions and having problems
                        * pulling it off properly.
                        */
                        this.parent.log.Debug("Max recursion depth reached for " + this.parent.Id(context));
                        this.depth--;
                        return false;
                    }
                    else
                    {
                        this.parent.block.Render(context, writer);
                        this.depth--;
                        return true;
                    }
                }
                catch (System.IO.IOException e)
                {
                    string msg = "Failed to render " + this.parent.Id(context) + " to writer";
                    this.parent.log.Error(msg, e);
                    throw new RuntimeException(msg, e);
                }
                catch (VelocityException ve)
                {
                    string msg = "Failed to render " + this.parent.Id(context) + " due to " + ve;
                    this.parent.log.Error(msg, ve);
                    throw ve;
                }
            }

            public override string ToString()
            {
                System.IO.TextWriter stringwriter = new System.IO.StringWriter();

                if (this.Render(this.context, stringwriter))
                {
                    return stringwriter.ToString();
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
