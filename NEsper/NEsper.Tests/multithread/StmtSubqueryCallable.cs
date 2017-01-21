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
    public class StmtSubqueryCallable : ICallable<bool>
    {
        private readonly int _threadNum;
        private readonly EPServiceProvider _engine;
        private readonly int _numRepeats;
    
        public StmtSubqueryCallable(int threadNum, EPServiceProvider engine, int numRepeats)
        {
            _threadNum = threadNum;
            _engine = engine;
            _numRepeats = numRepeats;
        }

        public bool Call()
        {
            try
            {
                for (int loop = 0; loop < _numRepeats; loop++)
                {
                    int id = _threadNum * 10000000 + loop;
                    Object eventS0 = new SupportBean_S0(id);
                    Object eventS1 = new SupportBean_S1(id);
    
                    _engine.EPRuntime.SendEvent(eventS0);
                    _engine.EPRuntime.SendEvent(eventS1);
                }

                return true;
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
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
