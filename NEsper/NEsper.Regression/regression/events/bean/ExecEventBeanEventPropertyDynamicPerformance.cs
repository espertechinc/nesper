///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

// using static org.junit.Assert.assertEquals;
// using static org.junit.Assert.assertTrue;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.bean
{
    public class ExecEventBeanEventPropertyDynamicPerformance : RegressionExecution {
        public bool ExcludeWhenInstrumented() {
            return true;
        }
    
        public override void Run(EPServiceProvider epService) {
    
            string stmtText = "select simpleProperty?, " +
                    "indexed[1]? as indexed, " +
                    "Mapped('keyOne')? as mapped " +
                    "from " + typeof(SupportBeanComplexProps).FullName;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(Object), type.GetPropertyType("simpleProperty?"));
            Assert.AreEqual(typeof(Object), type.GetPropertyType("indexed"));
            Assert.AreEqual(typeof(Object), type.GetPropertyType("mapped"));
    
            SupportBeanComplexProps inner = SupportBeanComplexProps.MakeDefaultBean();
            epService.EPRuntime.SendEvent(inner);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(inner.SimpleProperty, theEvent.Get("simpleProperty?"));
            Assert.AreEqual(inner.GetIndexed(1), theEvent.Get("indexed"));
            Assert.AreEqual(inner.GetMapped("keyOne"), theEvent.Get("mapped"));
    
            long start = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(inner);
                if (i % 1000 == 0) {
                    listener.Reset();
                }
            }
            long end = DateTimeHelper.CurrentTimeMillis;
            long delta = end - start;
            Assert.IsTrue("delta=" + delta, delta < 1000);
        }
    }
} // end of namespace
