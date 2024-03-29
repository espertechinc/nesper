///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics
{
    public class MetricNameFactory
    {
        public const string METRIC_GROUP_NAME = "com.espertech.esper";

        public static MetricName Name(string runtimeURI, string type, Type clazz)
        {
            return new MetricName(METRIC_GROUP_NAME + "-" + runtimeURI, type, clazz.Name);
        }

        public static MetricName Name(string runtimeURI, string type)
        {
            return new MetricName(METRIC_GROUP_NAME + "-" + runtimeURI, type, "");
        }
    }
} // end of namespace