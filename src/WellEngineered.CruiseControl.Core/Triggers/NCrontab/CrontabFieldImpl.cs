#region License, Terms and Author(s)
//
// NCrontab - Crontab for .NET
// Copyright (c) 2008 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the New BSD License, a copy of which should have 
// been delivered along with this distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT 
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
#endregion

using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;

namespace WellEngineered.CruiseControl.Core.Triggers.NCrontab
{
    #region Imports

	using Debug = System.Diagnostics.Debug;

    #endregion

    /// <summary>
    /// 	
    /// </summary>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    /// <param name="interval">The interval.</param>
    public delegate void CrontabFieldAccumulator(int start, int end, int interval);

    /// <summary>
    /// 	
    /// </summary>
    [ Serializable ]
    public sealed class CrontabFieldImpl : IObjectReference
    {
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public static CrontabFieldImpl Minute    = new CrontabFieldImpl(CrontabFieldKind.Minute, 0, 59, null);
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public static CrontabFieldImpl Hour      = new CrontabFieldImpl(CrontabFieldKind.Hour, 0, 23, null);
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public static CrontabFieldImpl Day       = new CrontabFieldImpl(CrontabFieldKind.Day, 1, 31, null);
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public static CrontabFieldImpl Month     = new CrontabFieldImpl(CrontabFieldKind.Month, 1, 12, new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" });
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public static CrontabFieldImpl DayOfWeek = new CrontabFieldImpl(CrontabFieldKind.DayOfWeek, 0, 6, new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" });

        private static readonly CrontabFieldImpl[] _fieldByKind = new[] { Minute, Hour, Day, Month, DayOfWeek };

        private static readonly CompareInfo _comparer = CultureInfo.InvariantCulture.CompareInfo;
        private static readonly char[] _comma = new[] { ',' };

        private readonly CrontabFieldKind _kind;
        private readonly int _minValue;
        private readonly int _maxValue;
        private readonly string[] _names;

        /// <summary>
        /// Froms the kind.	
        /// </summary>
        /// <param name="kind">The kind.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static CrontabFieldImpl FromKind(CrontabFieldKind kind)
        {
            if (!Enum.IsDefined(typeof(CrontabFieldKind), kind))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture, "Invalid crontab field kind. Valid values are {0}.",
                    string.Join(", ", Enum.GetNames(typeof(CrontabFieldKind)))), "kind");
            }

            return _fieldByKind[(int) kind];
        }

        private CrontabFieldImpl(CrontabFieldKind kind, int minValue, int maxValue, string[] names)
        {
            Debug.Assert(Enum.IsDefined(typeof(CrontabFieldKind), kind));
            Debug.Assert(minValue >= 0);
            Debug.Assert(maxValue >= minValue);
            Debug.Assert(names == null || names.Length == (maxValue - minValue + 1));

            this._kind = kind;
            this._minValue = minValue;
            this._maxValue = maxValue;
            this._names = names;
        }

        /// <summary>
        /// Gets the kind.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public CrontabFieldKind Kind
        {
            get { return this._kind; }
        }

        /// <summary>
        /// Gets the min value.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public int MinValue
        {
            get { return this._minValue; }
        }

        /// <summary>
        /// Gets the max value.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public int MaxValue
        {
            get { return this._maxValue; }
        }

        /// <summary>
        /// Gets the value count.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public int ValueCount
        {
            get { return this._maxValue - this._minValue + 1; }
        }

        /// <summary>
        /// Formats the specified field.	
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="writer">The writer.</param>
        /// <remarks></remarks>
        public void Format(ICrontabField field, TextWriter writer)
        {
            this.Format(field, writer, false);
        }

        /// <summary>
        /// Formats the specified field.	
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="noNames">The no names.</param>
        /// <remarks></remarks>
        public void Format(ICrontabField field, TextWriter writer, bool noNames)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            if (writer == null)
                throw new ArgumentNullException("writer");

            var next = field.GetFirst();
            var count = 0;

