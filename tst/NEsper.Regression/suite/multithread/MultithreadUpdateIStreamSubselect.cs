///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.client.SupportCompileDeployUtil;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadUpdateIStreamSubselect : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy(
                "@Name('s0') update istream SupportBean as sb set LongPrimitive = (select count(*) from SupportBean_S0#keepall as S0 where S0.P00 = sb.TheString)");
            var listener = new SupportUpdateListener();
            env.Statement("s0").AddListener(listener);

            // insert 5 data events for each symbol
            var numGroups = 20;
            var numRepeats = 5;
            for (var i = 0; i < numGroups; i++) {
                for (var j = 0; j < numRepeats; j++) {
                    env.SendEventBean(new SupportBean_S0(i, "S0_" + i)); // S0_0 .. S0_19 each has 5 events
                }
            }

            IList<Thread> threads = new List<Thread>();
            for (var i = 0; i < numGroups; i++) {
                var group = i;
                var t = new Thread(() => env.SendEventBean(new SupportBean("S0_" + group, 1))) {
                    Name = typeof(MultithreadUpdateIStreamSubselect).Name
                };
                threads.Add(t);
                t.Start();
            }

            foreach (var t in threads) {
                ThreadJoin(t);
            }

            // validate results, price must be 5 for each symbol
            Assert.AreEqual(numGroups, listener.NewDataList.Count);
            foreach (var newData in listener.NewDataList) {
                var result = (SupportBean) newData[0].Underlying;
                Assert.AreEqual(numRepeats, result.LongPrimitive);
            }

            env.UndeployAll();
        }
    }
} // end of namespace