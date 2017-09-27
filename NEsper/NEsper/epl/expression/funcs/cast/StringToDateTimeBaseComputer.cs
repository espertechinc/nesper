///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.funcs.cast
{
    public abstract class StringToDateTimeBaseComputer : CasterParserComputer
    {
        public event Action<string, string, Exception> HandleParseException;

        /// <summary>
        /// Gets a value indicating whether this instance is constant for constant input.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is constant for constant input; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsConstantForConstInput { get; }

        /// <summary>
        /// Returns the date format that should be used for a given invocation.
        /// </summary>
        /// <param name="evaluateParams">The evaluate parameters.</param>
        /// <returns></returns>
        protected abstract string GetDateFormat(EvaluateParams evaluateParams);

        /// <summary>
        /// Parses a date time using a standard algorithm given inputs, a format and a timezone.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="dateFormat">The date format.</param>
        /// <param name="timeZoneInfo">The time zone information.</param>
        /// <returns></returns>
        protected DateTimeOffset ParseDateTime(string input, string dateFormat, TimeZoneInfo timeZoneInfo)
        {
            DateTimeOffset dateTime;

            if (DateTimeOffset.TryParseExact(input, dateFormat, null, DateTimeStyles.None, out dateTime) ||
                DateTimeOffset.TryParseExact(input, dateFormat, null, DateTimeStyles.AssumeLocal, out dateTime) ||
                DateTimeOffset.TryParseExact(input, dateFormat, null, DateTimeStyles.AssumeUniversal, out dateTime) ||
                DateTimeOffset.TryParseExact(input, dateFormat, null, DateTimeStyles.AdjustToUniversal, out dateTime))
            {
                return dateTime;
            }

            // this will cause a FormatException
            return DateTimeOffset.ParseExact(input, dateFormat, null, DateTimeStyles.None);
        }

        public abstract object Compute(object input, EvaluateParams evaluateParams);

        /// <summary>
        /// Called when a format exception occurs.
        /// </summary>
        /// <param name="dateTimeFormat">The date time format.</param>
        /// <param name="input">The input.</param>
        /// <param name="formatException">The format exception.</param>
        protected void OnHandleParseException(string dateTimeFormat, string input, FormatException formatException)
        {
            if (HandleParseException != null)
            {
                HandleParseException(dateTimeFormat, input, formatException);
            }
        }
    }

    public abstract class StringToDateTimeBaseComputer<T> : StringToDateTimeBaseComputer
    {
        /// <summary>
        /// Computes the value from the date format and input.
        /// </summary>
        /// <param name="dateFormat">The date format.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        protected abstract T ComputeFromFormat(string dateFormat, string input);

        /// <summary>
        /// Compute an result performing casting and parsing.
        /// </summary>
        /// <param name="input">to process</param>
        /// <param name="evaluateParams">The evaluate parameters.</param>
        /// <returns>
        /// cast or parse result
        /// </returns>
        /// <exception cref="IllegalStateException"></exception>
        public override Object Compute(Object input, EvaluateParams evaluateParams)
        {
            // Get the format that should be used
            var dateTimeFormat = GetDateFormat(evaluateParams);

            // Compute the date from the format
            try
            {
                return ComputeFromFormat(dateTimeFormat, (string) input);
            }
            catch (FormatException ex)
            {
                OnHandleParseException(dateTimeFormat, input.ToString(), ex);
                throw new IllegalStateException();
            }
        }

        /// <summary>
        /// Adds the parse handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns></returns>
        public StringToDateTimeBaseComputer AddParseHandler(Action<string, string, Exception> handler)
        {
            HandleParseException += handler;
            return this;
        }
    }
}
