///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.util
{
    /// <summary>
    ///     A gauge which measures the ratio of one value to another.
    ///     If the denominator is zero, not a number, or infinite, the resulting ratio is not a number.
    /// </summary>
    public abstract class RatioGauge : Gauge<double>
    {
        /// <summary>
        ///     Returns the numerator (the value on the top half of the fraction or the left-hand side of the
        ///     ratio).
        /// </summary>
        /// <returns>the numerator</returns>
        protected abstract double Numerator { get; }

        /// <summary>
        ///     Returns the denominator (the value on the bottom half of the fraction or the right-hand side
        ///     of the ratio).
        /// </summary>
        /// <returns>the denominator</returns>
        protected abstract double Denominator { get; }

        public override double Value {
            get {
                var d = Denominator;
                if (double.IsNaN(d) || double.IsInfinity(d) || d < double.Epsilon) {
                    return double.NaN;
                }

                return Numerator / d;
            }
        }
    }
} // end of namespace