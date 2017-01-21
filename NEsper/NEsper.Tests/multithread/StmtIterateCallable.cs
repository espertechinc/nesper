///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    public class StmtIterateCallable : ICallable<bool>
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _threadNum;
        private readonly EPServiceProvider _engine;
        private readonly EPStatement[] _stmt;
        private readonly int _numRepeats;
    
        public StmtIterateCallable(int threadNum, EPServiceProvider engine, EPStatement[] stmt, int numRepeats)
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
                for (int loop = 0; loop < _numRepeats; loop++)
                {
                    Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " sending event " + loop);
                    String id = Convert.ToString(_threadNum * 100000000 + loop);
                    SupportBean bean = new SupportBean(id, 0);
                    _engine.EPRuntime.SendEvent(bean);
    
                    for (int i = 0; i < _stmt.Length; i++)
                    {
                        Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " starting enumerator " + loop);
                        bool found = false;

                        using (var en = _stmt[i].GetSafeEnumerator())
                        {
                            while (en.MoveNext()) {
                                EventBean theEvent = en.Current;
                                if (theEvent.Get("TheString").Equals(id)) {
                                    found = true;
                                }
                            }
                        }

                        Assert.IsTrue(found);
                        Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " end enumerator " + loop);
                    }
                }
            }
            catch (AssertionException ex)
            {
                Log.Fatal("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            catch (Exception e)
            {
                Log.Fatal("Error in thread " + Thread.CurrentThread.ManagedThreadId, e);
                return false;
            }
            return true;
        }
    }
}
