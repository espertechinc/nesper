///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
	public class ClientRuntimeJMX {
	    private static readonly string JMX_ENGINE_NAME = typeof(ClientRuntimeJMX).Name + "__JMX";
	    private static readonly string JMX_FILTER_NAME = "\"com.espertech.esper-" + JMX_ENGINE_NAME + "\":type=\"filter\"";
	    private static readonly string JMX_RUNTIME_NAME = "\"com.espertech.esper-" + JMX_ENGINE_NAME + "\":type=\"runtime\"";
	    private static readonly string JMX_SCHEDULE_NAME = "\"com.espertech.esper-" + JMX_ENGINE_NAME + "\":type=\"schedule\"";
	    private static readonly string[] ALL = new string[]{JMX_FILTER_NAME, JMX_RUNTIME_NAME, JMX_SCHEDULE_NAME};

	    public void Run(Configuration configuration) {
	        AssertNoEngineJMX();

	        configuration.Common.AddEventType(typeof(SupportBean));
	        configuration.Runtime.MetricsReporting.JmxRuntimeMetrics = true;
	        EPRuntime runtime = EPRuntimeProvider.GetRuntime(JMX_ENGINE_NAME, configuration);

	        AssertEngineJMX();

	        runtime.Destroy();

	        AssertNoEngineJMX();
	    }

	    private void AssertEngineJMX() {
	        foreach (string name in ALL) {
	            AssertJMXVisible(name);
	        }
	    }

	    private void AssertJMXVisible(string name) {
	        try {
	            ManagementFactory.PlatformMBeanServer.GetObjectInstance(new ObjectName(name));
	        } catch (Exception t) {
	            throw new EPException(t);
	        }
	    }

	    private void AssertNoEngineJMX() {
	        foreach (string name in ALL) {
	            AssertJMXNotVisible(name);
	        }
	    }

	    private void AssertJMXNotVisible(string name) {
	        try {
	            ManagementFactory.PlatformMBeanServer.GetObjectInstance(new ObjectName(name));
	            Assert.Fail();
	        } catch (InstanceNotFoundException ex) {
	            // expected
	        } catch (Exception t) {
	            throw new EPException(t);
	        }
	    }
	}
} // end of namespace