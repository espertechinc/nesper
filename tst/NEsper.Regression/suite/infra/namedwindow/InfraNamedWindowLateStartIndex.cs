///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowLateStartIndex : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
        }

        public void Run(RegressionEnvironment env)
        {
            // prepare
            var path = PreloadData(env, false);

            // test join
            var eplJoin =
                "@name('s0') select * from SupportBean_S0 as S0 unidirectional, AWindow(P00='x') as aw where aw.Id = S0.Id";
            env.CompileDeploy(eplJoin, path).AddListener("s0");
            Assert.AreEqual(2, SupportCountAccessEvent.GetAndResetCountGetterCalled());

            env.SendEventBean(new SupportBean_S0(-1, "x"));
            env.AssertListenerInvoked("s0");

            // test subquery no-index-share
            var eplSubqueryNoIndexShare =
                "@name('s1') select (select Id from AWindow(P00='x') as aw where aw.Id = S0.Id) " +
                "from SupportBean_S0 as S0 unidirectional";
            env.CompileDeploy(eplSubqueryNoIndexShare, path).AddListener("s1");
            Assert.AreEqual(2, SupportCountAccessEvent.GetAndResetCountGetterCalled());

            env.SendEventBean(new SupportBean_S0(-1, "x"));
            env.UndeployAll();

            // test subquery with index share
            path = PreloadData(env, true);

            var eplSubqueryWithIndexShare =
                "@name('s2') select (select Id from AWindow(P00='x') as aw where aw.Id = S0.Id) " +
                "from SupportBean_S0 as S0 unidirectional";
            env.CompileDeploy(eplSubqueryWithIndexShare, path).AddListener("s2");
            Assert.AreEqual(2, SupportCountAccessEvent.GetAndResetCountGetterCalled());

            env.SendEventBean(new SupportBean_S0(-1, "x"));
            env.AssertListenerInvoked("s2");

            env.UndeployAll();
        }

        private static RegressionPath PreloadData(
            RegressionEnvironment env,
            bool indexShare)
        {
            var path = new RegressionPath();
            var createEpl = "@public create window AWindow#keepall as SupportCountAccessEvent";
            if (indexShare) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }

            env.CompileDeploy(createEpl, path);
            env.CompileDeploy("insert into AWindow select * from SupportCountAccessEvent", path);
            env.CompileDeploy("create index I1 on AWindow(P00)", path);
            SupportCountAccessEvent.GetAndResetCountGetterCalled();
            for (var i = 0; i < 100; i++) {
                env.SendEventBean(new SupportCountAccessEvent(i, "E" + i));
            }

            env.SendEventBean(new SupportCountAccessEvent(-1, "x"));
            Assert.AreEqual(101, SupportCountAccessEvent.GetAndResetCountGetterCalled());
            return path;
        }
    }
} // end of namespace