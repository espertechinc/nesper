///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.soda;

namespace com.espertech.esper.runtime.client.linq
{
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Gets the time period expression.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        public static TimePeriodExpression ToTimePeriodExpression(this TimeSpan timeSpan)
        {
            var timePeriodExpression = new TimePeriodExpression(
                ExpressionWhenNonZero(timeSpan.Days),
                ExpressionWhenNonZero(timeSpan.Hours),
                ExpressionWhenNonZero(timeSpan.Minutes),
                ExpressionWhenNonZero(timeSpan.Seconds),
                ExpressionWhenNonZero(timeSpan.Milliseconds)
                );
            return timePeriodExpression;
        }

        /// <summary>
        /// Returns a constant expression when the underlying value is not zero.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static ConstantExpression ExpressionWhenNonZero(int value)
        {
            return value != 0 ? new ConstantExpression(value) : null;
        }
    }
}