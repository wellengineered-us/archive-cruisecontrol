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
using System.IO;
using System.Reflection;

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.App.Event;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Context;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Exception;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Util.Introspection;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser.Node
{
	/// <summary> This class is responsible for handling the references in
    /// VTL ($foo).
    /// 
    /// Please look at the Parser.jjt file which is
    /// what controls the generation of this class.
    /// 
    /// </summary>
    /// <author>  <a href="mailto:jvanzyl@apache.org">Jason van Zyl</a>
    /// </author>
    /// <author>  <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
    /// </author>
    /// <author>  <a href="mailto:Christoph.Reck@dlr.de">Christoph Reck</a>
    /// </author>
    /// <author>  <a href="mailto:kjohnson@transparent.com>Kent Johnson</a>
    /// </author>
    /// <version>  $Id: ASTReference.java 730988 2009-01-03 14:15:56Z byron $
    /// </version>
    public class ASTReference : SimpleNode
    {
        /// <summary>  Returns the 'root string', the reference key</summary>
        /// <returns> the root string.
        /// </returns>
        public virtual string RootString
        {
            get
            {
                return this.rootString;
            }

        }
        private string Root
        {
            get
            {
                Token t = this.FirstToken;

                /*
                *  we have a special case where something like
                *  $(\\)*!, where the user want's to see something
                *  like $!blargh in the output, but the ! prevents it from showing.
                *  I think that at this point, this isn't a reference.
                */

                /* so, see if we have "\\!" */

                int slashbang = t.Image.IndexOf("\\!");

                if (slashbang != -1)
                {
                    /*
                    *  lets do all the work here.  I would argue that if this occurrs,
                    *  it's not a reference at all, so preceeding \ characters in front
                    *  of the $ are just schmoo.  So we just do the escape processing
                    *  trick (even | odd) and move on.  This kind of breaks the rule
                    *  pattern of $ and # but '!' really tosses a wrench into things.
                    */

                    /*
                    *  count the escapes : even # -> not escaped, odd -> escaped
                    */

                    int i = 0;
                    int len = t.Image.Length;

                    i = t.Image.IndexOf('$');

                    if (i == -1)
                    {
                        /* yikes! */
                        this.log.Error("ASTReference.getRoot() : internal error : " + "no $ found for slashbang.");
                        this.computableReference = false;
                        this.nullString = t.Image;
                        return this.nullString;
                    }

                    while (i < len && t.Image[i] != '\\')
                    {
                        i++;
                    }

                    /*  ok, i is the first \ char */

                    int start = i;
                    int count = 0;

                    while (i < len && t.Image[i++] == '\\')
                    {
                        count++;
                    }

                    /*
                    *  now construct the output string.  We really don't care about
                    *  leading  slashes as this is not a reference.  It's quasi-schmoo
                    */

                    this.nullString = t.Image.Substring(0, (start) - (0)); // prefix up to the first
                    this.nullString += t.Image.Substring(start, (start + count - 1) - (start)); // get the slashes
                    this.nullString += t.Image.Substring(start + count); // and the rest, including the

                    /*
                    *  this isn't a valid reference, so lets short circuit the value
                    *  and set calcs
                    */

                    this.computableReference = false;

                    return this.nullString;
                }

                /*
                *  we need to see if this reference is escaped.  if so
                *  we will clean off the leading \'s and let the
                *  regular behavior determine if we should output this
                *  as \$foo or $foo later on in render(). Lazyness..
                */

                this.escaped = false;

                if (t.Image.StartsWith("\\"))
                {
                    /*
                    *  count the escapes : even # -> not escaped, odd -> escaped
                    */

                    int i = 0;
                    int len = t.Image.Length;

                    while (i < len && t.Image[i] == '\\')
                    {
                        i++;
                    }

                    if ((i % 2) != 0)
                        this.escaped = true;

                    if (i > 0)
                        this.escPrefix = t.Image.Substring(0, (i / 2) - (0));

                    t.Image = t.Image.Substring(i);
                }

                /*
                *  Look for preceeding stuff like '#' and '$'
                *  and snip it off, except for the
                *  last $
                */

                int loc1 = t.Image.LastIndexOf('$');

                /*
                *  if we have extra stuff, loc > 0
                *  ex. '#$foo' so attach that to
                *  the prefix.
                */
                if (loc1 > 0)
                {
                    this.morePrefix = this.morePrefix + t.Image.Substring(0, (loc1) - (0));
                    t.Image = t.Image.Substring(loc1);
                }

                /*
                *  Now it should be clean. Get the literal in case this reference
                *  isn't backed by the context at runtime, and then figure out what
                *  we are working with.
                */

                // FIXME: this is the key to render nulls as literals, we need to look at context(refname+".literal") 
                this.nullString = this.Literal;

                if (t.Image.StartsWith("$!"))
                {
                    this.referenceType = QUIET_REFERENCE;

                    /*
                    *  only if we aren't escaped do we want to null the output
                    */

                    if (!this.escaped)
                        this.nullString = "";

                    if (t.Image.StartsWith("$!{"))
                    {
                        /*
                        *  ex : $!{provider.Title}
                        */

                        return t.Next.Image;
                    }
                    else
                    {
                        /*
                        *  ex : $!provider.Title
                        */

                        return t.Image.Substring(2);
                    }
                }
                else if (t.Image.Equals("${"))
                {
                    /*
                    *  ex : ${provider.Title}
                    */

                    this.referenceType = FORMAL_REFERENCE;
                    return t.Next.Image;
                }
                else if (t.Image.StartsWith("$"))
                {
                    /*
                    *  just nip off the '$' so we have
                    *  the root
                    */

                    this.referenceType = NORMAL_REFERENCE;
                    return t.Image.Substring(1);
                }
                else
                {
                    /*
                    * this is a 'RUNT', which can happen in certain circumstances where
                    *  the parser is fooled into believeing that an IDENTIFIER is a real
                    *  reference.  Another 'dreaded' MORE hack :).
                    */
                    this.referenceType = RUNT;
                    return t.Image;
                }
            }

        }

        /// <summary>  Override of the SimpleNode method literal()
        /// Returns the literal representation of the
        /// node.  Should be something like
        /// $<token>.
        /// </summary>
        /// <returns> A literal string.
        /// </returns>
        /// <summary>  Routine to allow the literal representation to be
        /// externally overridden.  Used now in the VM system
        /// to override a reference in a VM tree with the
        /// literal of the calling arg to make it work nicely
        /// when calling arg is null.  It seems a bit much, but
        /// does keep things consistant.
        /// 
        /// Note, you can only set the literal once...
        /// 
        /// </summary>
        /// <param name="literal">String to render to when null
        /// </param>
        public virtual string Literal
        {
            get
            {
                if (this.literal_Renamed_Field != null)
                    return this.literal_Renamed_Field;

                // this value could be cached in this.literal but it increases memory usage
                return base.Literal;
            }
            set
            {
                /*
                * do only once
                */

                if (this.literal_Renamed_Field == null)
                    this.literal_Renamed_Field = value;
            }

        }
        /* Reference types */
        private const int NORMAL_REFERENCE = 1;
        private const int FORMAL_REFERENCE = 2;
        private const int QUIET_REFERENCE = 3;
        private const int RUNT = 4;

        private int referenceType;
        private string nullString;
        private string rootString;
        private bool escaped = false;
        private bool computableReference = true;
        private bool logOnNull = true;
        private string escPrefix = "";
        private string morePrefix = "";
        private string identifier = "";

        private string literal_Renamed_Field = null;

        /// <summary> Indicates if we are running in strict reference mode.</summary>
        public bool strictRef = false;

        private int numChildren = 0;

        protected internal Info uberInfo;

        /// <param name="id">
        /// </param>
        public ASTReference(int id)
            : base(id)
        {
        }

        /// <param name="p">
        /// </param>
        /// <param name="id">
        /// </param>
        public ASTReference(Parser p, int id)
            : base(p, id)
        {
        }

        /// <seealso cref="SimpleNode.jjtAccept(org.apache.veloobjectser.node.ParserVisitor, java.lang.Object)">
        /// </seealso>
        public override object Accept(IParserVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <seealso cref="SimpleNode.init(org.apache.vobjectInternalContextAdapter, java.lang.Object)">
        /// </seealso>
        public override object Init(IInternalContextAdapter context, object data)
        {
            /*
            *  init our children
            */

            base.Init(context, data);

            /*
            *  the only thing we can do in init() is getRoot()
            *  as that is template based, not context based,
            *  so it's thread- and context-safe
            */

            this.rootString = this.Root;

            this.numChildren = this.GetNumChildren();

            /*
            * and if appropriate...
            */

            if (this.numChildren > 0)
            {
                this.identifier = this.GetChild(this.numChildren - 1).FirstToken.Image;
            }

            /*
            * make an uberinfo - saves new's later on
            */

            this.uberInfo = new Info(this.TemplateName, this.Line, this.Column);

            /*
            * track whether we log invalid references
            */
            this.logOnNull = this.rsvc.GetBoolean(RuntimeConstants.RUNTIME_LOG_REFERENCE_LOG_INVALID, true);

            this.strictRef = this.rsvc.GetBoolean(RuntimeConstants.RUNTIME_REFERENCES_STRICT, false);

            /**
            * In the case we are referencing a variable with #if($foo) or
            * #if( ! $foo) then we allow variables to be undefined and we 
            * set strictRef to false so that if the variable is undefined
            * an exception is not thrown. 
            */
            if (this.strictRef && this.numChildren == 0)
            {
                this.logOnNull = false; // Strict mode allows nulls

                INode node = this.Parent;
                if (node is ASTNotNode || node is ASTExpression || node is ASTOrNode || node is ASTAndNode)
                // #if( $foo && ...
                {
                    // Now scan up tree to see if we are in an If statement
                    while (node != null)
                    {
                        if (node is ASTIfStatement)
                        {
                            this.strictRef = false;
                            break;
                        }
                        node = node.Parent;
                    }
                }
            }

            return data;
        }

        /// <summary>   gets an Object that 'is' the value of the reference
        /// 
        /// </summary>
        /// <param name="o">  unused Object parameter
        /// </param>
        /// <param name="context">context used to generate value
        /// </param>
        /// <returns> The execution result.
        /// </returns>
        /// <throws>  MethodInvocationException </throws>
        public override object Execute(object o, IInternalContextAdapter context)
        {

            if (this.referenceType == RUNT)
                return null;

            /*
            *  get the root object from the context
            */

            object result = this.GetVariableValue(context, this.rootString);

            if (result == null && !this.strictRef)
            {
                return EventHandlerUtil.InvalidGetMethod(this.rsvc, context, "$" + this.rootString, (object)null, null, this.uberInfo);
            }

            /*
            * Iteratively work 'down' (it's flat...) the reference
            * to get the value, but check to make sure that
            * every result along the path is valid. For example:
            *
            * $hashtable.Customer.Name
            *
            * The $hashtable may be valid, but there is no key
            * 'Customer' in the hashtable so we want to stop
            * when we find a null value and return the null
            * so the error gets logged.
            */

            try
            {
                object previousResult = result;
                int failedChild = -1;
                for (int i = 0; i < this.numChildren; i++)
                {
                    if (this.strictRef && result == null)
                    {
                        /**
                        * At this point we know that an attempt is about to be made
                        * to call a method or property on a null value.
                        */
                        string name = this.GetChild(i).FirstToken.Image;
                        throw new VelocityException("Attempted to access '" + name + "' on a null value at " + Log.Log.FormatFileString(this.uberInfo.TemplateName, +this.GetChild(i).Line, this.GetChild(i).Column));
                    }
                    previousResult = result;
                    result = this.GetChild(i).Execute(result, context);
                    if (result == null && !this.strictRef)
                    // If strict and null then well catch this
                    // next time through the loop
                    {
                        failedChild = i;
                        break;
                    }
                }

                if (result == null)
                {
                    if (failedChild == -1)
                    {
                        result = EventHandlerUtil.InvalidGetMethod(this.rsvc, context, "$" + this.rootString, previousResult, null, this.uberInfo);
                    }
                    else
                    {
                        System.Text.StringBuilder name = new System.Text.StringBuilder("$").Append(this.rootString);
                        for (int i = 0; i <= failedChild; i++)
                        {
                            INode node = this.GetChild(i);
                            if (node is ASTMethod)
                            {
                                name.Append(".").Append(((ASTMethod)node).MethodName).Append("()");
                            }
                            else
                            {
                                name.Append(".").Append(node.FirstToken.Image);
                            }
                        }

                        if (this.GetChild(failedChild) is ASTMethod)
                        {
                            string methodName = ((ASTMethod)this.GetChild(failedChild)).MethodName;
                            result = EventHandlerUtil.InvalidMethod(this.rsvc, context, name.ToString(), previousResult, methodName, this.uberInfo);
                        }
                        else
                        {
                            string property = this.GetChild(failedChild).FirstToken.Image;
                            result = EventHandlerUtil.InvalidGetMethod(this.rsvc, context, name.ToString(), previousResult, property, this.uberInfo);
                        }
                    }
                }

                return result;
            }
            catch (MethodInvocationException mie)
            {
                mie.ReferenceName = this.rootString;
                throw mie;
            }
        }

        /// <summary>  gets the value of the reference and outputs it to the
        /// writer.
        /// 
        /// </summary>
        /// <param name="context"> context of data to use in getting value
        /// </param>
        /// <param name="writer">  writer to render to
        /// </param>
        /// <returns> True if rendering was successful.
        /// </returns>
        /// <throws>  IOException </throws>
        /// <throws>  MethodInvocationException </throws>
        public override bool Render(IInternalContextAdapter context, TextWriter writer)
        {
            if (this.referenceType == RUNT)
            {
                if (context.AllowRendering)
                {
                    writer.Write(this.rootString);
                }

                return true;
            }

            object value = this.Execute((object)null, context);

            string localNullString = null;

            /*
            * if this reference is escaped (\$foo) then we want to do one of two things : 1) if this is
            * a reference in the context, then we want to print $foo 2) if not, then \$foo (its
            * considered schmoo, not VTL)
            */

            if (this.escaped)
            {
                localNullString = this.GetNullString(context);

                if (value == null)
                {
                    if (context.AllowRendering)
                    {
                        writer.Write(this.escPrefix);
                        writer.Write("\\");
                        writer.Write(localNullString);
                    }
                }
                else
                {
                    if (context.AllowRendering)
                    {
                        writer.Write(this.escPrefix);
                        writer.Write(localNullString);
                    }
                }
                return true;
            }

            /*
            * the normal processing
            * 
            * if we have an event cartridge, get a new value object
            */

            value = EventHandlerUtil.ReferenceInsert(this.rsvc, context, this.Literal, value);

            string toString = null;
            if (value != null)
            {

                if (value is IRenderable && ((IRenderable)value).Render(context, writer))
                {
                    return true;
                }

                //UPGRADE_TODO: �� .NET �У�������java.lang.Object.toString���ĵ�Ч����ܷ��ز�ͬ��ֵ�� "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
                toString = value.ToString();
            }

            if (value == null || toString == null)
            {
                /*
                * write prefix twice, because it's schmoo, so the \ don't escape each other...
                */

                if (context.AllowRendering)
                {
                    localNullString = this.GetNullString(context);

                    writer.Write(this.escPrefix);
                    writer.Write(this.escPrefix);
                    writer.Write(this.morePrefix);
                    writer.Write(localNullString);
                }

                if (this.logOnNull && this.referenceType != QUIET_REFERENCE && this.log.DebugEnabled)
                {
                    this.log.Debug("Null reference [template '" + this.TemplateName + "', line " + this.Line + ", column " + this.Column + "] : " + this.Literal + " cannot be resolved.");
                }
                return true;
            }
            else
            {
                /*
                * non-null processing
                */

                if (context.AllowRendering)
                {
                    writer.Write(this.escPrefix);
                    writer.Write(this.morePrefix);
                    writer.Write(toString);
                }

                return true;
            }
        }

        /// <summary> This method helps to implement the "render literal if null" functionality.
        /// 
        /// VelocimacroProxy saves references to macro arguments (AST nodes) so that if we have a macro
        /// #foobar($a $b) then there is key "$a.literal" which points to the literal presentation of the
        /// argument provided to variable $a. If the value of $a is null, we render the string that was
        /// provided as the argument.
        /// 
        /// </summary>
        /// <param name="context">
        /// </param>
        /// <returns>
        /// </returns>
        private string GetNullString(IInternalContextAdapter context)
        {
            object callingArgument = context.Get(".literal." + this.nullString);

            if (callingArgument != null)
                return ((INode)callingArgument).Literal;
            else
                return this.nullString;
        }

        /// <summary>   Computes boolean value of this reference
        /// Returns the actual value of reference return type
        /// boolean, and 'true' if value is not null
        /// 
        /// </summary>
        /// <param name="context">context to compute value with
        /// </param>
        /// <returns> True if evaluation was ok.
        /// </returns>
        /// <throws>  MethodInvocationException </throws>
        public override bool Evaluate(IInternalContextAdapter context)
        {
            object value = this.Execute((object)null, context);

            if (value == null)
            {
                return false;
            }
            else if (value is System.Boolean)
            {
                if (((System.Boolean)value))
                    return true;
                else
                    return false;
            }
            else
            {
                try
                {
                    return value.ToString() != null;
                }
                catch (System.Exception e)
                {
                    throw new VelocityException("Reference evaluation threw an exception at " + Log.Log.FormatFileString(this), e);
                }
            }
        }

        /// <seealso cref="SimpleNode.value(org.apache.velocity.context.InternalContextAdapter)">
        /// </seealso>
        public override object Value(IInternalContextAdapter context)
        {
            return (this.computableReference ? this.Execute((object)null, context) : null);
        }

        /// <summary>  Sets the value of a complex reference (something like $foo.bar)
        /// Currently used by ASTSetReference()
        /// 
        /// </summary>
        /// <seealso cref="ASTSetDirective">
        /// 
        /// </seealso>
        /// <param name="context">context object containing this reference
        /// </param>
        /// <param name="value">Object to set as value
        /// </param>
        /// <returns> true if successful, false otherwise
        /// </returns>
        /// <throws>  MethodInvocationException </throws>
        public virtual bool SetValue(IInternalContextAdapter context, object value)
        {
            if (this.GetNumChildren() == 0)
            {
                context.Put(this.rootString, value);
                return true;
            }

            /*
            *  The rootOfIntrospection is the object we will
            *  retrieve from the Context. This is the base
            *  object we will apply reflection to.
            */

            object result = this.GetVariableValue(context, this.rootString);

            if (result == null)
            {
                string msg = "reference set is not a valid reference at " + Log.Log.FormatFileString(this.uberInfo);
                this.log.Error(msg);
                return false;
            }

            /*
            * How many child nodes do we have?
            */

            for (int i = 0; i < this.numChildren - 1; i++)
            {
                result = this.GetChild(i).Execute(result, context);

                if (result == null)
                {
                    if (this.strictRef)
                    {
                        string name = this.GetChild(i + 1).FirstToken.Image;
                        throw new MethodInvocationException("Attempted to access '" + name + "' on a null value", null, name, this.uberInfo.TemplateName, this.GetChild(i + 1).Line, this.GetChild(i + 1).Column);
                    }

                    string msg = "reference set is not a valid reference at " + Log.Log.FormatFileString(this.uberInfo);
                    this.log.Error(msg);

                    return false;
                }
            }

            /*
            *  We support two ways of setting the value in a #set($ref.foo = $value ) :
            *  1) ref.setFoo( value )
            *  2) ref,put("foo", value ) to parallel the get() map introspection
            */

            try
            {
                IVelPropertySet vs = this.rsvc.Uberspect.GetPropertySet(result, this.identifier, value, this.uberInfo);

                if (vs == null)
                {
                    if (this.strictRef)
                    {
                        throw new MethodInvocationException("Object '" + result.GetType().FullName + "' does not contain property '" + this.identifier + "'", null, this.identifier, this.uberInfo.TemplateName, this.uberInfo.Line, this.uberInfo.Column);
                    }
                    else
                    {
                        return false;
                    }
                }

                vs.Invoke(result, value);
            }
            catch (TargetInvocationException ite)
            {
                /*
                *  this is possible
                */

                throw new MethodInvocationException("ASTReference : Invocation of method '" + this.identifier + "' in  " + result.GetType() + " threw exception " + ite.GetBaseException().ToString(), ite.GetBaseException(), this.identifier, this.TemplateName, this.Line, this.Column);
            }
            /**
            * pass through application level runtime exceptions
            */
            catch (System.SystemException e)
            {
                throw e;
            }
            catch (System.Exception e)
            {
                /*
                *  maybe a security exception?
                */
                string msg = "ASTReference setValue() : exception : " + e + " template at " + Log.Log.FormatFileString(this.uberInfo);
                this.log.Error(msg, e);
                throw new VelocityException(msg, e);
            }

            return true;
        }

        /// <param name="context">
        /// </param>
        /// <param name="variable">
        /// </param>
        /// <returns> The evaluated value of the variable.
        /// </returns>
        /// <throws>  MethodInvocationException </throws>
        public virtual object GetVariableValue(IContext context, string variable)
        {
            object obj = null;
            try
            {
                obj = context.Get(variable);
            }
            catch (System.SystemException e)
            {
                this.log.Error("Exception calling reference $" + variable + " at " + Log.Log.FormatFileString(this.uberInfo));
                throw e;
            }

            if (this.strictRef && obj == null)
            {
                if (!context.ContainsKey(variable))
                {
                    this.log.Error("Variable $" + variable + " has not been set at " + Log.Log.FormatFileString(this.uberInfo));
                    throw new MethodInvocationException("Variable $" + variable + " has not been set", null, this.identifier, this.uberInfo.TemplateName, this.uberInfo.Line, this.uberInfo.Column);
                }
            }
            return obj;
        }
    }
}