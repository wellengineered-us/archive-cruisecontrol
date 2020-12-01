// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Commons.Collections
{
	/// <summary>
    /// 
    /// </summary>
	public class StringTokenizer
	{
		private List<string> elements;
		private string source;
		//The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character
		private string delimiters = " \t\n\r";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
		public StringTokenizer(string source)
		{
			this.elements = new List<string>();
			this.elements.AddRange(source.Split(this.delimiters.ToCharArray()));
			this.RemoveEmptyStrings();
			this.source = source;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="delimiters"></param>
		public StringTokenizer(string source, string delimiters)
		{
			this.elements = new List<string>();
			this.delimiters = delimiters;
			this.elements.AddRange(source.Split(this.delimiters.ToCharArray()));
			this.RemoveEmptyStrings();
			this.source = source;
		}

        /// <summary>
        /// 
        /// </summary>
		public int Count
		{
			get { return (this.elements.Count); }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public virtual bool HasMoreTokens()
		{
			return (this.elements.Count > 0);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public virtual string NextToken()
		{
			if (this.source == string.Empty)
			{
				throw new System.Exception();
			}
			else
			{
				this.elements = new List<string>();
				this.elements.AddRange(this.source.Split(this.delimiters.ToCharArray()));
				this.RemoveEmptyStrings();
				string result = this.elements[0];
				this.elements.RemoveAt(0);
				this.source = this.source.Replace(result, string.Empty);
				this.source = this.source.TrimStart(this.delimiters.ToCharArray());
				return result;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delimiters"></param>
        /// <returns></returns>
		public string NextToken(string delimiters)
		{
			this.delimiters = delimiters;
			return this.NextToken();
		}

		private void RemoveEmptyStrings()
		{
			//VJ++ does not treat empty strings as tokens
			for(int index = 0; index < this.elements.Count; index++)
				if (this.elements[index] == string.Empty)
				{
					this.elements.RemoveAt(index);
					index--;
				}
		}
	}
}