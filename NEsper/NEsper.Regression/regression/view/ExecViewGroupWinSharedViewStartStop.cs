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
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewGroupWinSharedViewStartStop : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
    
            epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(SubscriberName string, ValueInt float)");
            string query = "select SubscriberName, avg(ValueInt) "
                    + "from MyEvent#groupwin(SubscriberName)#length(4)"
                    + "group by SubscriberName output snapshot every 1 events";
            string query2 = "select SubscriberName, avedev(ValueInt) "
                    + "from MyEvent#groupwin(SubscriberName)#length(3) "
                    + "group by SubscriberName output snapshot every 1 events";
    
            string[] groups = {
                    "G_A", "G_A", "G_A", "G_A", "G_B", "G_B", "G_B", "G_B",
                    "G_B", "G_B", "G_B", "G_B", "G_B", "G_B", "G_B", "G_B",
                    "G_B", "G_B", "G_B", "G_B", "G_C", "G_C", "G_C", "G_C",
                    "G_D", "G_A", "G_D", "G_D", "G_A", "G_D", "G_D", "G_D",
                    "G_A", "G_A", "G_A", "G_A", "G_C", "G_C", "G_C", "G_C",
                    "G_D", "G_A", "G_D", "G_D", "G_D", "G_A", "G_D", "G_D",
                    "G_D", "G_E"};
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(query, "myquery");
            EPStatement statement2 = epService.EPAdministrator.CreateEPL(query2, "myquery2");
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            statement2.Events += listener.Update;
    
            int i = 0;
            foreach (string csv in groups) {
                object[] @event = {csv, 0f};
                epService.EPRuntime.SendEvent(@event, "MyEvent");
                i++;
    
                EPStatement stmt = epService.EPAdministrator.GetStatement("myquery");
                if (i % 6 == 0) {
                    stmt.Stop();
                } else if (i % 6 == 4) {
                    stmt.Start();
                }
            }
        }
    }
} // end of namespace
