///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.metrics
{
    [TestFixture]
	public class TestAnnotationJMX
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
	    public void TestAnnotations()
        {
	        MetricName metricName = MetricNameFactory.Name("default", "test", typeof(TestAnnotationJMX));
	        CommonJMXUtil.RegisterMbean(new MyJMXExposedClass(), metricName);
	        CommonJMXUtil.UnregisterMbean(metricName);
	    }

	    @JmxManaged
	    public class MyJMXExposedClass
        {

	        private string value = "initial value";

	        @JmxOperation(Description = "Perform an operation")
	        public void DoSomething() {
	            Log.Info("Invoking operation");
	        }

	        @JmxGetter(Name = "propertyToGet", Description = "Get some value")
	        public string GetValue() {
	            Log.Info("Getting value " + value);
	            return value;
	        }

	        @JmxSetter(name = "propertyToSet", Description = "Set some value")
	        public void SetValue(@JmxParam(name = "value to set", description = "description for the value to set") string value) {
	            Log.Info("Setting value " + value);
	            this.value = value;
	        }

	        @JmxGetter(name = "secondPropertyToGet", description = "Get a second value that is the same as the first value")
	        public string GetValueTwo() {
	            Log.Info("Getting value two " + value);
	            return value;
	        }
	    }
	}
} // end of namespace
