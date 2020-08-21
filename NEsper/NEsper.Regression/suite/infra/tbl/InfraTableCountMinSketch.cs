///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableCountMinSketch
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraFrequencyAndTopk());
            execs.Add(new InfraDocSamples());
            execs.Add(new InfraNonStringType());
            execs.Add(new InfraInvalid());
            return execs;
        }

        private static void AssertOutput(
            RegressionEnvironment env,
            string frequencyList,
            string topkList)
        {
            AssertFrequencies(env, frequencyList);
            AssertTopk(env, topkList);
        }

        private static void AssertFrequencies(
            RegressionEnvironment env,
            string frequencyList)
        {
            var pairs = frequencyList.SplitCsv();
            for (var i = 0; i < pairs.Length; i++) {
                var split = pairs[i].Split('=');
                env.SendEventBean(new SupportBean_S0(0, split[0].Trim()));
                var value = env.Listener("frequency").AssertOneGetNewAndReset().Get("freq");
                Assert.AreEqual(long.Parse(split[1]), value, "failed at index" + i);
            }
        }

        private static void AssertTopk(
            RegressionEnvironment env,
            string topkList)
        {
            env.SendEventBean(new SupportBean_S1(0));
            var @event = env.Listener("topk").AssertOneGetNewAndReset();
            var arr = (CountMinSketchTopK[]) @event.Get("topk");

            var pairs = topkList.SplitCsv();
            Assert.AreEqual(pairs.Length, arr.Length, "received " + Arrays.AsList(arr));

            foreach (var pair in pairs) {
                var pairArr = pair.Split('=');
                var expectedFrequency = long.Parse(pairArr[1]);
                var expectedValue = pairArr[0].Trim();
                var foundIndex = Find(expectedFrequency, expectedValue, arr);
                Assert.IsFalse(
                    foundIndex == -1,
                    "failed to find '" +
                    expectedValue +
                    "=" +
                    expectedFrequency +
                    "' among remaining " +
                    Arrays.AsList(arr));
                arr[foundIndex] = null;
            }
        }

        private static int Find(
            long expectedFrequency,
            string expectedValue,
            CountMinSketchTopK[] arr)
        {
            for (var i = 0; i < arr.Length; i++) {
                var item = arr[i];
                if (item != null && item.Frequency == expectedFrequency && item.Value.Equals(expectedValue)) {
                    return i;
                }
            }

            return -1;
        }

        internal class InfraDocSamples : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create schema WordEvent (word string)", path);
                env.CompileDeploy("create schema EstimateWordCountEvent (word string)", path);

                env.CompileDeploy("create table WordCountTable(wordcms countMinSketch())", path);
                env.CompileDeploy(
                    "create table WordCountTable2(wordcms countMinSketch({\n" +
                    "  EpsOfTotalCount: 0.000002,\n" +
                    "  Confidence: 0.999,\n" +
                    "  Seed: 38576,\n" +
                    "  Topk: 20,\n" +
                    "  Agent: '" +
                    typeof(CountMinSketchAgentStringUTF16Forge).FullName +
                    "'" +
                    "}))",
                    path);
                env.CompileDeploy(
                    "into table WordCountTable select countMinSketchAdd(word) as wordcms from WordEvent",
                    path);
                env.CompileDeploy(
                    "select WordCountTable.wordcms.countMinSketchFrequency(word) from EstimateWordCountEvent",
                    path);
                env.CompileDeploy(
                    "select WordCountTable.wordcms.countMinSketchTopk() from pattern[every timer:interval(10 sec)]",
                    path);

                env.UndeployAll();
            }
        }

        internal class InfraNonStringType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplTable = "create table MyApproxNS(bytefreq countMinSketch({" +
                               "  EpsOfTotalCount: 0.02," +
                               "  Confidence: 0.98," +
                               "  Topk: null," +
                               "  Agent: '" +
                               typeof(MyBytesPassthruAgentForge).FullName +
                               "'" +
                               "}))";
                env.CompileDeploy(eplTable, path);

                var eplInto =
                    "into table MyApproxNS select countMinSketchAdd(Body) as bytefreq from SupportByteArrEventStringId(Id='A')";
                env.CompileDeploy(eplInto, path);

                var eplRead =
                    "@Name('s0') select MyApproxNS.bytefreq.countMinSketchFrequency(Body) as freq from SupportByteArrEventStringId(Id='B')";
                env.CompileDeploy(eplRead, path).AddListener("s0");

                env.SendEventBean(new SupportByteArrEventStringId("A", new byte[] {1, 2, 3}));

                env.Milestone(0);

                env.SendEventBean(new SupportByteArrEventStringId("B", new byte[] {0, 2, 3}));
                Assert.AreEqual(0L, env.Listener("s0").AssertOneGetNewAndReset().Get("freq"));

                env.SendEventBean(new SupportByteArrEventStringId("B", new byte[] {1, 2, 3}));
                Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("freq"));

                env.UndeployAll();
            }
        }

        internal class InfraFrequencyAndTopk : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "create table MyApproxFT(wordapprox countMinSketch({topk:3}));\n" +
                    "into table MyApproxFT select countMinSketchAdd(TheString) as wordapprox from SupportBean;\n" +
                    "@Name('frequency') select MyApproxFT.wordapprox.countMinSketchFrequency(P00) as freq from SupportBean_S0;\n" +
                    "@Name('topk') select MyApproxFT.wordapprox.countMinSketchTopk() as topk from SupportBean_S1;\n";
                env.CompileDeploy(epl, path).AddListener("frequency").AddListener("topk");

                env.SendEventBean(new SupportBean("E1", 0));
                AssertOutput(env, "E1=1", "E1=1");

                env.Milestone(0);

                AssertOutput(env, "E1=1", "E1=1");
                env.SendEventBean(new SupportBean("E2", 0));

                env.Milestone(1);

                AssertOutput(env, "E1=1,E2=1", "E1=1,E2=1");

                env.SendEventBean(new SupportBean("E2", 0));
                AssertOutput(env, "E1=1,E2=2", "E1=1,E2=2");

                env.SendEventBean(new SupportBean("E3", 0));
                AssertOutput(env, "E1=1,E2=2,E3=1", "E1=1,E2=2,E3=1");

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 0));
                AssertOutput(env, "E1=1,E2=2,E3=1,E4=1", "E1=1,E2=2,E3=1");

                env.SendEventBean(new SupportBean("E4", 0));
                AssertOutput(env, "E1=1,E2=2,E3=1,E4=2", "E1=1,E2=2,E4=2");

                // test join
                var eplJoin =
                    "@Name('join') select wordapprox.countMinSketchFrequency(S2.P20) as c0 from MyApproxFT, SupportBean_S2 S2 unidirectional";
                env.CompileDeploy(eplJoin, path).AddListener("join");

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S2(0, "E3"));
                Assert.AreEqual(1L, env.Listener("join").AssertOneGetNewAndReset().Get("c0"));
                env.UndeployModuleContaining("join");

                // test subquery
                var eplSubquery =
                    "@Name('subq') select (select wordapprox.countMinSketchFrequency(S2.P20) from MyApproxFT) as c0 from SupportBean_S2 S2";
                env.CompileDeploy(eplSubquery, path).AddListener("subq");

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S2(0, "E3"));
                Assert.AreEqual(1L, env.Listener("subq").AssertOneGetNewAndReset().Get("c0"));
                env.UndeployModuleContaining("subq");

                env.UndeployAll();
            }
        }

        internal class InfraInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table MyCMS(wordcms countMinSketch())", path);

                // invalid "countMinSketch" declarations
                //
                TryInvalidCompile(
                    env,
                    path,
                    "select countMinSketch() from SupportBean",
                    "Failed to validate select-clause expression 'countMinSketch()': Count-min-sketch aggregation function 'countMinSketch' can only be used in create-table statements [");
                TryInvalidCompile(
                    env,
                    path,
                    "create table MyTable(cms countMinSketch(5))",
                    "Failed to validate table-column expression 'countMinSketch(5)': Count-min-sketch aggregation function 'countMinSketch'  expects either no parameter or a single json parameter object [");
                TryInvalidCompile(
                    env,
                    path,
                    "create table MyTable(cms countMinSketch({xxx:3}))",
                    "Failed to validate table-column expression 'countMinSketch({\"xxx\"=3})': Unrecognized parameter 'xxx' [");
                TryInvalidCompile(
                    env,
                    path,
                    "create table MyTable(cms countMinSketch({EpsOfTotalCount:'a'}))",
                    "Failed to validate table-column expression 'countMinSketch({\"EpsOfTotalCount\"=\"a\"})': Property 'EpsOfTotalCount' expects an System.Nullable<System.Double> but receives a value of type System.String [");
                TryInvalidCompile(
                    env,
                    path,
                    "create table MyTable(cms countMinSketch({agent:'a'}))",
                    "Failed to validate table-column expression 'countMinSketch({\"agent\"=\"a\"})': Failed to instantiate agent provider: Could not load class by name 'a', please check imports [");
                TryInvalidCompile(
                    env,
                    path,
                    "create table MyTable(cms countMinSketch({agent:'System.String'}))",
                    "Failed to validate table-column expression 'countMinSketch({\"agent\"=\"System.Str...(41 chars)': Failed to instantiate agent provider: Type 'System.String' does not implement interface 'com.espertech.esper.common.client.util.CountMinSketchAgentForge' [");

                // invalid "countMinSketchAdd" declarations
                //
                TryInvalidCompile(
                    env,
                    path,
                    "select countMinSketchAdd(TheString) from SupportBean",
                    "Failed to validate select-clause expression 'countMinSketchAdd(TheString)': Count-min-sketch aggregation function 'countMinSketchAdd' can only be used with into-table");
                TryInvalidCompile(
                    env,
                    path,
                    "into table MyCMS select countMinSketchAdd() as wordcms from SupportBean",
                    "Failed to validate select-clause expression 'countMinSketchAdd()': Count-min-sketch aggregation function 'countMinSketchAdd' requires a single parameter expression");
                TryInvalidCompile(
                    env,
                    path,
                    "into table MyCMS select countMinSketchAdd(Body) as wordcms from SupportByteArrEventStringId",
                    "Incompatible aggregation function for table 'MyCMS' column 'wordcms', expecting 'countMinSketch()' and received 'countMinSketchAdd(Body)': " +
                    "Mismatching parameter return type, expected any of [System.String] but received System.Byte[] [");
                TryInvalidCompile(
                    env,
                    path,
                    "into table MyCMS select countMinSketchAdd(distinct 'abc') as wordcms from SupportByteArrEventStringId",
                    "Failed to validate select-clause expression 'countMinSketchAdd(distinct \"abc\")': Count-min-sketch aggregation function 'countMinSketchAdd' is not supported with distinct [");

                // invalid "countMinSketchFrequency" declarations
                //
                TryInvalidCompile(
                    env,
                    path,
                    "into table MyCMS select countMinSketchFrequency(TheString) as wordcms from SupportBean",
                    "Failed to validate select-clause expression 'countMinSketchFrequency(TheString)': Unknown single-row function, aggregation function or mapped or indexed property named 'countMinSketchFrequency' could not be resolved ");
                TryInvalidCompile(
                    env,
                    path,
                    "select countMinSketchFrequency() from SupportBean",
                    "Failed to validate select-clause expression 'countMinSketchFrequency()': Unknown single-row function, expression declaration, script or aggregation function named 'countMinSketchFrequency' could not be resolved");

                // invalid "countMinSketchTopk" declarations
                //
                TryInvalidCompile(
                    env,
                    path,
                    "select countMinSketchTopk() from SupportBean",
                    "Failed to validate select-clause expression 'countMinSketchTopk()': Unknown single-row function, expression declaration, script or aggregation function named 'countMinSketchTopk' could not be resolved");
                TryInvalidCompile(
                    env,
                    path,
                    "select MyCMS.wordcms.countMinSketchTopk(TheString) from SupportBean",
                    "Failed to validate select-clause expression 'MyCMS.wordcms.countMinSketchTopk(Th...(43 chars)': Count-min-sketch aggregation function 'countMinSketchTopk' requires a no parameter expressions [");

                env.UndeployAll();
            }
        }

        public class MyBytesPassthruAgentForge : CountMinSketchAgentForge
        {
            public Type[] AcceptableValueTypes => new[] {typeof(byte[])};

            public CodegenExpression CodegenMake(
                CodegenMethod parent,
                CodegenClassScope classScope)
            {
                return NewInstance(typeof(MyBytesPassthruAgent));
            }
        }

        public class MyBytesPassthruAgent : CountMinSketchAgent
        {
            public void Add(CountMinSketchAgentContextAdd ctx)
            {
                if (ctx.Value == null) {
                    return;
                }

                var value = (byte[]) ctx.Value;
                ctx.State.Add(value, 1);
            }

            public long? Estimate(CountMinSketchAgentContextEstimate ctx)
            {
                if (ctx.Value == null) {
                    return null;
                }

                var value = (byte[]) ctx.Value;
                return ctx.State.Frequency(value);
            }

            public object FromBytes(CountMinSketchAgentContextFromBytes ctx)
            {
                return ctx.Bytes;
            }
        }
    }
} // end of namespace