///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;

namespace com.espertech.esper.regression.multithread
{
    public class StmtNamedWindowConsumeCallable : ICallable<IList<String>>
    {
        private readonly EPServiceProvider _engine;
        private readonly int _numRepeats;
        private readonly String _threadKey;
    
        public StmtNamedWindowConsumeCallable(String threadKey, EPServiceProvider engine, int numRepeats)
        {
            _engine = engine;
            _numRepeats = numRepeats;
            _threadKey = threadKey;
        }

        public IList<String> Call()
        {
            IList<String> eventKeys = new List<String>(_numRepeats);
            try
            {
                for (int loop = 0; loop < _numRepeats; loop++)
                {
                    // Insert event into named window
                    String theEvent = "E" + _threadKey + "_" + loop;
                    eventKeys.Add(theEvent);
                    SendMarketBean(theEvent, 0);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return null;
            }
            return eventKeys;
        }
    
        private void SendMarketBean(String symbol, long volume)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, 0, volume, "");
            _engine.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
