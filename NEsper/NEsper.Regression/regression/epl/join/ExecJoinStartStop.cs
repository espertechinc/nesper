///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoinStartStop : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionJoinUniquePerId(epService);
            RunAssertionInvalidJoin(epService);
        }
    
        private void RunAssertionJoinUniquePerId(EPServiceProvider epService) {
            string joinStatement = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "(symbol='IBM')#length(3) s0, " +
                    typeof(SupportMarketDataBean).FullName + "(symbol='CSCO')#length(3) s1" +
                    " where s0.volume=s1.volume";
    
            var setOne = new Object[5];
            var setTwo = new Object[5];
            var volumesOne = new long[]{10, 20, 20, 40, 50};
            var volumesTwo = new long[]{10, 20, 30, 40, 50};
            for (int i = 0; i < setOne.Length; i++) {
                setOne[i] = new SupportMarketDataBean("IBM", volumesOne[i], (long) i, "");
                setTwo[i] = new SupportMarketDataBean("CSCO", volumesTwo[i], (long) i, "");
            }
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(joinStatement, "MyJoin");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, setOne[0]);
            SendEvent(epService, setTwo[0]);
            Assert.IsNotNull(listener.LastNewData);
            listener.Reset();
    
            stmt.Stop();
            SendEvent(epService, setOne[1]);
            SendEvent(epService, setTwo[1]);
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Start();
            SendEvent(epService, setOne[2]);
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Stop();
            SendEvent(epService, setOne[3]);
            SendEvent(epService, setOne[4]);
            SendEvent(epService, setTwo[3]);
    
            stmt.Start();
            SendEvent(epService, setTwo[4]);
            Assert.IsFalse(listener.IsInvoked);
    
            // assert type-statement reference
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse(typeof(SupportMarketDataBean).FullName));
            var stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportMarketDataBean).FullName);
            Assert.IsTrue(stmtNames.Contains("MyJoin"));
    
            stmt.Dispose();
    
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse(typeof(SupportMarketDataBean).FullName));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportMarketDataBean).FullName);
            EPAssertionUtil.AssertEqualsAnyOrder(null, stmtNames.ToArray());
            Assert.IsFalse(stmtNames.Contains("MyJoin"));
        }
    
        private void RunAssertionInvalidJoin(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportBean_B));
    
            string invalidJoin = "select * from A, B";
            TryInvalid(epService, invalidJoin,
                    "Error starting statement: Joins require that at least one view is specified for each stream, no view was specified for A [select * from A, B]");
    
            invalidJoin = "select * from A#time(5 min), B";
            TryInvalid(epService, invalidJoin,
                    "Error starting statement: Joins require that at least one view is specified for each stream, no view was specified for B [select * from A#time(5 min), B]");
    
            invalidJoin = "select * from A#time(5 min), pattern[A->B]";
            TryInvalid(epService, invalidJoin,
                    "Error starting statement: Joins require that at least one view is specified for each stream, no view was specified for pattern event stream [select * from A#time(5 min), pattern[A->B]]");
        }
    
        private void SendEvent(EPServiceProvider epService, Object theEvent) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
