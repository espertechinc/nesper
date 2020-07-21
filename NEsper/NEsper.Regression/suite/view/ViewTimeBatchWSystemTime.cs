///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.view.derived;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewTimeBatchWSystemTime : RegressionExecution
    {
        private const string SYMBOL = "CSCO.O";

        public void Run(RegressionEnvironment env)
        {
            // Set up a 2 second time window
            var epl = "@name('s0') select * from SupportMarketDataBean(Symbol='" +
                      SYMBOL +
                      "')#time_batch(2)#uni(Volume)";
            env.CompileDeployAddListenerMileZero(epl, "s0");

            CheckMeanIterator(env, double.NaN);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Send a couple of events, check mean
            SendEvent(env, SYMBOL, 500);
            SendEvent(env, SYMBOL, 1000);
            CheckMeanIterator(env, double.NaN); // The iterator is still showing no result yet as no batch was released
            Assert.IsFalse(env.Listener("s0").IsInvoked); // No new data posted to the iterator, yet

            // Sleep for 1 seconds
            Sleep(1000);

            // Send more events
            SendEvent(env, SYMBOL, 1000);
            SendEvent(env, SYMBOL, 1200);
            CheckMeanIterator(env, double.NaN); // The iterator is still showing no result yet as no batch was released
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Sleep for 1.5 seconds, thus triggering a new batch
            Sleep(1500);
            CheckMeanIterator(env, 925); // Now the statistics view received the first batch
            Assert.IsTrue(env.Listener("s0").IsInvoked); // Listener has been invoked
            CheckMeanListener(env, 925);

            // Send more events
            SendEvent(env, SYMBOL, 500);
            SendEvent(env, SYMBOL, 600);
            SendEvent(env, SYMBOL, 1000);
            CheckMeanIterator(env, 925); // The iterator is still showing the old result as next batch not released
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Sleep for 1 seconds
            Sleep(1000);

            // Send more events
            SendEvent(env, SYMBOL, 200);
            CheckMeanIterator(env, 925);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Sleep for 1.5 seconds, thus triggering a new batch
            Sleep(1500);
            CheckMeanIterator(
                env,
                2300d / 4d); // Now the statistics view received the second batch, the mean now is over all events
            Assert.IsTrue(env.Listener("s0").IsInvoked); // Listener has been invoked
            CheckMeanListener(env, 2300d / 4d);

            // Send more events
            SendEvent(env, SYMBOL, 1200);
            CheckMeanIterator(env, 2300d / 4d);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Sleep for 2 seconds, no events received anymore
            Sleep(2000);
            CheckMeanIterator(env, 1200); // statistics view received the third batch
            Assert.IsTrue(env.Listener("s0").IsInvoked); // Listener has been invoked
            CheckMeanListener(env, 1200);

            env.UndeployAll();
        }

        private void SendEvent(
            RegressionEnvironment env,
            string symbol,
            long volume)
        {
            var theEvent = new SupportMarketDataBean(symbol, 0, volume, "");
            env.SendEventBean(theEvent);
        }

        private void CheckMeanListener(
            RegressionEnvironment env,
            double meanExpected)
        {
            Assert.IsTrue(env.Listener("s0").LastNewData.Length == 1);
            var listenerValues = env.Listener("s0").LastNewData[0];
            CheckValue(listenerValues, meanExpected);
            env.Listener("s0").Reset();
        }

        private void CheckMeanIterator(
            RegressionEnvironment env,
            double meanExpected)
        {
            var iterator = env.Statement("s0").GetEnumerator();
            CheckValue(iterator.Advance(), meanExpected);
            Assert.IsTrue(!iterator.MoveNext());
        }

        private void CheckValue(
            EventBean values,
            double avgE)
        {
            var avg = GetDoubleValue(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE, values);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(avg, avgE, 6));
        }

        private double GetDoubleValue(
            ViewFieldEnum field,
            EventBean theEvent)
        {
            return theEvent.Get(field.GetName()).AsDouble();
        }

        private void Sleep(int msec)
        {
            try {
                Thread.Sleep(msec);
            }
            catch (ThreadInterruptedException) {
            }
        }
    }
} // end of namespace