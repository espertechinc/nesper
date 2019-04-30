///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    ///     Bean for performing statistical calculations. The bean keeps sums of X and Y datapoints and sums on squares
    ///     that can be reused by subclasses. The bean calculates standard deviation (sample and population), variance,
    ///     average and sum.
    /// </summary>
    [Serializable]
    public class BaseStatisticsBean
    {
        /// <summary>
        ///     Calculates standard deviation for X based on the entire population given as arguments.
        ///     Equivalent to Microsoft Excel formula STDEVPA.
        /// </summary>
        /// <value>standard deviation assuming population for X</value>
        public double XStandardDeviationPop {
            get {
                if (N == 0) {
                    return double.NaN;
                }

                var temp = (SumXSq - XSum * XSum / N) / N;
                return Math.Sqrt(temp);
            }
        }

        /// <summary>
        ///     Calculates standard deviation for Y based on the entire population given as arguments.
        ///     Equivalent to Microsoft Excel formula STDEVPA.
        /// </summary>
        /// <value>standard deviation assuming population for Y</value>
        public double YStandardDeviationPop {
            get {
                if (N == 0) {
                    return double.NaN;
                }

                var temp = (SumYSq - YSum * YSum / N) / N;
                return Math.Sqrt(temp);
            }
        }

        /// <summary>
        ///     Calculates standard deviation for X based on the sample data points supplied.
        ///     Equivalent to Microsoft Excel formula STDEV.
        /// </summary>
        /// <value>standard deviation assuming sample for X</value>
        public double XStandardDeviationSample {
            get {
                if (N < 2) {
                    return double.NaN;
                }

                var variance = XVariance;
                return Math.Sqrt(variance);
            }
        }

        /// <summary>
        ///     Calculates standard deviation for Y based on the sample data points supplied.
        ///     Equivalent to Microsoft Excel formula STDEV.
        /// </summary>
        /// <value>standard deviation assuming sample for Y</value>
        public double YStandardDeviationSample {
            get {
                if (N < 2) {
                    return double.NaN;
                }

                var variance = YVariance;
                return Math.Sqrt(variance);
            }
        }

        /// <summary>
        ///     Calculates standard deviation for X based on the sample data points supplied.
        ///     Equivalent to Microsoft Excel formula STDEV.
        /// </summary>
        /// <value>variance as the square of the sample standard deviation for X</value>
        public double XVariance {
            get {
                if (N < 2) {
                    return double.NaN;
                }

                return (SumXSq - XSum * XSum / N) / (N - 1);
            }
        }

        /// <summary>
        ///     Calculates standard deviation for Y based on the sample data points supplied.
        ///     Equivalent to Microsoft Excel formula STDEV.
        /// </summary>
        /// <value>variance as the square of the sample standard deviation for Y</value>
        public double YVariance {
            get {
                if (N < 2) {
                    return double.NaN;
                }

                return (SumYSq - YSum * YSum / N) / (N - 1);
            }
        }

        /// <summary>
        ///     Returns the number of data points.
        /// </summary>
        /// <returns>number of data points</returns>
        public long N { get; private set; }

        /// <summary>
        ///     Returns the sum of all X data points.
        /// </summary>
        /// <returns>sum of X data points</returns>
        public double XSum { get; private set; }

        /// <summary>
        ///     Returns the sum of all Y data points.
        /// </summary>
        /// <returns>sum of Y data points</returns>
        public double YSum { get; private set; }

        /// <summary>
        ///     Returns the average of all X data points.
        /// </summary>
        /// <value>average of X data points</value>
        public double XAverage {
            get {
                if (N == 0) {
                    return double.NaN;
                }

                return XSum / N;
            }
        }

        /// <summary>
        ///     Returns the average of all Y data points.
        /// </summary>
        /// <value>average of Y data points</value>
        public double YAverage {
            get {
                if (N == 0) {
                    return double.NaN;
                }

                return YSum / N;
            }
        }

        /// <summary>
        ///     For use by subclasses, returns sum (X * X).
        /// </summary>
        /// <returns>sum of X squared</returns>
        public double SumXSq { get; set; }

        /// <summary>
        ///     For use by subclasses, returns sum (Y * Y).
        /// </summary>
        /// <returns>sum of Y squared</returns>
        public double SumYSq { get; set; }

        /// <summary>
        ///     For use by subclasses, returns sum (X * Y).
        /// </summary>
        /// <returns>sum of X times Y</returns>
        public double SumXY { get; set; }

        /// <summary>
        ///     Returns sum of x.
        /// </summary>
        /// <returns>sum of x</returns>
        public double SumX {
            get => XSum;
            set => XSum = value;
        }

        /// <summary>
        ///     Returns sum of y.
        /// </summary>
        /// <returns>sum of y</returns>
        public double SumY {
            get => YSum;
            set => YSum = value;
        }

        /// <summary>
        ///     Returns the number of datapoints.
        /// </summary>
        /// <returns>datapoints</returns>
        public long DataPoints {
            get => N;
            set => N = value;
        }

        /// <summary>
        ///     Returns the Y intercept.
        /// </summary>
        /// <value>Y intercept</value>
        public double YIntercept {
            get {
                var slope = Slope;

                if (double.IsNaN(slope)) {
                    return double.NaN;
                }

                return YSum / N - Slope * XSum / N;
            }
        }

        /// <summary>
        ///     Returns the slope.
        /// </summary>
        /// <value>regression slope</value>
        public double Slope {
            get {
                if (N == 0) {
                    return double.NaN;
                }

                var ssx = SumXSq - XSum * XSum / N;

                if (ssx == 0) {
                    return double.NaN;
                }

                var sp = SumXY - XSum * YSum / N;

                return sp / ssx;
            }
        }

        /// <summary>
        ///     Return the correlation value for the two data series (Microsoft Excel function CORREL).
        /// </summary>
        /// <value>correlation value</value>
        public double Correlation {
            get {
                if (N == 0) {
                    return double.NaN;
                }

                var dx = SumXSq - XSum * XSum / N;
                var dy = SumYSq - YSum * YSum / N;

                if (dx == 0 || dy == 0) {
                    return double.NaN;
                }

                var sp = SumXY - XSum * YSum / N;
                return sp / Math.Sqrt(dx * dy);
            }
        }

        private void Initialize()
        {
            XSum = 0;
            SumXSq = 0;
            YSum = 0;
            SumYSq = 0;
            SumXY = 0;
            N = 0;
        }

        /// <summary>
        ///     Add a data point for the X data set only.
        /// </summary>
        /// <param name="x">is the X data point to add.</param>
        public void AddPoint(double x)
        {
            N++;
            XSum += x;
            SumXSq += x * x;
        }

        /// <summary>
        ///     Add a data point.
        /// </summary>
        /// <param name="x">is the X data point to add.</param>
        /// <param name="y">is the Y data point to add.</param>
        public void AddPoint(
            double x,
            double y)
        {
            N++;
            XSum += x;
            SumXSq += x * x;
            YSum += y;
            SumYSq += y * y;
            SumXY += x * y;
        }

        /// <summary>
        ///     Remove a X data point only.
        /// </summary>
        /// <param name="x">is the X data point to remove.</param>
        public void RemovePoint(double x)
        {
            N--;
            if (N <= 0) {
                Initialize();
            }
            else {
                XSum -= x;
                SumXSq -= x * x;
            }
        }

        /// <summary>
        ///     Remove a data point.
        /// </summary>
        /// <param name="x">is the X data point to remove.</param>
        /// <param name="y">is the Y data point to remove.</param>
        public void RemovePoint(
            double x,
            double y)
        {
            N--;
            if (N <= 0) {
                Initialize();
            }
            else {
                XSum -= x;
                SumXSq -= x * x;
                YSum -= y;
                SumYSq -= y * y;
                SumXY -= x * y;
            }
        }

        public override string ToString()
        {
            return "datapoints=" + N +
                   "  sumX=" + XSum +
                   "  sumXSq=" + SumXSq +
                   "  sumY=" + YSum +
                   "  sumYSq=" + SumYSq +
                   "  sumXY=" + SumXY;
        }
    }
} // end of namespace