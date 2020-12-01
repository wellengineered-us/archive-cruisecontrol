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

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Util.Introspection;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Exception
{
	/// <summary>  Application-level exception thrown when a resource of any type
    /// has a syntax or other Error which prevents it from being parsed.
    /// <br>
    /// When this resource is thrown, a best effort will be made to have
    /// useful information in the exception's message.  For complete
    /// information, consult the runtime Log.
    /// 
    /// </summary>
    /// <author>  <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
    /// </author>
    /// <author>  <a href="hps@intermeta.de">Henning P. Schmiedehausen</a>
    /// </author>
    /// <version>  $Id: ParseErrorException.java 685685 2008-08-13 21:43:27Z nbubna $
    /// </version>
    [Serializable]
    public class ParseErrorException : VelocityException
    {
        /// <summary> Return the column number of the parsing Error, or -1 if not defined.
        /// 
        /// </summary>
        /// <returns> column number of the parsing Error, or -1 if not defined
        /// </returns>
        /// <since> 1.5
        /// </since>
        virtual public int ColumnNumber
        {
            get
            {
                return this.columnNumber;
            }

        }
        /// <summary> Return the line number of the parsing Error, or -1 if not defined.
        /// 
        /// </summary>
        /// <returns> line number of the parsing Error, or -1 if not defined
        /// </returns>
        /// <since> 1.5
        /// </since>
        virtual public int LineNumber
        {
            get
            {
                return this.lineNumber;
            }

        }
        /// <summary> Return the name of the template containing the Error, or null if not
        /// defined.
        /// 
        /// </summary>
        /// <returns> the name of the template containing the parsing Error, or null
        /// if not defined
        /// </returns>
        /// <since> 1.5
        /// </since>
        virtual public string TemplateName
        {
            get
            {
                return this.templateName;
            }

        }
        /// <summary> Return the invalid syntax or reference that triggered this Error, or null
        /// if not defined.
        /// 
        /// </summary>
        /// <returns> Return the invalid syntax or reference that triggered this Error, or null
        /// if not defined
        /// </returns>
        /// <since> 1.5
        /// </since>
        virtual public string InvalidSyntax
        {
            get
            {
                return this.invalidSyntax;
            }

        }

        /// <summary> The column number of the parsing Error, or -1 if not defined.</summary>
        private int columnNumber = -1;

        /// <summary> The line number of the parsing Error, or -1 if not defined.</summary>
        private int lineNumber = -1;

        /// <summary> The name of the template containing the Error, or null if not defined.</summary>
        private string templateName = "*unset*";

        /// <summary> If applicable, contains the invalid syntax or reference that triggered this exception</summary>
        private string invalidSyntax;

        /// <summary> Create a ParseErrorException with the given message.
        /// 
        /// </summary>
        /// <param name="exceptionMessage">the Error exception message
        /// </param>
        public ParseErrorException(string exceptionMessage)
            : base(exceptionMessage)
        {
        }

        /// <summary> Create a ParseErrorException with the given ParseException.
        /// 
        /// </summary>
        /// <param name="pex">the parsing exception
        /// </param>
        /// <since> 1.5
        /// </since>
        public ParseErrorException(ParseException pex)
            : base(pex.Message)
        {

            // Don't use a second C'tor, TemplateParseException is a subclass of
            // ParseException...
            if (pex is IExtendedParseException)
            {
                IExtendedParseException xpex = (IExtendedParseException)pex;

                this.columnNumber = xpex.ColumnNumber;
                this.lineNumber = xpex.LineNumber;
                this.templateName = xpex.TemplateName;
            }
            else
            {
                //  ugly, ugly, ugly...

                if (pex.currentToken != null && pex.currentToken.Next != null)
                {
                    this.columnNumber = pex.currentToken.Next.BeginColumn;
                    this.lineNumber = pex.currentToken.Next.BeginLine;
                }
            }
        }

        /// <summary> Create a ParseErrorException with the given ParseException.
        /// 
        /// </summary>
        /// <param name="pex">the parsing exception
        /// </param>
        /// <since> 1.5
        /// </since>
        public ParseErrorException(VelocityException pex)
            : base(pex.Message)
        {

            // Don't use a second C'tor, TemplateParseException is a subclass of
            // ParseException...
            if (pex is IExtendedParseException)
            {
                IExtendedParseException xpex = (IExtendedParseException)pex;

                this.columnNumber = xpex.ColumnNumber;
                this.lineNumber = xpex.LineNumber;
                this.templateName = xpex.TemplateName;
            }
            else if (pex.GetType().Equals(typeof(ParseException)))
            {
                ParseException pex2 = (ParseException)pex.InnerException;

                if (pex2.currentToken != null && pex2.currentToken.Next != null)
                {
                    this.columnNumber = pex2.currentToken.Next.BeginColumn;
                    this.lineNumber = pex2.currentToken.Next.BeginLine;
                }
            }
        }


        /// <summary> Create a ParseErrorRuntimeException with the given message and Info
        /// 
        /// </summary>
        /// <param name="exceptionMessage">the Error exception message
        /// </param>
        /// <param name="Info">an Info object with the current template Info
        /// </param>
        /// <since> 1.5
        /// </since>
        public ParseErrorException(string exceptionMessage, Info info)
            : base(exceptionMessage)
        {
            this.columnNumber = info.Column;
            this.lineNumber = info.Line;
            this.templateName = info.TemplateName;
        }

        /// <summary> Create a ParseErrorRuntimeException with the given message and Info
        /// 
        /// </summary>
        /// <param name="exceptionMessage">the Error exception message
        /// </param>
        /// <param name="Info">an Info object with the current template Info
        /// </param>
        /// <param name="invalidSyntax">the invalid syntax or reference triggering this exception
        /// </param>
        /// <since> 1.5
        /// </since>
        public ParseErrorException(string exceptionMessage, Info info, string invalidSyntax)
            : base(exceptionMessage)
        {
            this.columnNumber = info.Column;
            this.lineNumber = info.Line;
            this.templateName = info.TemplateName;
            this.invalidSyntax = invalidSyntax;
        }
    }
}