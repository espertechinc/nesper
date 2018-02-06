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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class StmtNamedWindowIterateCallable : ICallable<bool>
    {
        private readonly EPServiceProvider _engine;
        private readonly int _numRepeats;
        private readonly String _threadKey;
        private readonly EPStatement _statement;
    
        public StmtNamedWindowIterateCallable(String threadKey, EPServiceProvider engine, int numRepeats)
        {
            _engine = engine;
            _numRepeats = numRepeats;
            _threadKey = threadKey;
            _statement = engine.EPAdministrator.CreateEPL("select TheString, sum(LongPrimitive) as sumLong from MyWindow group by TheString");
        }
    
        public bool Call()
        {
            try
            {
                long total = 0;
                for (int loop = 0; loop < _numRepeats; loop++)
                {
                    // Insert event into named window
                    SendMarketBean(_threadKey, loop + 1);
                    total += loop + 1;
    
                    // iterate over private statement
                    var en = _statement.GetSafeEnumerator();
                    var received = EPAssertionUtil.EnumeratorToArray(en);
                    en.Dispose();

                    for (int i = 0; i < received.Length; i++)
                    {
                        if (Equals(received[i].Get("TheString"), _threadKey))
                        {
                            long? sum = (long?)received[i].Get("sumLong");
                            Assert.That(sum, Is.Not.Null);
                            Assert.That(sum.Value, Is.EqualTo(total));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                System.Diagnostics.Debug.WriteLine(
                    "mthread = " + Thread.CurrentThread.ManagedThreadId +
                    ", thread = " + _threadKey +
                    ", exit = false" + 
                    ", exception = " + ex
                    );

                return false;
            }

            return true;
        }
    
        private void SendMarketBean(String symbol, long volume)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, 0, volume, "");
            _engine.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
