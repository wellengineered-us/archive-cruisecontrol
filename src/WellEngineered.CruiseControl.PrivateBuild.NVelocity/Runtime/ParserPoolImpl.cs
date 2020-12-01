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

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Util;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime
{
	/// <summary> This wraps the original parser SimplePool class.  It also handles
    /// instantiating ad-hoc parsers if none are available.
    /// 
    /// </summary>
    /// <author>  <a href="mailto:sergek@lokitech.com">Serge Knystautas</a>
    /// </author>
    /// <version>  $Id: RuntimeInstance.java 384374 2006-03-08 23:19:30Z nbubna $
    /// </version>
    /// <since> 1.5
    /// </since>
    public class ParserPoolImpl : IParserPool
    {

        internal SimplePool<Parser.Parser> pool;
        internal int max = NVelocity.Runtime.RuntimeConstants.NUMBER_OF_PARSERS;

        /// <summary> Create the underlying "pool".</summary>
        /// <param name="rsvc">
        /// </param>
        public virtual void Initialize(IRuntimeServices rsvc)
        {
            this.max = rsvc.GetInt(NVelocity.Runtime.RuntimeConstants.PARSER_POOL_SIZE, NVelocity.Runtime.RuntimeConstants.NUMBER_OF_PARSERS);
            this.pool = new SimplePool<Parser.Parser>(this.max);

            for (int i = 0; i < this.max; i++)
            {
                this.pool.Put(rsvc.CreateNewParser());
            }

            if (rsvc.Log.DebugEnabled)
            {
                rsvc.Log.Debug("Created '" + this.max + "' parsers.");
            }
        }

        /// <summary> Call the wrapped pool.  If none are available, it will create a new
        /// temporary one.
        /// </summary>
        /// <returns> A parser Object.
        /// </returns>
        public virtual Parser.Parser Get()
        {
            return this.pool.Get();
        }

        /// <summary> Call the wrapped pool.</summary>
        /// <param name="parser">
        /// </param>
        public virtual void Put(Parser.Parser parser)
        {
            parser.ReInit((ICharStream)null);
            this.pool.Put(parser);
        }
    }
}
