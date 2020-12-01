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
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace WellEngineered.CruiseControl.Core.Triggers.NCrontab
{
	#region Imports

	using Debug = System.Diagnostics.Debug;

    #endregion

    /// <summary>
    /// Represents a schedule initialized from the crontab expression.
    /// </summary>
    [ Serializable ]
    public sealed class CrontabSchedule
    {
        private readonly CrontabField _minutes;
        private readonly CrontabField _hours;
        private readonly CrontabField _days;
        private readonly CrontabField _months;
        private readonly CrontabField _daysOfWeek;

        private static readonly char[] _separators = new[] {' '};

        //
        // Crontab expression format:
        //
        // * * * * *
        // - - - - -
        // | | | | |
        // | | | | +----- day of week (0 - 6) (Sunday=0)
        // | | | +------- month (1 - 12)
        // | | +--------- day of month (1 - 31)
        // | +----------- hour (0 - 23)
        // +------------- min (0 - 59)
        //
        // Star (*) in the value field above means all legal values as in 
        // braces for that column. The value column can have a * or a list 
        // of elements separated by commas. An element is either a number in 
        // the ranges shown above or two numbers in the range separated by a 
        // hyphen (meaning an inclusive range). 
        //
        // Source: http://www.adminschoice.com/docs/crontab.htm
        //

        /// <summary>
        /// Parses the specified expression.	
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static CrontabSchedule Parse(string expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            return new CrontabSchedule(expression);
        }

        private CrontabSchedule(string expression)
        {
            Debug.Assert(expression != null);

            var fields = expression.Split(_separators, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length != 5)
            {
                throw new CrontabException(string.Format(
                    CultureInfo.CurrentCulture, "'{0}' is not a valid crontab expression. It must contain at least 5 components of a schedule "
                    + "(in the sequence of minutes, hours, days, months, days of week).", 
                    expression));
            }

            this._minutes = CrontabField.Minutes(fields[0]);
            this._hours = CrontabField.Hours(fields[1]);
            this._days = CrontabField.Days(fields[2]);
            this._months = CrontabField.Months(fields[3]);
            this._daysOfWeek = CrontabField.DaysOfWeek(fields[4]);
        }

        /// <summary>
        /// Gets the next occurrences.	
        /// </summary>
        /// <param name="baseTime">The base time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<DateTime> GetNextOccurrences(DateTime baseTime, DateTime endTime)
        {
            for (var occurrence = this.GetNextOccurrence(baseTime, endTime);
                 occurrence < endTime; 
                 occurrence = this.GetNextOccurrence(occurrence, endTime))
            {
                yield return occurrence;
            }
        }

        /// <summary>
        /// Gets the next occurrence.	
        /// </summary>
        /// <param name="baseTime">The base time.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public DateTime GetNextOccurrence(DateTime baseTime)
        {
            return this.GetNextOccurrence(baseTime, DateTime.MaxValue);
        }

        /// <summary>
        /// Gets the next occurrence.	
        /// </summary>
        /// <param name="baseTime">The base time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public DateTime GetNextOccurrence(DateTime baseTime, DateTime endTime) 
        {
            const int nil = -1;

            var baseYear = baseTime.Year;
            var baseMonth = baseTime.Month;
            var baseDay = baseTime.Day;
            var baseHour = baseTime.Hour;
            var baseMinute = baseTime.Minute;

            var endYear = endTime.Year;
            var endMonth = endTime.Month;
            var endDay = endTime.Day;

            var year = baseYear;
            var month = baseMonth;
            var day = baseDay;
            var hour = baseHour;
            var minute = baseMinute + 1;

            //
            // Minute
            //

            minute = this._minutes.Next(minute);

            if (minute == nil) 
            {
                minute = this._minutes.GetFirst();
                hour++;
            }

            //
            // Hour
            //

            hour = this._hours.Next(hour);
            
            if (hour == nil) 
            {
                minute = this._minutes.GetFirst();
                hour = this._hours.GetFirst();
                day++;
            }
            else if (hour > baseHour)
            {
                minute = this._minutes.GetFirst();
            }

            //
            // Day
            //

            day = this._days.Next(day);

            RetryDayMonth:
        
            if (day == nil) 
            {
                minute = this._minutes.GetFirst();
                hour = this._hours.GetFirst();
                day = this._days.GetFirst();
                month++;
            }
            else if (day > baseDay)
            {
                minute = this._minutes.GetFirst();
                hour = this._hours.GetFirst();
            }

            //
            // Month
            //

            month = this._months.Next(month);

            if (month == nil) 
            {
                minute = this._minutes.GetFirst();
                hour = this._hours.GetFirst();
                day = this._days.GetFirst();
                month = this._months.GetFirst();
                year++;
            }
            else if (month > baseMonth)
            {
                minute = this._minutes.GetFirst();
                hour = this._hours.GetFirst();
                day = this._days.GetFirst();
            }

            //
            // The day field in a cron expression spans the entire range of days
            // in a month, which is from 1 to 31. However, the number of days in
            // a month tend to be variable depending on the month (and the year
            // in case of February). So a check is needed here to see if the
            // date is a border case. If the day happens to be beyond 28
            // (meaning that we're dealing with the suspicious range of 29-31)
            // and the date part has changed then we need to determine whether
            // the day still makes sense for the given year and month. If the
            // day is beyond the last possible value, then the day/month part
            // for the schedule is re-evaluated. So an expression like "0 0
            // 15,31 * *" will yield the following sequence starting on midnight
            // of Jan 1, 2000:
            //
            //  Jan 15, Jan 31, Feb 15, Mar 15, Apr 15, Apr 31, ...
            //

            var dateChanged = day != baseDay || month != baseMonth || year != baseYear;

            if (day > 28 && dateChanged && day > Calendar.GetDaysInMonth(year, month))
            {
                if (year >= endYear && month >= endMonth && day >= endDay)
                    return endTime;

                day = nil;
                goto RetryDayMonth;
            }

            var nextTime = new DateTime(year, month, day, hour, minute, 0, 0, baseTime.Kind);

            if (nextTime >= endTime)
                return endTime;

            //
            // Day of week
            //

            if (this._daysOfWeek.Contains((int) nextTime.DayOfWeek)) 
                return nextTime;

            return this.GetNextOccurrence(new DateTime(year, month, day, 23, 59, 0, 0, baseTime.Kind), endTime);
        }

        /// <summary>
        /// Toes the string.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string ToString()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);

            this._minutes.Format(writer, true);
            writer.Write(' ');
            this._hours.Format(writer, true);
            writer.Write(' ');
            this._days.Format(writer, true);
            writer.Write(' ');
            this._months.Format(writer, true);
            writer.Write(' ');
            this._daysOfWeek.Format(writer, true);

            return writer.ToString();
        }

        private static Calendar Calendar
        {
            get { return CultureInfo.InvariantCulture.Calendar; }
        }
    }
}
