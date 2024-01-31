///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        /// Uses an exponentially decaying sample of 1028 elements, which offers a 99.9% confidence
        /// level with a 5% margin of error assuming a normal distribution, and an alpha factor of
        /// 0.015, which heavily biases the sample to the past 5 minutes of measurements.
        /// </summary>
        internal class BiasedSampleType : SampleType
        {
            public static BiasedSampleType INSTANCE = new BiasedSampleType();

            public Sample NewSample()
            {
                return new ExponentiallyDecayingSample(DEFAULT_SAMPLE_SIZE, DEFAULT_ALPHA);
            }
        }
    }
}