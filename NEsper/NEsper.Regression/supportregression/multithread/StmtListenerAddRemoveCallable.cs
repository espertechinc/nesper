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
    public class StmtListenerAddRemoveCallable : ICallable<bool>
    {
        private readonly EPServiceProvider _engine;
        private readonly EPStatement _stmt;
        private readonly bool isEPL;
        private readonly int _numRepeats;

        public StmtListenerAddRemoveCallable(EPServiceProvider engine, EPStatement stmt, bool isEPL, int numRepeats)
        {
            this._engine = engine;
            this._stmt = stmt;
            this.isEPL = isEPL;
            this._numRepeats = numRepeats;
        }

        public bool Call()
        {
            try
            {
                for (int loop = 0; loop < _numRepeats; loop++)
                {
                    // Add assertListener
                    var assertListener = new SupportMTUpdateListener();
                    LogUpdateListener logListener;
                    if (isEPL)
                    {
                        logListener = new LogUpdateListener(null);
                    }
                    else
                    {
                        logListener = new LogUpdateListener("a");
                    }

                    ThreadLogUtil.Trace("adding listeners ", assertListener, logListener);
                    _stmt.Events += assertListener.Update;
                    _stmt.Events += logListener.Update;

                    // send event
                    Object theEvent = MakeEvent();
                    ThreadLogUtil.Trace("sending event ", theEvent);
                    _engine.EPRuntime.SendEvent(theEvent);

                    // Should have received one or more events, one of them must be mine
                    EventBean[] newEvents = assertListener.GetNewDataListFlattened();
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
                    assertListener.Reset();

                    // Remove assertListener
                    ThreadLogUtil.Trace("removing assertListener");
                    _stmt.Events -= assertListener.Update;
                    _stmt.Events -= logListener.Update;

                    // Send another event
                    theEvent = MakeEvent();
                    ThreadLogUtil.Trace("send non-matching event ", theEvent);
                    _engine.EPRuntime.SendEvent(theEvent);

                    // Make sure the event was not received
                    newEvents = assertListener.GetNewDataListFlattened();
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
