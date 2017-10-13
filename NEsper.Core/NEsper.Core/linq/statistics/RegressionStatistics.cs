///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.linq.statistics
{
    public class RegressionStatistics
    {
        public double Slope { get; set; }
        public double YIntercept { get; set; }
        public double XAverage { get; set; }
        public double XStandardDeviationPop { get; set; }
        public double XStandardDeviationSample { get; set; }
        public double XSum { get; set; }
        public double XVariance { get; set; }
        public double YAverage { get; set; }
        public double YStandardDeviationPop { get; set; }
        public double YStandardDeviationSample { get; set; }
        public double YSum { get; set; }
        public double YVariance { get; set; }
        public int DataPoints { get; set; }
        public int N { get; set; }
        public double SumXsumXSq { get; set; }
        public double SumXY { get; set; }
        public double SumY { get; set; }
        public double SumYSq { get; set; }
    }
}
