///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextKeySegmentedPrioritized : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@public create context SegmentedByMessage partition by TheString from SupportBean",
                path);

            env.CompileDeploy(
                "@name('s0') @Drop @Priority(1) context SegmentedByMessage select 'test1' from SupportBean",
                path);
            env.AddListener("s0");

            env.CompileDeploy(
                "@name('s1') @Priority(0) context SegmentedByMessage select 'test2' from SupportBean",
                path);
            env.AddListener("s1");

            env.SendEventBean(new SupportBean("test msg", 1));

            env.AssertListenerInvoked("s0");
            env.AssertListenerNotInvoked("s1");

            env.UndeployAll();
        }
    }
} // end of namespace