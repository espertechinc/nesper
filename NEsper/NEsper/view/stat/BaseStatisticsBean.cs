///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.client;

namespace com.espertech.esper.view.stat
{
    /// <summary>
    /// Bean for performing statistical calculations. The bean keeps sums of X and Y datapoints and sums on squares
    /// that can be reused by subclasses. The bean calculates standard deviation (sample and population), variance,
    /// average and sum.
    /// </summary>
    [Serializable]
    public class BaseStatisticsBean : ICloneable
    {
        /// <summary> Calculates standard deviation for X based on the entire population given as arguments.
        /// Equivalent to Microsoft Excel formula STDEVPA.
        /// </summary>
        /// <returns> standard deviation assuming population for X
        /// </returns>
        public double XStandardDeviationPop
        {
            get
            {
                if (DataPoints == 0)
                {
                    return Double.NaN;
                }

                double temp = (SumXSq - XSum * XSum / DataPoints) / DataPoints;
                return Math.Sqrt(temp);
            }
        }

        /// <summary> Calculates standard deviation for Y based on the entire population given as arguments.
        /// Equivalent to Microsoft Excel formula STDEVPA.
        /// </summary>
        /// <returns> standard deviation assuming population for Y
        /// </returns>
        public double YStandardDeviationPop
        {
            get
            {
                if (DataPoints == 0)
                {
                    return Double.NaN;
                }

                double temp = (SumYSq - YSum * YSum / DataPoints) / DataPoints;
                return Math.Sqrt(temp);
            }
        }

        /// <summary> Calculates standard deviation for X based on the sample data points supplied.
        /// Equivalent to Microsoft Excel formula STDEV.
        /// </summary>
        /// <returns> standard deviation assuming sample for X
        /// </returns>
        public double XStandardDeviationSample
        {
            get
            {
                if (DataPoints < 2)
                {
                    return Double.NaN;
                }

                double variance = XVariance;
                return Math.Sqrt(variance);
            }
        }

        /// <summary> Calculates standard deviation for Y based on the sample data points supplied.
        /// Equivalent to Microsoft Excel formula STDEV.
        /// </summary>
        /// <returns> standard deviation assuming sample for Y
        /// </returns>
        public double YStandardDeviationSample
        {
            get
            {
                if (DataPoints < 2)
                {
                    return Double.NaN;
                }

                double variance = YVariance;
                return Math.Sqrt(variance);
            }
        }

        /// <summary> Calculates standard deviation for X based on the sample data points supplied.
        /// Equivalent to Microsoft Excel formula STDEV.
        /// </summary>
        /// <returns> variance as the square of the sample standard deviation for X
        /// </returns>
        public double XVariance
        {
            get
            {
                if (DataPoints < 2)
                {
                    return Double.NaN;
                }

                return (SumXSq - XSum * XSum / DataPoints) / (DataPoints - 1);
            }
        }

        /// <summary> Calculates standard deviation for Y based on the sample data points supplied.
        /// Equivalent to Microsoft Excel formula STDEV.
        /// </summary>
        /// <returns> variance as the square of the sample standard deviation for Y
        /// </returns>
        public double YVariance
        {
            get
            {
                if (DataPoints < 2)
                {
                    return Double.NaN;
                }

                return (SumYSq - YSum * YSum / DataPoints) / (DataPoints - 1);
            }

        }
        /// <summary> Returns the number of data points.</summary>
        /// <returns> number of data points
        /// </returns>
        public long N
        {
            get { return DataPoints; }
        }

        /// <summary>
        /// Gets or sets the number of data points.
        /// </summary>
        /// <value>The number of data points.</value>
        public long DataPoints { get; set; }

        /// <summary> Returns the sum of all X data points.</summary>
        /// <returns> sum of X data points
        /// </returns>
        public double XSum { get; set; }

        /// <summary> Returns the sum of all Y data points.</summary>
        /// <returns> sum of Y data points
        /// </returns>
        public double YSum { get; set; }

        /// <summary> Returns the average of all X data points.</summary>
        /// <returns> average of X data points
        /// </returns>
        public double XAverage
        {
            get
            {
                if (DataPoints == 0)
                {
                    return Double.NaN;
                }

                return XSum / DataPoints;
            }
        }

