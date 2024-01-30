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

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtSubqueryCallable : ICallable<object>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int numRepeats;
        private readonly EPRuntime runtime;
        private readonly int threadNum;

        public StmtSubqueryCallable(
            int threadNum,
            EPRuntime runtime,
            int numRepeats)
        {
            this.threadNum = threadNum;
            this.runtime = runtime;
            this.numRepeats = numRepeats;
        }

        public object Call()
        {
            try {
                for (var loop = 0; loop < numRepeats; loop++) {
                    var id = threadNum * 10000000 + loop;
                    object eventS0 = new SupportBean_S0(id);
                    object eventS1 = new SupportBean_S1(id);

                    runtime.EventService.SendEventBean(eventS0, "SupportBean_S0");
                    runtime.EventService.SendEventBean(eventS1, "SupportBean_S1");
                }
            }
            catch (AssertionException ex) {
                Log.Error("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            catch (Exception ex) {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }

            return true;
        }
    }
} // end of namespace