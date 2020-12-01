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

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Exception;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Directive
{
	/// <summary>  Exception to indicate problem happened while constructing #macro()
    /// 
    /// For internal use in parser - not to be passed to app level
    /// 
    /// </summary>
    /// <author>  <a href="mailto:geirm@apache.org">Geir Magnusson Jr.</a>
    /// </author>
    /// <author>  <a href="hps@intermeta.de">Henning P. Schmiedehausen</a>
    /// </author>
    /// <version>  $Id: MacroParseException.java 736127 2009-01-20 21:59:00Z byron $
    /// </version>
    [Serializable]
    public class MacroParseException : ParseException, IExtendedParseException
    {
        /// <summary> returns the Template name where this exception occured.</summary>
        /// <returns> The Template name where this exception occured.
        /// </returns>
        /// <since> 1.5
        /// </since>
        public virtual string TemplateName
        {
            get
            {
                return this.templateName;
            }

        }
        /// <summary> returns the line number where this exception occured.</summary>
        /// <returns> The line number where this exception occured.
        /// </returns>
        /// <since> 1.5
        /// </since>
        public virtual int LineNumber
        {
            get
            {
                if ((this.currentToken != null) && (this.currentToken.Next != null))
                {
                    return this.currentToken.Next.BeginLine;
                }
                else if (this.currentToken != null)
                {
                    return this.currentToken.BeginLine;
                }
                else
                {
                    return -1;
                }
            }

        }
        /// <summary> returns the column number where this exception occured.</summary>
        /// <returns> The column number where this exception occured.
        /// </returns>
        /// <since> 1.5
        /// </since>
        public virtual int ColumnNumber
        {
            get
            {
                if ((this.currentToken != null) && (this.currentToken.Next != null))
                {
                    return this.currentToken.Next.BeginColumn;
                }
                else if (this.currentToken != null)
                {
                    return this.currentToken.BeginColumn;
                }
                else
                {
                    return -1;
                }
            }

        }
        /// <summary> This method has the standard behavior when this object has been
        /// created using the standard constructors.  Otherwise, it uses
        /// "currentToken" and "expectedTokenSequences" to generate a parse
        /// Error message and returns it.  If this object has been created
        /// due to a parse Error, and you do not catch it (it gets thrown
        /// from the parser), then this method is called during the printing
        /// of the final stack Trace, and hence the correct Error message
        /// gets displayed.
        /// </summary>
        /// <returns> the current message.
        /// </returns>
        /// <since> 1.5
        /// </since>
        public override string Message
        {
            get
            {
                if (!this.specialConstructor)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder(base.Message);
                    this.AppendTemplateInfo(sb);
                    return sb.ToString();
                }

                int maxSize = 0;

                System.Text.StringBuilder expected = new System.Text.StringBuilder();

                for (int i = 0; i < this.expectedTokenSequences.Length; i++)
                {
                    if (maxSize < this.expectedTokenSequences[i].Length)
                    {
                        maxSize = this.expectedTokenSequences[i].Length;
                    }

                    for (int j = 0; j < this.expectedTokenSequences[i].Length; j++)
                    {
                        expected.Append(this.tokenImage[this.expectedTokenSequences[i][j]]).Append(" ");
                    }

                    if (this.expectedTokenSequences[i][this.expectedTokenSequences[i].Length - 1] != 0)
                    {
                        expected.Append("...");
                    }

                    expected.Append(this.eol).Append("    ");
                }

                System.Text.StringBuilder retval = new System.Text.StringBuilder("Encountered \"");
                Token tok = this.currentToken.Next;

                for (int i = 0; i < maxSize; i++)
                {
                    if (i != 0)
                    {
                        retval.Append(" ");
                    }

                    if (tok.Kind == 0)
                    {
                        retval.Append(this.tokenImage[0]);
                        break;
                    }

                    retval.Append(this.AddEscapes(tok.Image));
                    tok = tok.Next;
                }

                retval.Append("\"");
                this.AppendTemplateInfo(retval);

                if (this.expectedTokenSequences.Length == 1)
                {
                    retval.Append("Was expecting:").Append(this.eol).Append("    ");
                }
                else
                {
                    retval.Append("Was expecting one of:").Append(this.eol).Append("    ");
                }

                // avoid JDK 1.3 StringBuffer.append(Object instance) vs 1.4 StringBuffer.append(StringBuffer sb) gotcha.
                retval.Append(expected.ToString());
                return retval.ToString();
            }

        }

        private readonly string templateName;


        /// <param name="msg">
        /// </param>
        /// <param name="templateName">
        /// </param>
        /// <param name="currentToken">
        /// </param>
        public MacroParseException(string msg, string templateName, Token currentToken)
            : base(msg)
        {
            this.currentToken = currentToken;
            this.templateName = templateName;
        }

        /// <param name="sb">
        /// </param>
        /// <since> 1.5
        /// </since>
        protected internal virtual void AppendTemplateInfo(System.Text.StringBuilder sb)
        {
            sb.Append(Log.Log.FormatFileString(this.TemplateName, this.LineNumber, this.ColumnNumber));
            sb.Append(this.eol);
        }
    }
}
