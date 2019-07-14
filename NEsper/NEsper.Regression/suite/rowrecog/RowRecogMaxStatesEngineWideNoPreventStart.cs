///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogMaxStatesEngineWideNoPreventStart : RegressionExecutionWithConfigure
    {
        private SupportConditionHandlerFactory.SupportConditionHandler handler;

        public bool EnableHATest => false;

        public bool HAWithCOnly => false;

        public void Configure(Configuration configuration)
        {
        }

        public void Run(RegressionEnvironment env)
        {
            var conditionHandlerFactoryContext =
                SupportConditionHandlerFactory.FactoryContexts[0];
            Assert.AreEqual(conditionHandlerFactoryContext.RuntimeURI, env.RuntimeURI);
            handler = SupportConditionHandlerFactory.LastHandler;

            var fields = "c0".SplitCsv();

            var epl = "@Name('s0') select * from SupportBean " +
                      "match_recognize (" +
                      "  partition by theString " +
                      "  measures P1.TheString as c0" +
                      "  pattern (P1 P2) " +
                      "  define " +
                      "    P1 as P1.IntPrimitive = 1," +
                      "    P2 as P2.IntPrimitive = 2" +
                      ")";

            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("A", 1));
            env.SendEventBean(new SupportBean("B", 1));
            env.SendEventBean(new SupportBean("C", 1));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            // overflow
            env.SendEventBean(new SupportBean("D", 1));
            RowRecogMaxStatesEngineWide3Instance.AssertContextEnginePool(
                env,
                env.Statement("s0"),
                handler.GetAndResetContexts(),
                3,
                RowRecogMaxStatesEngineWide3Instance.GetExpectedCountMap(env, "s0", 3));
            env.SendEventBean(new SupportBean("E", 1));
            RowRecogMaxStatesEngineWide3Instance.AssertContextEnginePool(
                env,
                env.Statement("s0"),
                handler.GetAndResetContexts(),
                3,
                RowRecogMaxStatesEngineWide3Instance.GetExpectedCountMap(env, "s0", 4));

            env.SendEventBean(new SupportBean("D", 2)); // D gone
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"D"});

            env.SendEventBean(new SupportBean("A", 2)); // A gone
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"A"});

            env.SendEventBean(new SupportBean("C", 2)); // C gone
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"C"});

            env.SendEventBean(new SupportBean("F", 1));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            env.SendEventBean(new SupportBean("G", 1));
            RowRecogMaxStatesEngineWide3Instance.AssertContextEnginePool(
                env,
                env.Statement("s0"),
                handler.GetAndResetContexts(),
                3,
                RowRecogMaxStatesEngineWide3Instance.GetExpectedCountMap(env, "s0", 3));

            env.UndeployAll();
        }
    }
} // end of namespace