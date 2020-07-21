///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeRowPerEventPerformance : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var statements = new EPStatement[100];
            var listeners = new SupportUpdateListener[statements.Length];
            for (var i = 0; i < statements.Length; i++) {
                var secondsWindowSpan = i % 30 + 1;
                var percent = 0.25 + i;
                var id = i % 5;

                var text = "@name('s" +
                           i +
                           "') select Symbol, min(Price) " +
                           "from SupportMarketDataBean(Id='${Id}')#time(${secondsWindowSpan})\n" +
                           "having Price >= min(Price) * ${percent}";

                text = text.Replace("${Id}", Convert.ToString(id));
                text = text.Replace("${secondsWindowSpan}", Convert.ToString(secondsWindowSpan));
                text = text.Replace("${percent}", Convert.ToString(percent));

                statements[i] = env.CompileDeploy(text).Statement("s" + i);
                listeners[i] = new SupportUpdateListener();
                statements[i].AddListener(listeners[i]);
            }

            var start = PerformanceObserver.MilliTime;
            var count = 0;
            for (var i = 0; i < 10000; i++) {
                count++;
                if (i % 10000 == 0) {
                    var now = PerformanceObserver.MilliTime;
                    var deltaSec = (now - start) / 1000.0;
                    var throughput = 10000.0 / deltaSec;
                    for (var j = 0; j < listeners.Length; j++) {
                        listeners[j].Reset();
                    }

                    start = now;
                }

                var bean = new SupportMarketDataIDBean("IBM", Convert.ToString(i % 5), 1);
                env.SendEventBean(bean);
            }

            var end = PerformanceObserver.MilliTime;
            var delta = end - start;
            Assert.That(delta, Is.LessThan(2000), "Delta=" + delta);
            //System.out.println("total=" + count + " delta=" + delta + " per sec:" + 10000.0 / (delta / 1000.0));

            env.UndeployAll();
        }
    }
} // end of namespace