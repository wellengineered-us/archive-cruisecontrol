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

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Exception
{
	/// <summary> Separate exception class to distinguish math problems.   
    /// 
    /// </summary>
    /// <author>  Nathan Bubna
    /// </author>
    /// <since> 1.6
    /// </since>
    /// <version>  $Id: MathException.java 685685 2008-08-13 21:43:27Z nbubna $
    /// </version>
    [Serializable]
    public class MathException : VelocityException
    {
        public MathException(string exceptionMessage)
            : base(exceptionMessage)
        {
        }
    }
}
