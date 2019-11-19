///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtNamedWindowIterateCallable : ICallable<object>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly RegressionEnvironment env;
        private readonly int numRepeats;
        private readonly string threadKey;
        private readonly EPStatement statement;

        public StmtNamedWindowIterateCallable(
            string threadKey,
            RegressionEnvironment env,
            RegressionPath path,
            int numRepeats)
        {
            this.env = env;
            this.numRepeats = numRepeats;
            this.threadKey = threadKey;

            var stmtName = UuidGenerator.Generate();
            env.CompileDeploy(
                "@Name('" +
                stmtName +
                "') select TheString, sum(LongPrimitive) as sumLong from MyWindow group by TheString",
                path);
            statement = env.Statement(stmtName);
        }

        public object Call()
        {
            try {
                long total = 0;
                for (var loop = 0; loop < numRepeats; loop++) {
                    // Insert event into named window
                    SendMarketBean(threadKey, loop + 1);
                    total += loop + 1;

                    EventBean[] received;

                    // iterate over private statement
                    using (var safeIter = statement.GetSafeEnumerator()) {
                        received = EPAssertionUtil.EnumeratorToArray(safeIter);
                    }

                    for (var i = 0; i < received.Length; i++) {
                        if (received[i].Get("TheString").Equals(threadKey)) {
                            var sum = received[i].Get("sumLong").AsLong();
                            Assert.AreEqual(total, sum);
                        }
                    }
                }
            }
            catch (Exception ex) {
                log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }

            return true;
        }

        private void SendMarketBean(
            string symbol,
            long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            env.SendEventBean(bean);
        }
    }
} // end of namespace