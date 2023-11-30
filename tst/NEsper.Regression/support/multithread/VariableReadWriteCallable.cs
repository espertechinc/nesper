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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class VariableReadWriteCallable : ICallable<object>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int numRepeats;
        private readonly EPRuntime runtime;
        private readonly SupportUpdateListener selectListener;
        private readonly int threadNum;

        public VariableReadWriteCallable(
            int threadNum,
            RegressionEnvironment env,
            int numRepeats)
        {
            runtime = env.Runtime;
            this.numRepeats = numRepeats;
            this.threadNum = threadNum;

            selectListener = new SupportUpdateListener();
            var stmtText = $"@name('t{threadNum}') select var1, var2, var3 from SupportBean_A(Id='{threadNum}')";
            env.CompileDeploy(stmtText).Statement("t" + threadNum).AddListener(selectListener);
        }

        public object Call()
        {
            try {
                for (var loop = 0; loop < numRepeats; loop++) {
                    long newValue = threadNum * 1000000 + loop;
                    object theEvent;

                    if (loop % 2 == 0) {
                        theEvent = new SupportMarketDataBean("", 0, newValue, "");
                    }
                    else {
                        var bean = new SupportBean();
                        bean.LongPrimitive = newValue;
                        theEvent = bean;
                    }

                    // Changes the variable values through either of the set-statements
                    runtime.EventService.SendEventBean(theEvent, theEvent.GetType().Name);

                    // Select the variable value back, another thread may have changed it, we are only
                    // determining if the set operation is atomic
                    runtime.EventService.SendEventBean(
                        new SupportBean_A(Convert.ToString(threadNum)),
                        nameof(SupportBean_A));
                    var received = selectListener.AssertOneGetNewAndReset();
                    Assert.AreEqual(received.Get("var1"), received.Get("var2"));
                }
            }
            catch (AssertionException ex) {
                log.Error("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            catch (Exception ex) {
                log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }

            return true;
        }
    }
} // end of namespace