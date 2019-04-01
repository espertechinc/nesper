///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class ExecMTUpdateIStreamSubselect : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            EPStatement stmt = epService.EPAdministrator.CreateEPL("update istream SupportBean as sb " +
                    "set LongPrimitive = (select count(*) from SupportBean_S0#keepall as s0 where s0.p00 = sb.TheString)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // insert 5 data events for each symbol
            int numGroups = 20;
            int numRepeats = 5;
            for (int i = 0; i < numGroups; i++) {
                for (int j = 0; j < numRepeats; j++) {
                    epService.EPRuntime.SendEvent(new SupportBean_S0(i, "S0_" + i)); // S0_0 .. S0_19 each has 5 events
                }
            }
    
            var threads = new List<Thread>();
            for (int i = 0; i < numGroups; i++) {
                int group = i;
                var t = new Thread(() => epService.EPRuntime.SendEvent(new SupportBean("S0_" + group, 1)));
                threads.Add(t);
                t.Start();
            }

            threads.ForEach(t => t.Join());
    
            // validate results, price must be 5 for each symbol
            Assert.AreEqual(numGroups, listener.NewDataList.Count);
            foreach (EventBean[] newData in listener.NewDataList) {
                SupportBean result = (SupportBean) (newData[0]).Underlying;
                Assert.AreEqual(numRepeats, result.LongPrimitive);
            }
        }
    }
    
} // end of namespace
