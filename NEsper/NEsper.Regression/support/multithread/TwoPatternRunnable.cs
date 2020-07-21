///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class TwoPatternRunnable : IRunnable
    {
        private readonly RegressionEnvironment env;
        private readonly SupportUpdateListener listener;
        private bool isShutdown;

        public TwoPatternRunnable(RegressionEnvironment env)
        {
            this.env = env;
            listener = new SupportUpdateListener();
        }

        public bool Shutdown {
            set => isShutdown = value;
        }

        public void Run()
        {
            var stmtText =
                "@name('s0') select * from pattern[every event1=SupportTradeEvent(userId in ('100','101'),amount>=1000)]";
            env.CompileDeploy(stmtText);
            env.Statement("s0").AddListener(listener);

            while (!isShutdown) {
                IList<SupportTradeEvent> matches = new List<SupportTradeEvent>();

                for (var i = 0; i < 10000; i++) {
                    SupportTradeEvent bean;
                    if (i % 1000 == 1) {
                        bean = new SupportTradeEvent(i, "100", 1001);
                        matches.Add(bean);
                    }
                    else {
                        bean = new SupportTradeEvent(i, "101", 10);
                    }

                    env.SendEventBean(bean);

                    if (isShutdown) {
                        break;
                    }
                }

                // check results
                var received = listener.NewDataListFlattened;
                Assert.AreEqual(matches.Count, received.Length);
                for (var i = 0; i < received.Length; i++) {
                    Assert.AreSame(matches[i], received[i].Get("event1"));
                }

                // System.out.println("Found " + received.Length + " matches in loop #" + countLoops);
                listener.Reset();
            }
        }
    }
} // end of namespace