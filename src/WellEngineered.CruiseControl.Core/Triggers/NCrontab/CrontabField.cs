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
using System.Collections;
using System.Globalization;
using System.IO;

namespace WellEngineered.CruiseControl.Core.Triggers.NCrontab
{
    #region Imports

	#endregion

    /// <summary>
    /// Represents a single crontab field.
    /// </summary>

    [ Serializable ]
    public sealed class CrontabField : ICrontabField
    {
        private readonly BitArray _bits;
        private /* readonly */ int _minValueSet;
        private /* readonly */ int _maxValueSet;
        private readonly CrontabFieldImpl _impl;

        /// <summary>
        /// Parses a crontab field expression given its kind.
        /// </summary>

        public static CrontabField Parse(CrontabFieldKind kind, string expression)
        {
            return new CrontabField(CrontabFieldImpl.FromKind(kind), expression);
        }
        
        /// <summary>
        /// Parses a crontab field expression representing minutes.
        /// </summary>

        public static CrontabField Minutes(string expression)
        {
            return new CrontabField(CrontabFieldImpl.Minute, expression);
        }

        /// <summary>
        /// Parses a crontab field expression representing hours.
        /// </summary>

        public static CrontabField Hours(string expression)
        {
            return new CrontabField(CrontabFieldImpl.Hour, expression);
        }

        /// <summary>
        /// Parses a crontab field expression representing days in any given month.
        /// </summary>
        
        public static CrontabField Days(string expression)
        {
            return new CrontabField(CrontabFieldImpl.Day, expression);
        }

        /// <summary>
        /// Parses a crontab field expression representing months.
        /// </summary>

        public static CrontabField Months(string expression)
        {
            return new CrontabField(CrontabFieldImpl.Month, expression);
        }

        /// <summary>
        /// Parses a crontab field expression representing days of a week.
        /// </summary>

        public static CrontabField DaysOfWeek(string expression)
        {
            return new CrontabField(CrontabFieldImpl.DayOfWeek, expression);
        }

        private CrontabField(CrontabFieldImpl impl, string expression)
        {
            if (impl == null)
                throw new ArgumentNullException("impl");

            this._impl = impl;
            this._bits = new BitArray(impl.ValueCount);

            this._bits.SetAll(false);
            this._minValueSet = int.MaxValue;
            this._maxValueSet = -1;

            this._impl.Parse(expression, this.Accumulate);
        }

        /// <summary>
        /// Gets the first value of the field or -1.
        /// </summary>

        public int GetFirst()
        {
            return this._minValueSet < int.MaxValue ? this._minValueSet : -1;
        }

        /// <summary>
        /// Gets the next value of the field that occurs after the given 
        /// start value or -1 if there is no next value available.
        /// </summary>

        public int Next(int start)
        {
            if (start < this._minValueSet)
                return this._minValueSet;

            var startIndex = this.ValueToIndex(start);
            var lastIndex = this.ValueToIndex(this._maxValueSet);

            for (var i = startIndex; i <= lastIndex; i++)
            {
                if (this._bits[i]) 
                    return this.IndexToValue(i);
            }

            return -1;
        }

        private int IndexToValue(int index)
        {
            return index + this._impl.MinValue;
        }

        private int ValueToIndex(int value)
        {
            return value - this._impl.MinValue;
        }

        /// <summary>
        /// Determines if the given value occurs in the field.
        /// </summary>

        public bool Contains(int value)
        {
            return this._bits[this.ValueToIndex(value)];
        }

        /// <summary>
        /// Accumulates the given range (start to end) and interval of values
        /// into the current set of the field.
        /// </summary>
        /// <remarks>
        /// To set the entire range of values representable by the field,
        /// set <param name="start" /> and <param name="end" /> to -1 and
        /// <param name="interval" /> to 1.
        /// </remarks>

        private void Accumulate(int start, int end, int interval)
        {
            var minValue = this._impl.MinValue;
            var maxValue = this._impl.MaxValue;

            if (start == end) 
            {
                if (start < 0) 
                {
                    //
                    // We're setting the entire range of values.
                    //

                    if (interval <= 1) 
                    {
                        this._minValueSet = minValue;
                        this._maxValueSet = maxValue;
                        this._bits.SetAll(true);
                        return;
                    }

                    start = minValue;
                    end = maxValue;
                } 
                else 
                {
                    //
                    // We're only setting a single value - check that it is in range.
                    //

                    if (start < minValue) 
                    {
                        throw new CrontabException(string.Format(
                            CultureInfo.CurrentCulture, "'{0} is lower than the minimum allowable value for this field. Value must be between {1} and {2} (all inclusive).", 
                            start, this._impl.MinValue, this._impl.MaxValue));
                    } 
                    
                    if (start > maxValue) 
                    {
                        throw new CrontabException(string.Format(
                            CultureInfo.CurrentCulture, "'{0} is higher than the maximum allowable value for this field. Value must be between {1} and {2} (all inclusive).", 
                            end, this._impl.MinValue, this._impl.MaxValue));
                    }
                }
            } 
            else 
            {
                //
                // For ranges, if the start is bigger than the end value then
                // swap them over.
                //

                if (start > end) 
                {
                    end ^= start;
                    start ^= end;
                    end ^= start;
                }

                if (start < 0) 
                {
                    start = minValue;
                } 
                else if (start < minValue) 
                {
                    throw new CrontabException(string.Format(
                        CultureInfo.CurrentCulture, "'{0} is lower than the minimum allowable value for this field. Value must be between {1} and {2} (all inclusive).", 
                        start, this._impl.MinValue, this._impl.MaxValue));
                }

                if (end < 0) 
                {
                    end = maxValue;
                } 
                else if (end > maxValue) 
                {
                    throw new CrontabException(string.Format(
                        CultureInfo.CurrentCulture, "'{0} is higher than the maximum allowable value for this field. Value must be between {1} and {2} (all inclusive).", 
                        end, this._impl.MinValue, this._impl.MaxValue));
                }
            }

            if (interval < 1) 
                interval = 1;

            int i;

            //
            // Populate the _bits table by setting all the bits corresponding to
            // the valid field values.
            //

            for (i = start - minValue; i <= (end - minValue); i += interval) 
                this._bits[i] = true;

            //
            // Make sure we remember the minimum value set so far Keep track of
            // the highest and lowest values that have been added to this field
            // so far.
            //

            if (this._minValueSet > start) 
                this._minValueSet = start;

            i += (minValue - interval);

            if (this._maxValueSet < i) 
                this._maxValueSet = i;
        }

        /// <summary>
        /// Toes the string.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string ToString()
        {
            return this.ToString(null);
        }

        /// <summary>
        /// Toes the string.	
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ToString(string format)
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);

            switch (format)
            {
                case "G":
                case null:
                    this.Format(writer, true);
                    break;
                case "N":
                    this.Format(writer);
                    break;
                default:
                    throw new FormatException();
            }

            return writer.ToString();
        }

        /// <summary>
        /// Formats the specified writer.	
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <remarks></remarks>
        public void Format(TextWriter writer)
        {
            this.Format(writer, false);
        }

        /// <summary>
        /// Formats the specified writer.	
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="noNames">The no names.</param>
        /// <remarks></remarks>
        public void Format(TextWriter writer, bool noNames)
        {
            this._impl.Format(this, writer, noNames);
        }
    }
}
