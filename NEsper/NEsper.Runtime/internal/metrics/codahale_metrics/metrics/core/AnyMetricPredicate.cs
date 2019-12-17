///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    /// A predicate which matches all inputs.
    /// </summary>
    public class AnyMetricPredicate : MetricPredicate
    {
        public static readonly AnyMetricPredicate INSTANCE = new AnyMetricPredicate();

        public bool Matches(
            MetricName name,
            Metric metric)
        {
            return true;
        }
    }
}