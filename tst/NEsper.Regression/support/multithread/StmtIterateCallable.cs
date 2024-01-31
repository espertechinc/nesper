///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtIterateCallable : ICallable<object>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int numRepeats;
        private readonly EPRuntime runtime;
        private readonly EPStatement[] stmt;
        private readonly int threadNum;

        public StmtIterateCallable(
            int threadNum,
            EPRuntime runtime,
            EPStatement[] stmt,
            int numRepeats)
        {
            this.threadNum = threadNum;
            this.runtime = runtime;
            this.stmt = stmt;
            this.numRepeats = numRepeats;
        }

        public object Call()
        {
            try {
                for (var loop = 0; loop < numRepeats; loop++) {
                    Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " sending event " + loop);
                    var id = Convert.ToString(threadNum * 100000000 + loop);
                    var bean = new SupportBean(id, 0);
                    runtime.EventService.SendEventBean(bean, bean.GetType().Name);

                    for (var i = 0; i < stmt.Length; i++) {
                        Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " starting iterator " + loop);

                        var found = false;

                        using (IEnumerator<EventBean> enumerator = stmt[i].GetSafeEnumerator()) {
                            while (enumerator.MoveNext()) {
                                var theEvent = enumerator.Current;
                                if (theEvent.Get("TheString").Equals(id)) {
                                    found = true;
                                }
                            }
                        }

                        ClassicAssert.IsTrue(found);
                        Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " end iterator " + loop);
                    }
                }

                //} catch (AssertionException ex) {
                //    Log.Error("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                //    return false;
            }
            catch (Exception t) {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, t);
                return false;
            }

            return true;
        }
    }
} // end of namespace