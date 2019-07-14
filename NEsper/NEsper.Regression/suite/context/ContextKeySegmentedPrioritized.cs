///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextKeySegmentedPrioritized : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create context SegmentedByMessage partition by theString from SupportBean", path);

            env.CompileDeploy(
                "@Name('s0') @Drop @Priority(1) context SegmentedByMessage select 'test1' from SupportBean",
                path);
            env.AddListener("s0");

            env.CompileDeploy(
                "@Name('s1') @Priority(0) context SegmentedByMessage select 'test2' from SupportBean",
                path);
            env.AddListener("s1");

            env.SendEventBean(new SupportBean("test msg", 1));

            Assert.IsTrue(env.Listener("s0").IsInvoked);
            Assert.IsFalse(env.Listener("s1").IsInvoked);

            env.UndeployAll();
        }
    }
} // end of namespace