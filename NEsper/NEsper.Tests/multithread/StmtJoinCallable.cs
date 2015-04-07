///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    public class StmtJoinCallable : ICallable<bool>
    {
        private readonly int _threadNum;
        private readonly EPServiceProvider _engine;
        private readonly EPStatement _stmt;
        private readonly int _numRepeats;
    
        public StmtJoinCallable(int threadNum, EPServiceProvider engine, EPStatement stmt, int numRepeats)
        {
            _threadNum = threadNum;
            _engine = engine;
            _stmt = stmt;
            _numRepeats = numRepeats;
        }
    
        public bool Call()
        {
            try
            {
                // Add assertListener
                SupportMTUpdateListener assertListener = new SupportMTUpdateListener();
                ThreadLogUtil.Trace("adding listeners ", assertListener);
                _stmt.Events += assertListener.Update;
    
                for (int loop = 0; loop < _numRepeats; loop++)
                {
                    long id = _threadNum * 100000000 + loop;
                    Object eventS0 = MakeEvent("s0", id);
                    Object eventS1 = MakeEvent("s1", id);
    
                    ThreadLogUtil.Trace("SENDING s0 event ", id, eventS0);
                    _engine.EPRuntime.SendEvent(eventS0);
                    ThreadLogUtil.Trace("SENDING s1 event ", id, eventS1);
                    _engine.EPRuntime.SendEvent(eventS1);
    
                    //ThreadLogUtil.Info("sent", eventS0, eventS1);
                    // Should have received one that's mine, possible multiple since the statement is used by other threads
                    bool found = false;
                    EventBean[] events = assertListener.GetNewDataListFlattened();
                    foreach (EventBean theEvent in events)
                    {
                        Object s0Received = theEvent.Get("s0");
                        Object s1Received = theEvent.Get("s1");
                        //ThreadLogUtil.Info("received", theEvent.Get("s0"), theEvent.Get("s1"));
                        if ((s0Received == eventS0) && (s1Received == eventS1))
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                    }
                    Assert.IsTrue(found);
                    assertListener.Reset();
                }
            }
            catch (AssertionException ex)
            {
                Log.Fatal("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Fatal("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            return true;
        }
    
        private SupportBean MakeEvent(String stringValue, long longPrimitive)
        {
            SupportBean theEvent = new SupportBean();
            theEvent.LongPrimitive = longPrimitive;
            theEvent.TheString = stringValue;
            return theEvent;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
