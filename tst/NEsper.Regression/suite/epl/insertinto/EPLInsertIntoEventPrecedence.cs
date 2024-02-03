///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoEventPrecedence
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithConstantInfraMergeInsertInto(execs);
            WithConstantInsertInto(execs);
            WithConstantOnSplit(execs);
            WithNonConstInsertIntoContainedEvent(execs);
            WithSubqueryOnSplitSODA(execs);
            WithSubqueryInsertIntoSODA(execs);
            WithSubqueryMergeSODA(execs);
            WithSubqueryOnInsertSODA(execs);
            WithConstantInsertIntoOutputRate(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEventPrecInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithConstantInsertIntoOutputRate(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEventPrecConstantInsertIntoOutputRate());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryOnInsertSODA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEventPrecSubqueryOnInsertSODA());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryMergeSODA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEventPrecSubqueryMergeSODA());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryInsertIntoSODA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEventPrecSubqueryInsertIntoSODA());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryOnSplitSODA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEventPrecSubqueryOnSplitSODA());
            return execs;
        }

        public static IList<RegressionExecution> WithNonConstInsertIntoContainedEvent(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEventPrecNonConstInsertIntoContainedEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithConstantOnSplit(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEventPrecConstantOnSplit());
            return execs;
        }

        public static IList<RegressionExecution> WithConstantInsertInto(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEventPrecConstantInsertInto());
            return execs;
        }

        public static IList<RegressionExecution> WithConstantInfraMergeInsertInto(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEventPrecConstantInfraMergeInsertInto(false));
            execs.Add(new EPLInsertIntoEventPrecConstantInfraMergeInsertInto(true));
            return execs;
        }

        public static IList<RegressionExecution> ExecutionsNoLatch()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEventPrecConstantInsertInto());
            execs.Add(new EPLInsertIntoEventPrecConstantInfraMergeInsertInto(false));
            execs.Add(new EPLInsertIntoEventPrecConstantOnSplit());
            execs.Add(new EPLInsertIntoEventPrecNonConstInsertIntoContainedEvent());
            return execs;
        }

        public class EPLInsertIntoEventPrecNonConstInsertIntoContainedEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema LvlA as " +
                    typeof(LvlA).FullName +
                    ";\n" +
                    "insert into LvlB event-precedence(precedence) select * from LvlA[b];\n" +
                    "insert into LvlC event-precedence(precedence) select * from LvlB[c];\n" +
                    "@name('s0') select Id from LvlC;\n";
                env.CompileDeploy(epl).AddListener("s0");

                // precedenced
                var c1 = new LvlC[] { new LvlC("A", 0), new LvlC("B", 1) };
                var c2 = new LvlC[] { new LvlC("C", 1), new LvlC("D", 0) };
                var a0 = new LvlA(new LvlB(10, c1), new LvlB(11, c2));
                SendAssert(env, a0, "C,B,D,A");

                // same-precedence with one raised
                var c11 = new LvlC[] { new LvlC("A", 0), new LvlC("B", 0) };
                var c12 = new LvlC[] { new LvlC("C", 1), new LvlC("D", 0) };
                var a1 = new LvlA(new LvlB(10, c11), new LvlB(10, c12));
                SendAssert(env, a1, "C,A,B,D");

                // no precedences
                var c21 = new LvlC[] { new LvlC("A", 0), new LvlC("B", 0) };
                var c22 = new LvlC[] { new LvlC("C", 0), new LvlC("D", 0) };
                var a2 = new LvlA(new LvlB(0, c21), new LvlB(0, c22));
                SendAssert(env, a2, "A,B,C,D");

                // three precedence levels
                var c31 = new LvlC[] { new LvlC("A", 2), new LvlC("B", 1), new LvlC("C", 0) };
                var c32 = new LvlC[] { new LvlC("D", 2), new LvlC("E", 0), new LvlC("F", 1) };
                var c33 = new LvlC[] { new LvlC("G", 1), new LvlC("H", 2), new LvlC("I", 0) };
                var a3 = new LvlA(new LvlB(100, c31), new LvlB(101, c32), new LvlB(103, c33));
                SendAssert(env, a3, "H,D,A,G,F,B,I,E,C");

                // two precedence levels
                var c41 = new LvlC[] { new LvlC("A", 0), new LvlC("B", 1), new LvlC("C", 0) };
                var c42 = new LvlC[] { new LvlC("D", 1), new LvlC("E", 0), new LvlC("F", 1) };
                var c43 = new LvlC[] { new LvlC("G", 1), new LvlC("H", 1), new LvlC("I", 0) };
                var c4 = new LvlA(new LvlB(103, c41), new LvlB(100, c42), new LvlB(102, c43));
                SendAssert(env, c4, "B,G,H,D,F,A,C,I,E");

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                LvlA one,
                string s)
            {
                env.SendEventBean(one);
                var split = s.Split(",");
                var expected = new object[split.Length][];
                for (var i = 0; i < split.Length; i++) {
                    expected[i] = new object[] { split[i] };
                }

                env.AssertPropsPerRowNewFlattened("s0", new string[] { "Id" }, expected);
            }
        }

        public class EPLInsertIntoEventPrecConstantOnSplit : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "on SupportBean \n" +
                          "insert into Out event-precedence(1) select 1 as Id\n" +
                          "insert into Out event-precedence(2) select 2 as Id\n" +
                          "insert into Out event-precedence(3) select 3 as Id\n" +
                          "output all;\n" +
                          "@name('s0') select * from Out ";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean());
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    new string[] { "Id" },
                    new object[][] { new object[] { 3 }, new object[] { 2 }, new object[] { 1 } });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoEventPrecConstantInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('window') create window MyWindow#keepall as (Id int);\n" +
                          "insert into MyWindow event-precedence(4) select 4 as Id from SupportBean;\n" +
                          "insert into MyWindow event-precedence(2) select 2 as Id from SupportBean;\n" +
                          "insert into MyWindow select 0 as Id from SupportBean;\n" +
                          "insert into MyWindow event-precedence(5) select 5 as Id from SupportBean;\n" +
                          "insert into MyWindow event-precedence(1) select 1 as Id from SupportBean;\n" +
                          "insert into MyWindow event-precedence(3) select 3 as Id from SupportBean;\n";
                env.CompileDeploy(epl);

                env.SendEventBean(new SupportBean());
                env.AssertPropsPerRowIterator(
                    "window",
                    new string[] { "Id" },
                    new object[][] {
                        new object[] { 5 }, new object[] { 4 }, new object[] { 3 }, new object[] { 2 },
                        new object[] { 1 }, new object[] { 0 }
                    });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoEventPrecSubqueryInsertIntoSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create schema Out (Id string)", path);
                env.CompileDeploy(
                    true,
                    "insert into Out event-precedence((select intOne from SupportBeanNumeric#lastevent)) select \"a\" as Id from SupportBean",
                    path);
                env.CompileDeploy(
                    true,
                    "insert into Out event-precedence((select intTwo from SupportBeanNumeric#lastevent)) select \"b\" as Id from SupportBean",
                    path);
                env.CompileDeploy("@name('s0') select * from Out", path).AddListener("s0");

                SendSBAssert(env, "a", "b");

                env.SendEventBean(new SupportBeanNumeric(-2, -1));
                SendSBAssert(env, "b", "a");

                env.UndeployAll();
            }

            private void SendSBAssert(
                RegressionEnvironment env,
                string first,
                string second)
            {
                env.SendEventBean(new SupportBean());
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    new string[] { "Id" },
                    new object[][] { new object[] { first }, new object[] { second } });
            }
        }

        private class EPLInsertIntoEventPrecConstantInfraMergeInsertInto : RegressionExecution
        {
            private readonly bool namedWindow;

            public EPLInsertIntoEventPrecConstantInfraMergeInsertInto(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string epl;
                if (namedWindow) {
                    epl = "create window InfraMerge#keepall as (mergeid string);\n";
                }
                else {
                    epl = "create table InfraMerge(mergeid string primary key);\n";
                }

                epl += "@name('WindowOut') create window WindowOut#keepall as (outid string);\n" +
                       "on SupportBean sb merge InfraMerge mw where sb.TheString = mw.mergeid\n" +
                       "when not matched\n" +
                       "then insert into WindowOut event-precedence(0) select 'a' as outid\n" +
                       "then insert into WindowOut select 'b' as outid\n" +
                       "then insert into WindowOut event-precedence(1) select 'c' as outid\n" +
                       ";\n";
                env.CompileDeploy(epl);

                env.SendEventBean(new SupportBean());
                env.AssertPropsPerRowIterator(
                    "WindowOut",
                    new string[] { "outid" },
                    new object[][] { new object[] { "c" }, new object[] { "a" }, new object[] { "b" } });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        private class EPLInsertIntoEventPrecSubqueryOnSplitSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create schema Out (Id string)", path);
                var epl = "on SupportBean " +
                          "insert into Out event-precedence((select intOne from SupportBeanNumeric#lastevent)) select \"a\" as Id " +
                          "insert into Out event-precedence((select intTwo from SupportBeanNumeric#lastevent)) select \"b\" as Id " +
                          "output all";
                env.CompileDeploy(true, epl, path);
                env.CompileDeploy("@name('s0') select * from Out", path).AddListener("s0");

                SendSBAssert(env, "a", "b");

                env.SendEventBean(new SupportBeanNumeric(1, 2));
                SendSBAssert(env, "b", "a");

                env.SendEventBean(new SupportBeanNumeric(2, 1));
                SendSBAssert(env, "a", "b");

                env.UndeployAll();
            }

            private void SendSBAssert(
                RegressionEnvironment env,
                string first,
                string second)
            {
                env.SendEventBean(new SupportBean());
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    new string[] { "Id" },
                    new object[][] { new object[] { first }, new object[] { second } });
            }
        }

        private class EPLInsertIntoEventPrecSubqueryMergeSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create window MyWindow#keepall as (Id string);\n" +
                    "@public create schema Out (Id string);\n",
                    path);
                var epl = "on SupportBean merge MyWindow where TheString=Id " +
                          "when not matched " +
                          "then insert into Out event-precedence((select intOne from SupportBeanNumeric#lastevent)) select \"a\" as Id " +
                          "then insert into Out event-precedence((select intTwo from SupportBeanNumeric#lastevent)) select \"b\" as Id";
                env.CompileDeploy(true, epl, path);
                env.CompileDeploy("@name('s0') select * from Out", path).AddListener("s0");

                SendSBAssert(env, "a", "b");

                env.SendEventBean(new SupportBeanNumeric(1, 2));
                SendSBAssert(env, "b", "a");

                env.SendEventBean(new SupportBeanNumeric(2, 1));
                SendSBAssert(env, "a", "b");

                env.UndeployAll();
            }

            private void SendSBAssert(
                RegressionEnvironment env,
                string first,
                string second)
            {
                env.SendEventBean(new SupportBean());
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    new string[] { "Id" },
                    new object[][] { new object[] { first }, new object[] { second } });
            }
        }

        private class EPLInsertIntoEventPrecSubqueryOnInsertSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create schema Out (Id string);\n" +
                    "@public create window MyWindow#keepall as (value string);\n",
                    path);
                env.CompileExecuteFAF("insert into MyWindow select 'x' as value", path);

                var eplOne =
                    "on SupportBean insert into Out event-precedence((select intOne from SupportBeanNumeric#lastevent)) select \"a\" as Id from MyWindow";
                env.CompileDeploy(true, eplOne, path);

                var eplTwo =
                    "on SupportBean insert into Out event-precedence((select intTwo from SupportBeanNumeric#lastevent)) select \"b\" as Id from MyWindow;\n" +
                    "@name('s0') select * from Out;\n";
                env.CompileDeploy(false, eplTwo, path).AddListener("s0");

                SendSBAssert(env, "a", "b");

                env.SendEventBean(new SupportBeanNumeric(1, 2));
                SendSBAssert(env, "b", "a");

                env.SendEventBean(new SupportBeanNumeric(2, 1));
                SendSBAssert(env, "a", "b");

                env.UndeployAll();
            }

            private void SendSBAssert(
                RegressionEnvironment env,
                string first,
                string second)
            {
                env.SendEventBean(new SupportBean());
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    new string[] { "Id" },
                    new object[][] { new object[] { first }, new object[] { second } });
            }
        }

        private class EPLInsertIntoEventPrecConstantInsertIntoOutputRate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create schema Out(Id int);\n" +
                    "insert into Out event-precedence(1) select 1 + IntPrimitive * 10 as Id from SupportBean output every 2 events;\n" +
                    "insert into Out event-precedence(2) select 2 + IntPrimitive * 10 as Id from SupportBean output every 2 events;\n" +
                    "insert into Out event-precedence(" +
                    typeof(EPLInsertIntoEventPrecedence).FullName +
                    ".computeEventPrecedence(3, *)) select 3 + IntPrimitive * 10 as Id from SupportBean output every 2 events;\n" +
                    "@name('s0') select * from Out;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    new string[] { "Id" },
                    new object[][] {
                        new object[] { 13 }, new object[] { 23 }, new object[] { 12 }, new object[] { 22 },
                        new object[] { 11 }, new object[] { 21 }
                    });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoEventPrecInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create window MyWindow#keepall as (Id string);\n" +
                    "@public create table MyTable(Id string primary key);\n",
                    path);

                env.TryInvalidCompile(
                    path,
                    "insert into Out event-precedence('a') select * from SupportBean",
                    "Event-precedence expected an expression returning an integer value but the expression '\"a\"' returns String");

                env.TryInvalidCompile(
                    path,
                    "insert into Out event-precedence(IntPrimitive) select TheString from SupportBean",
                    "Failed to validate event-precedence considering only the output event type 'Out': Failed to validate event-precedence clause expression 'IntPrimitive': Property named 'IntPrimitive' is not valid in any stream (NOTE: this validation only considers the result event itself and not incoming streams)");

                env.TryInvalidCompile(
                    path,
                    "on SupportBean insert into Out event-precedence(null) select 'a' as Id",
                    "Event-precedence expected an expression returning an integer value but the expression 'null' returns null");

                env.TryInvalidCompile(
                    path,
                    "on SupportBean merge MyWindow when not matched then insert into Out event-precedence(cast(1, short)) select 'a' as Id",
                    "Validation failed in when-not-matched (clause 1): Event-precedence expected an expression returning an integer value but the expression 'cast(1,short)' returns Short");

                env.TryInvalidCompileFAF(
                    path,
                    "insert into MyWindow(Id) event-precedence(10) values ('a')",
                    "Fire-and-forget insert-queries do not allow event-precedence");

                env.TryInvalidCompile(
                    path,
                    "insert into MyTable event-precedence(1) select 'a' as Id from SupportBean",
                    "Event-precedence is not allowed when inserting into a table");

                env.TryInvalidCompile(
                    path,
                    "on SupportBean merge MyWindow insert event-precedence(1) select 'a' as Id",
                    "Incorrect syntax near 'event-precedence' (a reserved keyword)");

                env.UndeployAll();
            }
        }

        /// <summary>
        /// Sample Performance Test
        /// </summary>
        /*
        public static class EPLInsertIntoPerformance implements RegressionExecution {
            public void run(RegressionEnvironment env) {
                String epl =
                        "insert into MyStream1 select * from SupportBean;\n" +
                        "insert into MyStream2 select * from MyStream1;\n" +
                        "insert into MyStream3 select * from MyStream2;\n" +
                        "insert into MyStream4 select * from MyStream3;\n" +
                        "insert into MyStream5 select * from MyStream4;\n" +
                        "insert into MyStream6 select * from MyStream5;\n" +
                        "insert into MyStream7 select * from MyStream6;\n" +
                        "insert into MyStream8 select * from MyStream7;\n" +
                                "@name('s0') select count(*) as cnt from MyStream8;\n";
                env.compileDeploy(epl);
                int count = 10000000;

                long start = System.currentTimeMillis();
                for (int i = 0; i < count; i++) {
                    env.sendEventBean(new SupportBean());
                }
                long end = System.currentTimeMillis();
                env.assertPropsPerRowIterator("s0", new String[] {"cnt"}, new Object[][] {{count * 1L}});
                System.out.println((end-start) / 1000d);
            }
        }
         */
        public static int ComputeEventPrecedence(
            int value,
            object param)
        {
            ClassicAssert.IsInstanceOf<IDictionary<string, object>>(param);
            return value;
        }

        public class LvlA
        {
            private readonly LvlB[] b;

            public LvlA(params LvlB[] b)
            {
                this.b = b;
            }

            public LvlB[] B => b;
        }

        public class LvlB
        {
            private readonly int precedence;
            private readonly LvlC[] c;

            public LvlB(
                int precedence,
                LvlC[] c)
            {
                this.precedence = precedence;
                this.c = c;
            }

            public int Precedence => precedence;

            public LvlC[] C => c;
        }

        public class LvlC
        {
            private readonly string id;
            private readonly int precedence;

            public LvlC(
                string id,
                int precedence)
            {
                this.id = id;
                this.precedence = precedence;
            }

            public string Id => id;

            public int Precedence => precedence;
        }
    }
} // end of namespace