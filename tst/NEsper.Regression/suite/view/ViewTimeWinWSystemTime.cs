///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.view.derived;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework; // assertEquals

// assertTrue

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewTimeWinWSystemTime : RegressionExecution
    {
        private const string SYMBOL = "CSCO.O";
        private const string FEED = "feed1";

        public void Run(RegressionEnvironment env)
        {
            var epl =
                $"@name('s0') select * from SupportMarketDataBean(Symbol='{SYMBOL}')#time(3.0)#weighted_avg(Price, Volume, Symbol, Feed)";
            env.CompileDeployAddListenerMileZero(epl, "s0");

            env.AssertStatement(
                "s0",
                statement => { Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("Average")); });

            // Send 2 events, E1 and E2 at +0sec
            env.SendEventBean(MakeBean(SYMBOL, 10, 500));
            CheckValue(env, 10);

            env.SendEventBean(MakeBean(SYMBOL, 11, 500));
            CheckValue(env, 10.5);

            // Sleep for 1.5 seconds
            Sleep(1500);

            // Send 2 more events, E3 and E4 at +1.5sec
            env.SendEventBean(MakeBean(SYMBOL, 10, 1000));
            CheckValue(env, 10.25);
            env.SendEventBean(MakeBean(SYMBOL, 10.5, 2000));
            CheckValue(env, 10.375);

            // Sleep for 2 seconds, E1 and E2 should have left the window
            Sleep(2000);
            CheckValue(env, 10.333333333);

            // Send another event, E5 at +3.5sec
            env.SendEventBean(MakeBean(SYMBOL, 10.2, 1000));
            CheckValue(env, 10.3);

            // Sleep for 2.5 seconds, E3 and E4 should expire
            Sleep(2500);
            CheckValue(env, 10.2);

            // Sleep for 1 seconds, E5 should have expired
            Sleep(1000);
            CheckValue(env, double.NaN);

            env.UndeployAll();
        }

        private static SupportMarketDataBean MakeBean(
            string symbol,
            double price,
            long volume)
        {
            return new SupportMarketDataBean(symbol, price, volume, FEED);
        }

        private void CheckValue(
            RegressionEnvironment env,
            double avgE)
        {
            env.AssertIterator(
                "s0",
                iterator => {
                    CheckValue(iterator.Advance(), avgE);
                    Assert.IsTrue(!iterator.MoveNext());
                });

            env.AssertListener(
                "s0",
                listener => {
                    Assert.AreEqual(1, listener.LastNewData.Length);
                    var listenerValues = listener.LastNewData[0];
                    CheckValue(listenerValues, avgE);
                    listener.Reset();
                });
        }

        private void CheckValue(
            EventBean values,
            double avgE)
        {
            var avg = GetDoubleValue(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE, values);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(avg, avgE, 6));
            Assert.AreEqual(FEED, values.Get("Feed"));
            Assert.AreEqual(SYMBOL, values.Get("Symbol"));
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
            catch (ThreadInterruptedException e) {
            }
        }
    }
} // end of namespace