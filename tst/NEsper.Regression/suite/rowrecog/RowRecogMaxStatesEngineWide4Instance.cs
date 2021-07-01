///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogMaxStatesEngineWide4Instance : RegressionExecutionWithConfigure
    {
        private SupportConditionHandlerFactory.SupportConditionHandler handler;

        public void Configure(Configuration configuration)
        {
        }

        public bool EnableHATest => false;

        public bool HAWithCOnly => false;

        public void Run(RegressionEnvironment env)
        {
            handler = SupportConditionHandlerFactory.LastHandler;
            var fields = new [] { "c0" };

            var eplOne = "@Name('S1') select * from SupportBean(TheString = 'A') " +
                         "match_recognize (" +
                         "  partition by IntPrimitive " +
                         "  measures P2.IntPrimitive as c0" +
                         "  pattern (P1 P2) " +
                         "  define " +
                         "    P1 as P1.LongPrimitive = 1," +
                         "    P2 as P2.LongPrimitive = 2" +
                         ")";
            env.CompileDeploy(eplOne).AddListener("S1");

            var eplTwo = "@Name('S2') select * from SupportBean(TheString = 'B')#length(2) " +
                         "match_recognize (" +
                         "  partition by IntPrimitive " +
                         "  measures P2.IntPrimitive as c0" +
                         "  pattern (P1 P2) " +
                         "  define " +
                         "    P1 as P1.LongPrimitive = 1," +
                         "    P2 as P2.LongPrimitive = 2" +
                         ")";
            env.CompileDeploy(eplTwo).AddListener("S2");

            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("A", 100, 1));
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("A", 200, 1));
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("B", 100, 1));
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("B", 200, 1));
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("B", 300, 1));
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("B", 400, 1));
            EPAssertionUtil.EnumeratorToArray(env.Statement("S2").GetEnumerator());
            Assert.IsTrue(handler.Contexts.IsEmpty());

            // overflow
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("A", 300, 1));
            RowRecogMaxStatesEngineWide3Instance.AssertContextEnginePool(
                env,
                env.Statement("S1"),
                handler.GetAndResetContexts(),
                4,
                RowRecogMaxStatesEngineWide3Instance.GetExpectedCountMap(env, "S1", 2, "S2", 2));

            // terminate B
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("B", 400, 2));
            EPAssertionUtil.AssertProps(
                env.Listener("S2").AssertOneGetNewAndReset(),
                fields,
                new object[] {400});

            // terminate one of A
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("A", 100, 2));
            EPAssertionUtil.AssertProps(
                env.Listener("S1").AssertOneGetNewAndReset(),
                fields,
                new object[] {100});

            // fill up A
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("A", 300, 1));
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("A", 400, 1));
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("A", 500, 1));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            // overflow
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("B", 500, 1));
            RowRecogMaxStatesEngineWide3Instance.AssertContextEnginePool(
                env,
                env.Statement("S2"),
                handler.GetAndResetContexts(),
                4,
                RowRecogMaxStatesEngineWide3Instance.GetExpectedCountMap(env, "S1", 4, "S2", 0));

            // destroy statement-1 freeing up all "A"
            env.UndeployModuleContaining("S1");

            // any number of B doesn't trigger overflow because of data window
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("B", 600, 1));
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("B", 700, 1));
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("B", 800, 1));
            env.SendEventBean(RowRecogMaxStatesEngineWide3Instance.MakeBean("B", 900, 1));
            Assert.IsTrue(handler.Contexts.IsEmpty());
        }
    }
} // end of namespace