            while (next != -1)
            {
                var first = next;
                int last;

                do
                {
                    last = next;
                    next = field.Next(last + 1);
                }
                while (next - last == 1);

                if (count == 0 
                    && first == this._minValue && last == this._maxValue)
                {
                    writer.Write('*');
                    return;
                }
                
                if (count > 0)
                    writer.Write(',');

                if (first == last)
                {
                    this.FormatValue(first, writer, noNames);
                }
                else
                {
                    this.FormatValue(first, writer, noNames);
                    writer.Write('-');
                    this.FormatValue(last, writer, noNames);
                }

                count++;
            }
        }

        private void FormatValue(int value, TextWriter writer, bool noNames)
        {
            Debug.Assert(writer != null);

            if (noNames || this._names == null)
            {
                if (value >= 0 && value < 100)
                {
                    FastFormatNumericValue(value, writer);
                }
                else
                {
                    writer.Write(value.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                var index = value - this._minValue;
                writer.Write(this._names[index]);
            }
        }

        private static void FastFormatNumericValue(int value, TextWriter writer)
        {
            Debug.Assert(value >= 0 && value < 100);
            Debug.Assert(writer != null);

            if (value >= 10)
            {
                writer.Write((char) ('0' + (value / 10)));
                writer.Write((char) ('0' + (value % 10)));
            }
            else
            {
                writer.Write((char) ('0' + value));
            }
        }

        /// <summary>
        /// Parses the specified STR.	
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="acc">The acc.</param>
        /// <remarks></remarks>
        public void Parse(string str, CrontabFieldAccumulator acc)
        {
            if (acc == null)
                throw new ArgumentNullException("acc");

            if (string.IsNullOrEmpty(str))
                return;

            try
            {
                this.InternalParse(str, acc);
            }
            catch (FormatException e)
            {
                ThrowParseException(e, str);
            }
            catch (CrontabException e)
            {
                ThrowParseException(e, str);
            }
        }

        private static void ThrowParseException(Exception innerException, string str)
        {
            Debug.Assert(str != null);
            Debug.Assert(innerException != null);

            throw new CrontabException(string.Format(System.Globalization.CultureInfo.CurrentCulture,"'{0}' is not a valid crontab field expression.", str), innerException);
        }

        private void InternalParse(string str, CrontabFieldAccumulator acc)
        {
            Debug.Assert(str != null);
            Debug.Assert(acc != null);

            if (str.Length == 0)
                throw new CrontabException("A crontab field value cannot be empty.");

            //
            // Next, look for a list of values (e.g. 1,2,3).
            //
    
            var commaIndex = str.IndexOf(",");

            if (commaIndex > 0)
            {
                foreach (var token in str.Split(_comma))
                    this.InternalParse(token, acc);
            }
            else
            {
                var every = 1;

                //
                // Look for stepping first (e.g. */2 = every 2nd).
                // 

                var slashIndex = str.IndexOf("/");

                if (slashIndex > 0)
                {
                    every = int.Parse(str.Substring(slashIndex + 1), CultureInfo.InvariantCulture);
                    str = str.Substring(0, slashIndex);
                }

                //
                // Next, look for wildcard (*).
                //
    
                if (str.Length == 1 && str[0]== '*')
                {
                    acc(-1, -1, every);
                    return;
                }

                //
                // Next, look for a range of values (e.g. 2-10).
                //

                var dashIndex = str.IndexOf("-");
        
                if (dashIndex > 0)
                {
                    var first = this.ParseValue(str.Substring(0, dashIndex));
                    var last = this.ParseValue(str.Substring(dashIndex + 1));
        
                    acc(first, last, every);
                    return;
                }

                //
                // Finally, handle the case where there is only one number.
                //

                var value = this.ParseValue(str);

                if (every == 1)
                {
                    acc(value, value, 1);
                }
                else
                {
                    Debug.Assert(every != 0);

                    acc(value, this._maxValue, every);
                }
            }
        }

        private int ParseValue(string str)
        {
            Debug.Assert(str != null);

            if (str.Length == 0)
                throw new CrontabException("A crontab field value cannot be empty.");

            var firstChar = str[0];
        
            if (firstChar >= '0' && firstChar <= '9')
                return int.Parse(str, CultureInfo.InvariantCulture);

            if (this._names == null)
            {
                throw new CrontabException(string.Format(
                    CultureInfo.CurrentCulture, "'{0}' is not a valid value for this crontab field. It must be a numeric value between {1} and {2} (all inclusive).",
                    str, this._minValue.ToString(CultureInfo.CurrentCulture), this._maxValue.ToString()));
            }

            for (var i = 0; i < this._names.Length; i++)
            {
                if (_comparer.IsPrefix(this._names[i], str, CompareOptions.IgnoreCase))
                    return i + this._minValue;
            }

            throw new CrontabException(string.Format(
                CultureInfo.CurrentCulture, "'{0}' is not a known value name. Use one of the following: {1}.", 
                str, string.Join(", ", this._names)));
        }

        object IObjectReference.GetRealObject(StreamingContext context)
        {
            return FromKind(this.Kind);
        }
    }
}
