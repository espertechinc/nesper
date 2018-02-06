///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

namespace com.espertech.esper.supportregression.multithread
{
    public class StmtMgmtCallable : ICallable<bool>
    {
        private readonly EPServiceProvider _engine;
        private readonly object[][] _statements;
        private readonly int _numRepeats;

        public StmtMgmtCallable(EPServiceProvider engine, object[][] statements, int numRepeats)
        {
            this._engine = engine;
            this._statements = statements;
            this._numRepeats = numRepeats;
        }

        public bool Call()
        {
            try
            {
                for (int loop = 0; loop < _numRepeats; loop++)
                {
                    foreach (object[] statement in _statements)
                    {
                        bool isEPL = statement[0].AsBoolean();
                        string statementText = (string) statement[1];

                        // Create EPL or pattern statement
                        EPStatement stmt;
                        ThreadLogUtil.Trace("stmt create,", statementText);
                        if (isEPL)
                        {
                            stmt = _engine.EPAdministrator.CreateEPL(statementText);
                        }
                        else
                        {
                            stmt = _engine.EPAdministrator.CreatePattern(statementText);
                        }

                        ThreadLogUtil.Trace("stmt done,", stmt);

                        // Add listener
                        var listener = new SupportMTUpdateListener();
                        LogUpdateListener logListener;
                        if (isEPL)
                        {
                            logListener = new LogUpdateListener(null);
                        }
                        else
                        {
                            logListener = new LogUpdateListener("a");
                        }

                        ThreadLogUtil.Trace("adding listeners ", listener, logListener);
                        stmt.Events += listener.Update;
                        stmt.Events += logListener.Update;

                        Object theEvent = MakeEvent();
                        ThreadLogUtil.Trace("sending event ", theEvent);
                        _engine.EPRuntime.SendEvent(theEvent);

                        // Should have received one or more events, one of them must be mine
                        EventBean[] newEvents = listener.GetNewDataListFlattened();
                        Assert.IsTrue(newEvents.Length >= 1, "No event received");
                        ThreadLogUtil.Trace("assert received, size is", newEvents.Length);
                        bool found = false;
                        for (int i = 0; i < newEvents.Length; i++)
                        {
                            var underlying = newEvents[i].Underlying;
                            if (!isEPL)
                            {
                                underlying = newEvents[i].Get("a");
                            }

                            if (underlying == theEvent)
                            {
                                found = true;
                            }
                        }

                        Assert.IsTrue(found);
                        listener.Reset();

                        // Stopping statement, the event should not be received, another event may however
                        ThreadLogUtil.Trace("stop statement");
                        stmt.Stop();
                        theEvent = MakeEvent();
                        ThreadLogUtil.Trace("send non-matching event ", theEvent);
                        _engine.EPRuntime.SendEvent(theEvent);

                        // Make sure the event was not received
                        newEvents = listener.GetNewDataListFlattened();
                        found = false;
                        for (int i = 0; i < newEvents.Length; i++)
                        {
                            var underlying = newEvents[i].Underlying;
                            if (!isEPL)
                            {
                                underlying = newEvents[i].Get("a");
                            }

                            if (underlying == theEvent)
                            {
                                found = true;
                            }
                        }

                        Assert.IsFalse(found);
                    }
                }
            }
            catch (AssertionException ex)
            {
                Log.Error("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }

            return true;
        }

        private SupportMarketDataBean MakeEvent()
        {
            var theEvent = new SupportMarketDataBean("IBM", 50, 1000L, "RT");
            return theEvent;
        }

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