        /// <summary> Returns the average of all Y data points.</summary>
        /// <returns> average of Y data points
        /// </returns>
        public double YAverage
        {
            get
            {
                if (DataPoints == 0)
                {
                    return Double.NaN;
                }

                return YSum / DataPoints;
            }
        }

        /// <summary> Returns the sum of all X data points.</summary>
        /// <returns> sum of X data points
        /// </returns>
        public double SumX
        {
            get { return XSum; }
        }

        /// <summary> Returns the sum of all Y data points.</summary>
        /// <returns> sum of Y data points
        /// </returns>
        public double SumY
        {
            get { return YSum; }
        }

        /// <summary> For use by subclasses, returns sum (X * X).</summary>
        /// <returns> sum of X squared
        /// </returns>
        public double SumXSq { get; set; }

        /// <summary> For use by subclasses, returns sum (Y * Y).</summary>
        /// <returns> sum of Y squared
        /// </returns>
        public double SumYSq { get; set; }

        /// <summary> For use by subclasses, returns sum (X * Y).</summary>
        /// <returns> sum of X times Y
        /// </returns>
        public double SumXY { get; set; }


        /// <summary>Returns the Y intercept. </summary>
        /// <returns>Y intercept</returns>

        public double YIntercept
        {
            get
            {
                double slope = Slope;

                if (Double.IsNaN(slope)) {
                    return Double.NaN;
                }

                return YSum/N - Slope*XSum/N;
            }
        }

        /// <summary>Returns the slope. </summary>
        /// <returns>regression slope</returns>

        public double Slope
        {
            get
            {
                if (N == 0) {
                    return Double.NaN;
                }

                double ssx = SumXSq - XSum*XSum/N;

                if (ssx == 0) {
                    return Double.NaN;
                }

                double sp = SumXY - XSum*YSum/N;

                return sp/ssx;
            }
        }

        /// <summary>Return the correlation value for the two data series (Microsoft Excel function CORREL). </summary>
        /// <returns>correlation value</returns>

        public double Correlation
        {
            get
            {
                if (N == 0) {
                    return Double.NaN;
                }

                double dx = SumXSq - (XSum*XSum)/N;
                double dy = SumYSq - (YSum*YSum)/N;

                if (dx == 0 || dy == 0) {
                    return Double.NaN;
                }

                double sp = SumXY - XSum*YSum/N;
                return sp/Math.Sqrt(dx*dy);
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            XSum = 0;
            SumXSq = 0;
            YSum = 0;
            SumYSq = 0;
            SumXY = 0;
            DataPoints = 0;
        }

        /// <summary> Add a data point for the X data set only.</summary>
        /// <param name="x">is the X data point to add.
        /// </param>
        public void AddPoint(double x)
        {
            DataPoints++;
            XSum += x;
            SumXSq += x * x;
        }

        /// <summary> Add a data point.</summary>
        /// <param name="x">is the X data point to add.
        /// </param>
        /// <param name="y">is the Y data point to add.
        /// </param>
        public void AddPoint(double x, double y)
        {
            DataPoints++;
            XSum += x;
            SumXSq += x * x;
            YSum += y;
            SumYSq += y * y;
            SumXY += x * y;
        }

        /// <summary> Remove a X data point only.</summary>
        /// <param name="x">is the X data point to remove.
        /// </param>
        public void RemovePoint(double x)
        {
            DataPoints--;
            if (DataPoints <= 0)
            {
                Initialize();
            }
            else
            {
                XSum -= x;
                SumXSq -= x * x;
            }
        }

        /// <summary> Remove a data point.</summary>
        /// <param name="x">is the X data point to remove.
        /// </param>
        /// <param name="y">is the Y data point to remove.
        /// </param>
        public void RemovePoint(double x, double y)
        {
            DataPoints--;
            if (DataPoints <= 0)
            {
                Initialize();
            }
            else
            {
                XSum -= x;
                SumXSq -= x * x;
                YSum -= y;
                SumYSq -= y * y;
                SumXY -= x * y;
            }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public virtual Object Clone()
        {
            try
            {
                return MemberwiseClone();
            }
            catch (Exception e)
            {
                throw new EPException(e);
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            return
                "datapoints=" + DataPoints +
                "  sumX=" + XSum +
                "  sumXSq=" + SumXSq +
                "  sumY=" + YSum +
                "  sumYSq=" + SumYSq +
                "  sumXY=" + SumXY;
        }
    }
}
