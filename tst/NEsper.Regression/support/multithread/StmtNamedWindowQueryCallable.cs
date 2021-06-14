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

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtNamedWindowQueryCallable : ICallable<object>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly RegressionEnvironment env;
        private readonly int numRepeats;
        private readonly RegressionPath path;
        private readonly string threadKey;

        public StmtNamedWindowQueryCallable(
            RegressionEnvironment env,
            RegressionPath path,
            int numRepeats,
            string threadKey)
        {
            this.env = env;
            this.path = path;
            this.numRepeats = numRepeats;
            this.threadKey = threadKey;
        }

        public object Call()
        {
            var selectQuery = "select * from MyWindow where TheString='" + threadKey + "' and LongPrimitive=?::int";
            var compiled = env.CompileFAF(selectQuery, path);
            var prepared = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);

            try {
                long total = 0;
                for (var loop = 0; loop < numRepeats; loop++) {
                    // Insert event into named window
                    SendMarketBean(threadKey, loop);
                    total++;

                    prepared.SetObject(1, loop);
                    var queryResult = env.Runtime.FireAndForgetService.ExecuteQuery(prepared);
                    Assert.AreEqual(1, queryResult.Array.Length);
                    Assert.AreEqual(threadKey, queryResult.Array[0].Get("TheString"));
                    Assert.AreEqual((long) loop, queryResult.Array[0].Get("LongPrimitive"));
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