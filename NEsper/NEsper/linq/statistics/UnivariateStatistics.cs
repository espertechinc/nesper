///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.linq.statistics
{
    /// <summary>
    /// This class is not really output by the engine, but we do this to generate a
    /// placeholder so that properties can be bound in expressions.
    /// </summary>
    public class UnivariateStatistics
    {
        public int Datapoints { get; set; }
        public int Total { get; set; }
        public double Average { get; set; }
        public double Variance { get; set; }
        public double Stddev { get; set; }
        public double Stddevpa { get; set; }
    }
}
