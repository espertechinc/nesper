///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.orderby
{
    public class ExecOrderByRowForAll : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            var fields = new string[]{"sumPrice"};
            string statementString = "select sum(price) as sumPrice from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "order by price";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            SendJoinEvents(epService);
            SendEvent(epService, "CAT", 50);
            SendEvent(epService, "IBM", 49);
            SendEvent(epService, "CAT", 15);
            SendEvent(epService, "IBM", 100);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][]{new object[] {214d}});
    
            SendEvent(epService, "KGB", 75);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][]{new object[] {289d}});
    
            // JIRA ESPER-644 Infinite loop when restarting a statement
            epService.EPAdministrator.Configuration.AddEventType("FB", Collections.SingletonDataMap("timeTaken", typeof(double)));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select avg(timeTaken) as timeTaken from FB order by timeTaken desc");
            stmt.Stop();
            stmt.Start();
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendJoinEvents(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
        }
    }
} // end of namespace
