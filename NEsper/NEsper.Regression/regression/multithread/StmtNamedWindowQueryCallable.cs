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
using com.espertech.esper.core.service;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class StmtNamedWindowQueryCallable : ICallable<bool>
    {
        private readonly EPRuntimeSPI _engine;
        private readonly int _numRepeats;
        private readonly String _threadKey;
    
        public StmtNamedWindowQueryCallable(String threadKey, EPServiceProvider engine, int numRepeats)
        {
            _engine = (EPRuntimeSPI) engine.EPRuntime;
            _numRepeats = numRepeats;
            _threadKey = threadKey;
        }

        public bool Call()
        {
            try
            {
                long total = 0;
                for (int loop = 0; loop < _numRepeats; loop++)
                {
                    // Insert event into named window
                    SendMarketBean(_threadKey, loop);
                    total++;
    
                    String selectQuery = "select * from MyWindow where TheString='" + _threadKey + "' and LongPrimitive=" + loop;
                    EPOnDemandQueryResult queryResult = _engine.ExecuteQuery(selectQuery);
                    Assert.AreEqual(1, queryResult.Array.Length);
                    Assert.AreEqual(_threadKey, queryResult.Array[0].Get("TheString"));
                    Assert.AreEqual((long)loop, queryResult.Array[0].Get("LongPrimitive"));
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            return true;
        }
    
        private void SendMarketBean(String symbol, long volume)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, 0, volume, "");
            _engine.SendEvent(bean);
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
