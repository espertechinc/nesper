///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;


namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterJoinVarA3Stream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithMapLeftJoinUnsortedProps(execs);
            WithLeftJoin2SidesMulticolumn(execs);
            WithLeftOuterJoinRootS0OM(execs);
            WithLeftOuterJoinRootS0Compiled(execs);
            WithLeftOuterJoinRootS0(execs);
            WithRightOuterJoinS2RootS2(execs);
            WithRightOuterJoinS1RootS1(execs);
            WithInvalidMulticolumn(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidMulticolumn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinInvalidMulticolumn());
            return execs;
        }

        public static IList<RegressionExecution> WithRightOuterJoinS1RootS1(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRightOuterJoinS1RootS1());
            return execs;
        }

        public static IList<RegressionExecution> WithRightOuterJoinS2RootS2(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRightOuterJoinS2RootS2());
            return execs;
        }

        public static IList<RegressionExecution> WithLeftOuterJoinRootS0(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftOuterJoinRootS0());
            return execs;
        }

        public static IList<RegressionExecution> WithLeftOuterJoinRootS0Compiled(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftOuterJoinRootS0Compiled());
            return execs;
        }

        public static IList<RegressionExecution> WithLeftOuterJoinRootS0OM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftOuterJoinRootS0OM());
            return execs;
        }

        public static IList<RegressionExecution> WithLeftJoin2SidesMulticolumn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftJoin2SidesMulticolumn());
            return execs;
        }

        public static IList<RegressionExecution> WithMapLeftJoinUnsortedProps(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinMapLeftJoinUnsortedProps());
            return execs;
        }

        private class EPLJoinMapLeftJoinUnsortedProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select t1.col1, t1.col2, t2.col1, t2.col2, t3.col1, t3.col2 from Type1#keepall as t1" +
                    " left outer join Type2#keepall as t2" +
                    " on t1.col2 = t2.col2 and t1.col1 = t2.col1" +
                    " left outer join Type3#keepall as t3" +
                    " on t1.col1 = t3.col1";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                var fields = new string[] { "t1.col1", "t1.col2", "t2.col1", "t2.col2", "t3.col1", "t3.col2" };

                SendMapEvent(env, "Type2", "a1", "b1");
                env.AssertListenerNotInvoked("s0");

                SendMapEvent(env, "Type1", "b1", "a1");
                env.AssertPropsNew("s0", fields, new object[] { "b1", "a1", null, null, null, null });

                SendMapEvent(env, "Type1", "a1", "a1");
                env.AssertPropsNew("s0", fields, new object[] { "a1", "a1", null, null, null, null });

                SendMapEvent(env, "Type1", "b1", "b1");
                env.AssertPropsNew("s0", fields, new object[] { "b1", "b1", null, null, null, null });

                SendMapEvent(env, "Type1", "a1", "b1");
                env.AssertPropsNew("s0", fields, new object[] { "a1", "b1", "a1", "b1", null, null });

                SendMapEvent(env, "Type3", "c1", "b1");
                env.AssertListenerNotInvoked("s0");

                SendMapEvent(env, "Type1", "d1", "b1");
                env.AssertPropsNew("s0", fields, new object[] { "d1", "b1", null, null, null, null });

                SendMapEvent(env, "Type3", "d1", "bx");
                env.AssertPropsNew("s0", fields, new object[] { "d1", "b1", null, null, "d1", "bx" });

                env.AssertListenerNotInvoked("s0");
                env.UndeployAll();
            }
        }

        private class EPLJoinLeftJoin2SidesMulticolumn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "s0.Id, s0.P00, s0.P01, s1.Id, s1.P10, s1.P11, s2.Id, s2.P20, s2.P21".SplitCsv();

                var epl = "@name('s0') select * from " +
                          "SupportBean_S0#length(1000) as s0 " +
                          " left outer join SupportBean_S1#length(1000) as s1 on s0.P00 = s1.P10 and s0.P01 = s1.P11" +
                          " left outer join SupportBean_S2#length(1000) as s2 on s0.P00 = s2.P20 and s0.P01 = s2.P21";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean_S1(10, "A_1", "B_1"));
                env.SendEventBean(new SupportBean_S1(11, "A_2", "B_1"));
                env.SendEventBean(new SupportBean_S1(12, "A_1", "B_2"));
                env.SendEventBean(new SupportBean_S1(13, "A_2", "B_2"));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S2(20, "A_1", "B_1"));
                env.SendEventBean(new SupportBean_S2(21, "A_2", "B_1"));
                env.SendEventBean(new SupportBean_S2(22, "A_1", "B_2"));
                env.SendEventBean(new SupportBean_S2(23, "A_2", "B_2"));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S0(1, "A_3", "B_3"));
                env.AssertPropsNew("s0", fields, new object[] { 1, "A_3", "B_3", null, null, null, null, null, null });

                env.SendEventBean(new SupportBean_S0(2, "A_1", "B_3"));
                env.AssertPropsNew("s0", fields, new object[] { 2, "A_1", "B_3", null, null, null, null, null, null });

                env.SendEventBean(new SupportBean_S0(3, "A_3", "B_1"));
                env.AssertPropsNew("s0", fields, new object[] { 3, "A_3", "B_1", null, null, null, null, null, null });

                env.SendEventBean(new SupportBean_S0(4, "A_2", "B_2"));
                env.AssertPropsNew("s0", fields, new object[] { 4, "A_2", "B_2", 13, "A_2", "B_2", 23, "A_2", "B_2" });

                env.SendEventBean(new SupportBean_S0(5, "A_2", "B_1"));
                env.AssertPropsNew("s0", fields, new object[] { 5, "A_2", "B_1", 11, "A_2", "B_1", 21, "A_2", "B_1" });

                env.UndeployAll();
            }
        }

        private class EPLJoinLeftOuterJoinRootS0OM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard();
                var fromClause = FromClause.Create(
                    FilterStream.Create("SupportBean_S0", "s0").AddView("keepall"),
                    FilterStream.Create("SupportBean_S1", "s1").AddView("keepall"),
                    FilterStream.Create("SupportBean_S2", "s2").AddView("keepall"));
                fromClause.Add(OuterJoinQualifier.Create("s0.P00", OuterJoinType.LEFT, "s1.P10"));
                fromClause.Add(OuterJoinQualifier.Create("s0.P00", OuterJoinType.LEFT, "s2.P20"));
                model.FromClause = fromClause;
                model = env.CopyMayFail(model);

                ClassicAssert.AreEqual(
                    "select * from SupportBean_S0#keepall as s0 left outer join SupportBean_S1#keepall as s1 on s0.P00 = s1.P10 left outer join SupportBean_S2#keepall as s2 on s0.P00 = s2.P20",
                    model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinLeftOuterJoinRootS0Compiled : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from " +
                          "SupportBean_S0#length(1000) as s0 " +
                          "left outer join SupportBean_S1#length(1000) as s1 on s0.P00 = s1.P10 " +
                          "left outer join SupportBean_S2#length(1000) as s2 on s0.P00 = s2.P20";
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinLeftOuterJoinRootS0 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S0#length(1000) as s0 " +
                          " left outer join SupportBean_S1#length(1000) as s1 on s0.P00 = s1.P10 " +
                          " left outer join SupportBean_S2#length(1000) as s2 on s0.P00 = s2.P20 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinRightOuterJoinS2RootS2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query: right other join is eliminated/translated
                /// s0
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S2#length(1000) as s2 " +
                          " right outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.P00 = s2.P20 " +
                          " left outer join SupportBean_S1#length(1000) as s1 on s0.P00 = s1.P10 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinRightOuterJoinS1RootS1 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query: right other join is eliminated/translated
                /// s0
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S1#length(1000) as s1 " +
                          " right outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.P00 = s1.P10 " +
                          " left outer join SupportBean_S2#length(1000) as s2 on s0.P00 = s2.P20 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinInvalidMulticolumn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "@name('s0') select * from " +
                      "SupportBean_S0#length(1000) as s0 " +
                      " left outer join SupportBean_S1#length(1000) as s1 on s0.P00 = s1.P10 and s0.P01 = s1.P11" +
                      " left outer join SupportBean_S2#length(1000) as s2 on s0.P00 = s2.P20 and s1.P11 = s2.P21";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate outer-join expression: Outer join ON-clause columns must refer to properties of the same joined streams when using multiple columns in the on-clause");

                epl = "@name('s0') select * from " +
                      "SupportBean_S0#length(1000) as s0 " +
                      " left outer join SupportBean_S1#length(1000) as s1 on s0.P00 = s1.P10 and s0.P01 = s1.P11" +
                      " left outer join SupportBean_S2#length(1000) as s2 on s2.P20 = s0.P00 and s2.P20 = s1.P11";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate outer-join expression: Outer join ON-clause columns must refer to properties of the same joined streams when using multiple columns in the on-clause [");
            }
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            // Test s0 outer join to 2 streams, 2 results for each (cartesian product)
            //
            var s1Events = SupportBean_S1.MakeS1("A", new string[] { "A-s1-1", "A-s1-2" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            var s2Events = SupportBean_S2.MakeS2("A", new string[] { "A-s2-1", "A-s2-2" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            var s0Events = SupportBean_S0.MakeS0("A", new string[] { "A-s0-1" });
            SendEvent(env, s0Events);
            var expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[1], s2Events[0] },
                new object[] { s0Events[0], s1Events[0], s2Events[1] },
                new object[] { s0Events[0], s1Events[1], s2Events[1] },
            };
            AssertListenerUnd(env, expected);

            // Test s0 outer join to s1 and s2, no results for each s1 and s2
            //
            s0Events = SupportBean_S0.MakeS0("B", new string[] { "B-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, null } });

            s0Events = SupportBean_S0.MakeS0("B", new string[] { "B-s0-2" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, null } });

            // Test s0 outer join to s1 and s2, one row for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("C", new string[] { "C-s1-1" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("C", new string[] { "C-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], s1Events[0], null } });

            // Test s0 outer join to s1 and s2, two rows for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("D", new string[] { "D-s1-1", "D-s1-2" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("D", new string[] { "D-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], null },
                    new object[] { s0Events[0], s1Events[1], null }
                });

            // Test s0 outer join to s1 and s2, one row for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("E", new string[] { "E-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("E", new string[] { "E-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, s2Events[0] } });

            // Test s0 outer join to s1 and s2, two rows for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("F", new string[] { "F-s2-1", "F-s2-2" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("F", new string[] { "F-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], null, s2Events[0] },
                    new object[] { s0Events[0], null, s2Events[1] }
                });

            // Test s0 outer join to s1 and s2, one row for s1 and two rows s2
            //
            s1Events = SupportBean_S1.MakeS1("G", new string[] { "G-s1-1" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s2Events = SupportBean_S2.MakeS2("G", new string[] { "G-s2-1", "G-s2-2" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("G", new string[] { "G-s0-2" });
            SendEvent(env, s0Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[0], s2Events[1] },
            };
            AssertListenerUnd(env, expected);

            // Test s0 outer join to s1 and s2, one row for s2 and two rows s1
            //
            s1Events = SupportBean_S1.MakeS1("H", new string[] { "H-s1-1", "H-s1-2" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s2Events = SupportBean_S2.MakeS2("H", new string[] { "H-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("H", new string[] { "H-s0-2" });
            SendEvent(env, s0Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[1], s2Events[0] },
            };
            AssertListenerUnd(env, expected);

            // Test s0 outer join to s1 and s2, one row for each s1 and s2
            //
            s1Events = SupportBean_S1.MakeS1("I", new string[] { "I-s1-1" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s2Events = SupportBean_S2.MakeS2("I", new string[] { "I-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("I", new string[] { "I-s0-2" });
            SendEvent(env, s0Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
            };
            AssertListenerUnd(env, expected);

            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("Q", new string[] { "Q-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, null } });

            s2Events = SupportBean_S2.MakeS2("Q", new string[] { "Q-s2-1", "Q-s2-2" });
            SendEvent(env, s2Events[0]);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, s2Events[0] } });
            SendEvent(env, s2Events[1]);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, s2Events[1] } });

            s1Events = SupportBean_S1.MakeS1("Q", new string[] { "Q-s1-1" });
            SendEvent(env, s1Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[0], s2Events[1] },
            };
            AssertListenerUnd(env, expected);

            // Test s1 inner join to s0 and outer to s2:  s0 with 0 rows, s2 with 2 rows
            //
            s2Events = SupportBean_S2.MakeS2("R", new string[] { "R-s2-1", "R-s2-2" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("R", new string[] { "R-s1-1" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("S", new string[] { "S-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, null } });

            s1Events = SupportBean_S1.MakeS1("S", new string[] { "S-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], s1Events[0], null } });

            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("T", new string[] { "T-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, null } });

            s2Events = SupportBean_S2.MakeS2("T", new string[] { "T-s2-1" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("T", new string[] { "T-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], s1Events[0], s2Events[0] } });

            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("U", new string[] { "U-s0-1", "U-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("U", new string[] { "U-s1-1" });
            SendEvent(env, s1Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], null },
                new object[] { s0Events[1], s1Events[0], null },
            };
            AssertListenerUnd(env, expected);

            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("V", new string[] { "V-s0-1", "V-s0-1" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("V", new string[] { "V-s2-1" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("V", new string[] { "V-s1-1" });
            SendEvent(env, s1Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[1], s1Events[0], s2Events[0] },
            };
            AssertListenerUnd(env, expected);

            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("W", new string[] { "W-s0-1", "W-s0-2" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("W", new string[] { "W-s2-1", "W-s2-2" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("W", new string[] { "W-s1-1" });
            SendEvent(env, s1Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[1], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[0], s2Events[1] },
                new object[] { s0Events[1], s1Events[0], s2Events[1] },
            };
            AssertListenerUnd(env, expected);

            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("J", new string[] { "J-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("J", new string[] { "J-s1-1", "J-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("J", new string[] { "J-s2-1" });
            SendEvent(env, s2Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[1], s2Events[0] },
            };
            AssertListenerUnd(env, expected);

            // Test s2 inner join to s0 and outer to s1:  s0 with 0 rows, s1 with 2 rows
            //
            s1Events = SupportBean_S1.MakeS1("K", new string[] { "K-s1-1", "K-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("K", new string[] { "K-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("L", new string[] { "L-s0-1" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("L", new string[] { "L-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, s2Events[0] } });

            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("M", new string[] { "M-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("M", new string[] { "M-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("M", new string[] { "M-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], s1Events[0], s2Events[0] } });

            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("N", new string[] { "N-s0-1", "N-s0-1" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("N", new string[] { "N-s2-1" });
            SendEvent(env, s2Events);
            expected = new object[][] {
                new object[] { s0Events[0], null, s2Events[0] },
                new object[] { s0Events[1], null, s2Events[0] },
            };
            AssertListenerUnd(env, expected);

            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("O", new string[] { "O-s0-1", "O-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("O", new string[] { "O-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("O", new string[] { "O-s2-1" });
            SendEvent(env, s2Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[1], s1Events[0], s2Events[0] },
            };
            AssertListenerUnd(env, expected);

            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("P", new string[] { "P-s0-1", "P-s0-2" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("P", new string[] { "P-s1-1", "P-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("P", new string[] { "P-s2-1" });
            SendEvent(env, s2Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[1], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[1], s2Events[0] },
                new object[] { s0Events[1], s1Events[1], s2Events[0] },
            };
            AssertListenerUnd(env, expected);

            env.UndeployAll();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }

        private static void SendEventsAndReset(
            RegressionEnvironment env,
            object[] events)
        {
            SendEvent(env, events);
            env.ListenerReset("s0");
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object[] events)
        {
            for (var i = 0; i < events.Length; i++) {
                env.SendEventBean(events[i]);
            }
        }

        private static void SendMapEvent(
            RegressionEnvironment env,
            string type,
            string col1,
            string col2)
        {
            IDictionary<string, object> mapEvent = new Dictionary<string, object>();
            mapEvent.Put("col1", col1);
            mapEvent.Put("col2", col2);
            env.SendEventMap(mapEvent, type);
        }

        private static void AssertListenerUnd(
            RegressionEnvironment env,
            object[][] expected)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var und = ArrayHandlingUtil.GetUnderlyingEvents(
                        listener.GetAndResetLastNewData(),
                        new string[] { "s0", "s1", "s2" });
                    EPAssertionUtil.AssertSameAnyOrder(expected, und);
                });
        }
    }
} // end of namespace