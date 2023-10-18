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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtListenerRouteCallable : ICallable<object>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly RegressionEnvironment env;
        private readonly int numRepeats;
        private readonly int numThread;
        private readonly EPStatement statement;

        public StmtListenerRouteCallable(
            int numThread,
            RegressionEnvironment env,
            EPStatement statement,
            int numRepeats)
        {
            this.numThread = numThread;
            this.env = env;
            this.numRepeats = numRepeats;
            this.statement = statement;
        }

        public object Call()
        {
            try {
                for (var loop = 0; loop < numRepeats; loop++) {
                    var listener = new MyUpdateListener(env, numThread);
                    statement.AddListener(listener);
                    env.SendEventBean(new SupportBean(), "SupportBean");
                    statement.RemoveListener(listener);
                    listener.AssertCalled();
                }
            }
            catch (AssertionException ex) {
                Console.Error.WriteLine("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                Console.Error.WriteLine(ex.StackTrace);
                log.Error("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error in thread " + Thread.CurrentThread.ManagedThreadId);
                Console.Error.WriteLine(ex.StackTrace);
                log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }

            return true;
        }

        private class MyUpdateListener : UpdateListener
        {
            private readonly RegressionEnvironment _env;
            private readonly int _numThread;
            private readonly EPCompiled _compiled;
            private bool _isCalled;

            public MyUpdateListener(
                RegressionEnvironment env,
                int numThread)
            {
                this._env = env;
                this._numThread = numThread;
                _compiled = env.Compile(
                    "@name('t" + numThread + "') select * from SupportMarketDataBean where Volume=" + numThread);
            }

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                _isCalled = true;

                // create statement for thread - this can be called multiple times as other threads send SupportBean
                _env.Deploy(_compiled);
                var listener = new SupportMTUpdateListener();
                _env.Statement("t" + _numThread).AddListener(listener);

                object theEvent = new SupportMarketDataBean("", 0, _numThread, null);
                _env.SendEventBean(theEvent, theEvent.GetType().Name);
                _env.UndeployModuleContaining("t" + _numThread);

                var eventsReceived = listener.NewDataListFlattened;

                var found = false;
                for (var i = 0; i < eventsReceived.Length; i++) {
                    if (eventsReceived[i].Underlying == theEvent) {
                        found = true;
                    }
                }

                Assert.IsTrue(found);
            }

            public void AssertCalled()
            {
                Assert.IsTrue(_isCalled);
            }
        }
    }
} // end of namespace