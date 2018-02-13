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

// using static org.junit.Assert.assertFalse;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogEnumMethod : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
            string epl = "select * from SupportBean match_recognize ("
                    + "partition by theString "
                    + "measures A.theString as c0, C.intPrimitive as c1 "
                    + "pattern (A B+ C) "
                    + "define "
                    + "B as B.intPrimitive > A.intPrimitive, "
                    + "C as C.doublePrimitive > B.FirstOf().intPrimitive)";
            // can also be expressed as: B[0].intPrimitive
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).AddListener(listener);
    
            SendEvent(epService, "E1", 10, 0);
            SendEvent(epService, "E1", 11, 50);
            SendEvent(epService, "E1", 12, 11);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E2", 10, 0);
            SendEvent(epService, "E2", 11, 50);
            SendEvent(epService, "E2", 12, 12);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 12});
        }
    
        private void SendEvent(EPServiceProvider epService, string theString, int intPrimitive, double doublePrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.DoublePrimitive = doublePrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
