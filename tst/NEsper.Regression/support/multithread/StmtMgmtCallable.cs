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

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtMgmtCallable : ICallable<object>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int numRepeats;
        private readonly EPRuntime runtime;
        private readonly StmtMgmtCallablePair[] statements;

        public StmtMgmtCallable(
            EPRuntime runtime,
            StmtMgmtCallablePair[] statements,
            int numRepeats)
        {
            this.runtime = runtime;
            this.statements = statements;
            this.numRepeats = numRepeats;
        }

        public object Call()
        {
            try {
                for (var loop = 0; loop < numRepeats; loop++) {
                    foreach (var statement in statements) {
                        var statementText = statement.Epl;
                        var compiled = statement.Compiled;

                        // Create EPL or pattern statement
                        ThreadLogUtil.Trace("stmt create,", statementText);
                        var deployed = runtime.DeploymentService.Deploy(compiled);
                        ThreadLogUtil.Trace("stmt done,", statementText);

                        // Add listener
                        var listener = new SupportMTUpdateListener();
                        var logListener = new LogUpdateListener(null);
                        ThreadLogUtil.Trace("adding listeners ", listener, logListener);
                        deployed.Statements[0].AddListener(listener);
                        deployed.Statements[0].AddListener(logListener);

                        object theEvent = MakeEvent();
                        ThreadLogUtil.Trace("sending event ", theEvent);
                        runtime.EventService.SendEventBean(theEvent, theEvent.GetType().Name);

                        // Should have received one or more events, one of them must be mine
                        var newEvents = listener.GetNewDataListFlattened();
                        Assert.IsTrue(newEvents.Length >= 1, "No event received");
                        ThreadLogUtil.Trace("assert received, size is", newEvents.Length);
                        var found = false;
                        for (var i = 0; i < newEvents.Length; i++) {
                            var underlying = newEvents[i].Underlying;
                            if (underlying == theEvent) {
                                found = true;
                            }
                        }

                        Assert.IsTrue(found);
                        listener.Reset();

                        // Stopping statement, the event should not be received, another event may however
                        ThreadLogUtil.Trace("stop statement");
                        runtime.DeploymentService.Undeploy(deployed.DeploymentId);
                        theEvent = MakeEvent();
                        ThreadLogUtil.Trace("send non-matching event ", theEvent);
                        runtime.EventService.SendEventBean(theEvent, theEvent.GetType().Name);

                        // Make sure the event was not received
                        newEvents = listener.GetNewDataListFlattened();
                        found = false;
                        for (var i = 0; i < newEvents.Length; i++) {
                            var underlying = newEvents[i].Underlying;
                            if (underlying == theEvent) {
                                found = true;
                            }
                        }

                        Assert.IsFalse(found);
                    }
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

        private SupportMarketDataBean MakeEvent()
        {
            return new SupportMarketDataBean("IBM", 50, 1000L, "RT");
        }
    }
} // end of namespace