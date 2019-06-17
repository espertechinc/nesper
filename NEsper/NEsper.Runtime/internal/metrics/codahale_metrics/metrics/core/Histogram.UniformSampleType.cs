///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.stats;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    public partial class Histogram
    {
        /// <summary>
        /// Uses a uniform sample of 1028 elements, which offers a 99.9% confidence level with a 5%
        /// margin of error assuming a normal distribution.
        /// </summary>
        internal class UniformSampleType : SampleType
        {
            public static UniformSampleType INSTANCE = new UniformSampleType();

            public Sample NewSample()
            {
                return new UniformSample(DEFAULT_SAMPLE_SIZE);
            }
        }
    }
}