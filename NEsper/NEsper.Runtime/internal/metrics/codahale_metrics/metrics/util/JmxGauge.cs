///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.util
{
    /// <summary>
    ///     A gauge which exposes an attribute of a JMX MBean.
    /// </summary>
    public class JmxGauge : Gauge<object>
    {
        //private static readonly MBeanServer SERVER = ManagementFactory.PlatformMBeanServer;
        private readonly string attribute;
        private readonly string objectName;

        /// <summary>
        ///     Creates a new <seealso cref="JmxGauge" /> for the given attribute of the given MBean.
        /// </summary>
        /// <param name="objectName">the string value of the MBean's</param>
        /// <param name="attribute">the MBean attribute's name</param>
        /// <throws>javax.management.MalformedObjectNameException if {@code objectName} is malformed</throws>
        public JmxGauge(
            string objectName,
            string attribute)
        {
            this.objectName = objectName;
            this.attribute = attribute;
        }

        public override object Value {
            get {
                try {
                    return SERVER.GetAttribute(objectName, attribute);
                }
                catch (Exception e) {
                    throw new EPRuntimeException(e);
                }
            }
        }
    }
} // end of namespace