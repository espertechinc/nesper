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

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLSelectExprSQLCompat : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionProperty(epService);
            RunAssertionPrefixStream(epService);
    
            RunAssertionProperty(epService);
            RunAssertionPrefixStream(epService);
    
            // allow no as-keyword
            epService.EPAdministrator.CreateEPL("select intPrimitive abc from SupportBean");
        }
    
        private void RunAssertionProperty(EPServiceProvider engine) {
            string epl = "select default.SupportBean.theString as val1, SupportBean.intPrimitive as val2 from SupportBean";
            EPStatement stmt = engine.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.AddListener(testListener);
    
            SendEvent(engine, "E1", 10);
            EventBean received = testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("E1", received.Get("val1"));
            Assert.AreEqual(10, received.Get("val2"));
    
            stmt.Dispose();
        }
    
        // Test stream name prefixed by engine URI
        private void RunAssertionPrefixStream(EPServiceProvider engine) {
            string epl = "select theString from default.SupportBean";
            EPStatement stmt = engine.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.AddListener(testListener);
    
            SendEvent(engine, "E1", 10);
            EventBean received = testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("E1", received.Get("theString"));
    
            stmt.Dispose();
        }
    
        private void SendEvent(EPServiceProvider engine, string s, int intPrimitive) {
            var bean = new SupportBean(s, intPrimitive);
            engine.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
