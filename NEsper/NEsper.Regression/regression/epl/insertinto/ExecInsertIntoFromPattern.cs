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


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.insertinto
{
    public class ExecInsertIntoFromPattern : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionPropsWildcard(epService);
            RunAssertionProps(epService);
            RunAssertionNoProps(epService);
        }
    
        private void RunAssertionPropsWildcard(EPServiceProvider epService) {
            string stmtText =
                    "insert into MyThirdStream(es0id, es1id) " +
                            "select es0.id, es1.id " +
                            "from " +
                            "pattern [every (es0=" + typeof(SupportBean_S0).FullName +
                            " or es1=" + typeof(SupportBean_S1).FullName + ")]";
            epService.EPAdministrator.CreateEPL(stmtText);
    
            string stmtTwoText =
                    "select * from MyThirdStream";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtTwoText);
    
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            SendEventsAndAssert(epService, updateListener);
    
            statement.Dispose();
        }
    
        private void RunAssertionProps(EPServiceProvider epService) {
            string stmtText =
                    "insert into MySecondStream(s0, s1) " +
                            "select es0, es1 " +
                            "from " +
                            "pattern [every (es0=" + typeof(SupportBean_S0).FullName +
                            " or es1=" + typeof(SupportBean_S1).FullName + ")]";
            epService.EPAdministrator.CreateEPL(stmtText);
    
            string stmtTwoText =
                    "select s0.id as es0id, s1.id as es1id from MySecondStream";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtTwoText);
    
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            SendEventsAndAssert(epService, updateListener);
    
            statement.Dispose();
        }
    
        private void RunAssertionNoProps(EPServiceProvider epService) {
            string stmtText =
                    "insert into MyStream " +
                            "select es0, es1 " +
                            "from " +
                            "pattern [every (es0=" + typeof(SupportBean_S0).FullName +
                            " or es1=" + typeof(SupportBean_S1).FullName + ")]";
            epService.EPAdministrator.CreateEPL(stmtText);
    
            string stmtTwoText =
                    "select es0.id as es0id, es1.id as es1id from MyStream#length(10)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtTwoText);
    
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            SendEventsAndAssert(epService, updateListener);
    
            statement.Dispose();
        }
    
        private void SendEventsAndAssert(EPServiceProvider epService, SupportUpdateListener updateListener) {
            SendEventS1(epService, 10, "");
            EventBean theEvent = updateListener.AssertOneGetNewAndReset();
            Assert.IsNull(theEvent.Get("es0id"));
            Assert.AreEqual(10, theEvent.Get("es1id"));
    
            SendEventS0(epService, 20, "");
            theEvent = updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual(20, theEvent.Get("es0id"));
            Assert.IsNull(theEvent.Get("es1id"));
        }
    
        private void SendEventS0(EPServiceProvider epService, int id, string p00) {
            var theEvent = new SupportBean_S0(id, p00);
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEventS1(EPServiceProvider epService, int id, string p10) {
            var theEvent = new SupportBean_S1(id, p10);
            epService.EPRuntime.SendEvent(theEvent);
        }
    }
} // end of namespace
