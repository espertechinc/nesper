///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.util;
using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    public class TwoPatternRunnable : IRunnable
    {
        private readonly EPServiceProvider engine;
        private readonly SupportStmtAwareUpdateListener listener;

        public TwoPatternRunnable(EPServiceProvider engine)
        {
            this.engine = engine;
            listener = new SupportStmtAwareUpdateListener();
        }

        public bool Shutdown { get; set; }

        #region IRunnable Members

        public void Run()
        {
            String stmtText = "every event1=SupportEvent(UserId in ('100','101'),Amount>=1000)";
            EPStatement statement = engine.EPAdministrator.CreatePattern(stmtText);
            statement.Events += listener.Update;

            int countLoops = 0;
            while (!Shutdown) {
                countLoops++;
                IList<SupportTradeEvent> matches = new List<SupportTradeEvent>();

                for (int i = 0; i < 10000; i++) {
                    SupportTradeEvent bean;
                    if (i%1000 == 1) {
                        bean = new SupportTradeEvent(i, "100", 1001);
                        matches.Add(bean);
                    }
                    else {
                        bean = new SupportTradeEvent(i, "101", 10);
                    }
                    engine.EPRuntime.SendEvent(bean);
                }

                // check results
                EventBean[] received = listener.GetNewDataListFlattened();
                Assert.AreEqual(matches.Count, received.Length);
                for (int i = 0; i < received.Length; i++) {
                    Assert.AreSame(matches[i], received[i].Get("event1"));
                }

                // Console.Out.WriteLine("Found " + received.Length + " matches in loop #" + countLoops);
                listener.Reset();
            }
        }

        #endregion
    }
}
