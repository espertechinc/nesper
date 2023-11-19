///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileLargeWConfig
    {
        public static List<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithTableAggregationReset(execs);
            WithTableAggregationEnterLeaveMethod(execs);
            WithTableAggregationEnterLeaveAccessAgg(execs);
            WithAggregation(execs);
            WithAggregationAccess(execs);
            WithSubstitutionParams(execs);
            WithSubstitutionParamsFAF(execs);
            WithSelectCol(execs);
            WithCreateSchemaAndInsert(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchemaAndInsert(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileLargeCreateSchemaAndInsert(EventRepresentationChoice.MAP, 5000, false));
            execs.Add(new ClientCompileLargeCreateSchemaAndInsert(EventRepresentationChoice.MAP, 5000, true));
            execs.Add(new ClientCompileLargeCreateSchemaAndInsert(EventRepresentationChoice.OBJECTARRAY, 5000, false));
            return execs;
        }

        public static IList<RegressionExecution> WithSelectCol(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileLargeSelectCol(EventRepresentationChoice.MAP, 5000));
            execs.Add(new ClientCompileLargeSelectCol(EventRepresentationChoice.OBJECTARRAY, 5000));
            return execs;
        }

        public static IList<RegressionExecution> WithSubstitutionParamsFAF(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileLargeSubstitutionParamsFAF(5000));
            return execs;
        }

        public static IList<RegressionExecution> WithSubstitutionParams(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileLargeSubstitutionParams(5000));
            return execs;
        }

        public static IList<RegressionExecution> WithAggregationAccess(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileLargeAggregationAccess(1000));
            return execs;
        }

        public static IList<RegressionExecution> WithAggregation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileLargeAggregation(5000));
            return execs;
        }

        public static IList<RegressionExecution> WithTableAggregationEnterLeaveAccessAgg(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileLargeTableAggregationEnterLeaveAccessAgg(1000));
            return execs;
        }

        public static IList<RegressionExecution> WithTableAggregationEnterLeaveMethod(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileLargeTableAggregationEnterLeaveMethod(1000));
            return execs;
        }

        public static IList<RegressionExecution> WithTableAggregationReset(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileLargeTableAggregationReset(1000));
            return execs;
        }

        public class ClientCompileLargeCreateSchemaAndInsert : RegressionExecution
        {
            private readonly EventRepresentationChoice representation;
            private readonly int numColumns;
            private readonly bool widening;

            public ClientCompileLargeCreateSchemaAndInsert(
                EventRepresentationChoice representation,
                int numColumns,
                bool widening)
            {
                this.representation = representation;
                this.numColumns = numColumns;
                this.widening = widening;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.AdvanceTime(1000000);

                var eplSchema = new StringWriter();
                eplSchema.Write("@name('schema') @public @buseventtype create ");
                eplSchema.Write(representation.GetName());
                eplSchema.Write(" schema MyEvent (");

                var delimiter = "";
                for (var i = 0; i < numColumns; i++) {
                    // create-schema goes back from pN-1 to p0
                    eplSchema.Write(delimiter);
                    eplSchema.Write("p");
                    eplSchema.Write(numColumns - i - 1);
                    eplSchema.Write(" long");
                    delimiter = ",";
                }

                eplSchema.Write(");\n");
                env.CompileDeploy(eplSchema.ToString(), path);

                env.AssertStatement(
                    "schema",
                    statement => {
                        var eventType = statement.EventType;
                        for (var i = 0; i < numColumns; i++) {
                            Assert.AreEqual(typeof(long), eventType.GetPropertyType("p" + i));
                        }
                    });

                var eplInsert = new StringWriter();
                eplInsert.Write("insert into MyEvent select ");
                delimiter = "";
                var adder = widening ? "1000000" : "current_timestamp()";
                for (var i = 0; i < numColumns; i++) {
                    eplInsert.Write(delimiter);
                    eplInsert.Write(i.ToString());
                    eplInsert.Write("+");
                    eplInsert.Write(adder);
                    eplInsert.Write(" as p");
                    eplInsert.Write(i.ToString());

                    delimiter = ",";
                }

                eplInsert.Write(" from SupportBean;\n");
                env.CompileDeploy(eplInsert.ToString(), path);

                env.CompileDeploy("@name('s0') select * from MyEvent", path).AddListener("s0");
                env.SendEventBean(new SupportBean());
                env.AssertEventNew(
                    "s0",
                    @event => {
                        for (var i = 0; i < numColumns; i++) {
                            Assert.AreEqual(i + 1000000L, @event.Get("p" + i));
                        }
                    });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "representation=" +
                       representation +
                       ", numColumns=" +
                       numColumns +
                       ", widening=" +
                       widening +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }

        public class ClientCompileLargeSubstitutionParams : RegressionExecution
        {
            private readonly int numColumns;

            public ClientCompileLargeSubstitutionParams(int numColumns)
            {
                this.numColumns = numColumns;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = new StringWriter();
                epl.Write("@name('s0') select ");
                var delimiter = "";
                for (var i = 0; i < numColumns; i++) {
                    epl.Write(delimiter);
                    epl.Write("?:p");
                    epl.Write(i.ToString());
                    epl.Write(":string as c");
                    epl.Write(i.ToString());
                    delimiter = ",";
                }

                epl.Write(" from SupportBean;\n");
                var compiled = env.Compile(epl.ToString());

                var options = new DeploymentOptions();
                options.WithStatementSubstitutionParameter(
                    ctx => {
                        for (var i = 0; i < numColumns; i++) {
                            ctx.SetObject("p" + i, "v" + i);
                        }
                    });
                env.Deploy(compiled, options).AddListener("s0");

                env.SendEventBean(new SupportBean());
                env.AssertEventNew(
                    "s0",
                    @event => {
                        for (var i = 0; i < numColumns; i++) {
                            Assert.AreEqual("v" + i, @event.Get("c" + i));
                        }
                    });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }

        public class ClientCompileLargeSubstitutionParamsFAF : RegressionExecution
        {
            private readonly int numColumns;

            public ClientCompileLargeSubstitutionParamsFAF(int numColumns)
            {
                this.numColumns = numColumns;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplNamedWindow = "@public create window MyWindow#lastevent (p0 string);\n";
                env.CompileDeploy(eplNamedWindow, path);
                env.CompileExecuteFAF("insert into MyWindow select 'x' as p0", path);

                var eplFAF = new StringWriter();
                eplFAF.Write("select ");
                var delimiter = "";
                for (var i = 0; i < numColumns; i++) {
                    eplFAF.Write(delimiter);
                    eplFAF.Write("p0 || ?:p");
                    eplFAF.Write(i.ToString());
                    eplFAF.Write(":string as c");
                    eplFAF.Write(i.ToString());
                    delimiter = ",";
                }

                eplFAF.Write(" from MyWindow");

                var compiled = env.CompileFAF(eplFAF.ToString(), path);
                var prepared =
                    env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);
                for (var i = 0; i < numColumns; i++) {
                    prepared.SetObject("p" + i, "v" + i);
                }

                var result = env.Runtime.FireAndForgetService.ExecuteQuery(prepared);
                var @event = result.Array[0];

                for (var i = 0; i < numColumns; i++) {
                    Assert.AreEqual("xv" + i, @event.Get("c" + i));
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }

        public class ClientCompileLargeAggregationAccess : RegressionExecution
        {
            private readonly int numColumns;

            public ClientCompileLargeAggregationAccess(int numColumns)
            {
                this.numColumns = numColumns;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = new StringWriter();
                epl.Write("@name('s0') select ");
                var delimiter = "";
                for (var i = 0; i < numColumns; i++) {
                    epl.Write(delimiter);
                    epl.Write("sorted(IntPrimitive + ");
                    epl.Write(i.ToString());
                    epl.Write(").firstEvent() as c");
                    epl.Write(i.ToString());
                    delimiter = ",";
                    epl.Write(delimiter);
                    epl.Write("sorted(IntPrimitive + ");
                    epl.Write(i.ToString());
                    epl.Write(").selectFrom(v => v) as d");
                    epl.Write(i.ToString());
                }

                epl.Write(" from SupportBean#keepall");
                env.CompileDeploy(epl.ToString()).AddListener("s0");

                var sbOne = new SupportBean("E1", 10);
                env.SendEventBean(sbOne);
                env.AssertEventNew(
                    "s0",
                    @event => {
                        for (var i = 0; i < numColumns; i++) {
                            Assert.AreEqual(sbOne, @event.Get("c" + i));
                        }
                    });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }

        public class ClientCompileLargeAggregation : RegressionExecution
        {
            private readonly int numColumns;

            public ClientCompileLargeAggregation(int numColumns)
            {
                this.numColumns = numColumns;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = new StringWriter();
                epl.Write("@name('s0') select ");
                var delimiter = "";
                for (var i = 0; i < numColumns; i++) {
                    epl.Write(delimiter);
                    epl.Write("sum(IntPrimitive + ");
                    epl.Write(i.ToString());
                    epl.Write(") as c");
                    epl.Write(i.ToString());
                    delimiter = ",";
                }

                epl.Write(" from SupportBean#lastevent");
                env.CompileDeploy(epl.ToString()).AddListener("s0");

                SendBeanAssert(env, 10);
                SendBeanAssert(env, 50);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }

            private void SendBeanAssert(
                RegressionEnvironment env,
                int intPrimitive)
            {
                env.SendEventBean(new SupportBean("x", intPrimitive));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        for (var i = 0; i < numColumns; i++) {
                            Assert.IsTrue(@event.EventType.IsProperty("c" + i));
                            Assert.AreEqual(intPrimitive + i, @event.Get("c" + i));
                        }
                    });
            }
        }

        public class ClientCompileLargeTableAggregationEnterLeaveAccessAgg : RegressionExecution
        {
            private readonly int numColumns;

            public ClientCompileLargeTableAggregationEnterLeaveAccessAgg(int numColumns)
            {
                this.numColumns = numColumns;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = SetupTable(
                    numColumns,
                    "SupportBean#length(1)",
                    "window(*) @type(SupportBean)",
                    (
                        writer,
                        index) => writer.Write("window(*)"));
                env.CompileDeploy(epl.ToString());

                var sbOne = new SupportBean("E0", 0);
                env.SendEventBean(sbOne);
                AssertTableWindow(env, numColumns, sbOne);

                var sbTwo = new SupportBean("E1", 1);
                env.SendEventBean(sbTwo);
                AssertTableWindow(env, numColumns, sbTwo);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }

        public class ClientCompileLargeTableAggregationEnterLeaveMethod : RegressionExecution
        {
            private readonly int numColumns;

            public ClientCompileLargeTableAggregationEnterLeaveMethod(int numColumns)
            {
                this.numColumns = numColumns;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = SetupTable(
                    numColumns,
                    "SupportBean#length(1)",
                    "sum(int)",
                    (
                        writer,
                        index) => {
                        writer.Write("sum(IntPrimitive + ");
                        writer.Write(index.ToString());
                        writer.Write(")");
                    });
                env.CompileDeploy(epl.ToString());

                env.SendEventBean(new SupportBean("E0", 20));
                AssertTableSum(env, numColumns, 20);

                env.SendEventBean(new SupportBean("E1", 30));
                AssertTableSum(env, numColumns, 30);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }

        public class ClientCompileLargeTableAggregationReset : RegressionExecution
        {
            private readonly int numColumns;

            public ClientCompileLargeTableAggregationReset(int numColumns)
            {
                this.numColumns = numColumns;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = SetupTable(
                    numColumns,
                    "SupportBean",
                    "sum(int)",
                    (
                        writer,
                        index) => {
                        writer.Write("sum(IntPrimitive + ");
                        writer.Write(index.ToString());
                        writer.Write(")");
                    });

                epl.Write("on SupportBean_S0 merge MyTable when matched then update set ");
                var delimiter = "";
                for (var i = 0; i < numColumns; i++) {
                    epl.Write(delimiter);
                    epl.Write("c");
                    epl.Write(i.ToString());
                    epl.Write(".reset()");
                    delimiter = ",";
                }

                epl.Write(";\n");

                epl.Write("on SupportBean_S1 merge MyTable as mt when matched then update set mt.reset();\n");
                env.CompileDeploy(epl.ToString());

                env.SendEventBean(new SupportBean("E0", 2));
                AssertTableSum(env, numColumns, 2);

                env.SendEventBean(new SupportBean_S0(0));
                AssertTableReset(env, numColumns);

                env.SendEventBean(new SupportBean("E1", 3));
                AssertTableSum(env, numColumns, 3);

                env.SendEventBean(new SupportBean_S1(0));
                AssertTableReset(env, numColumns);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }

        public class ClientCompileLargeSelectCol : RegressionExecution
        {
            private readonly EventRepresentationChoice representation;
            private readonly int numColumns;

            public ClientCompileLargeSelectCol(
                EventRepresentationChoice representation,
                int numColumns)
            {
                this.representation = representation;
                this.numColumns = numColumns;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = new StringWriter();
                epl.Write(representation.GetAnnotationText());
                epl.Write("@name('s0') select ");
                var delimiter = "";
                for (var i = 0; i < numColumns; i++) {
                    epl.Write(delimiter);
                    epl.Write("TheString||'");
                    epl.Write(i.ToString());
                    epl.Write("' as c");
                    epl.Write(i.ToString());
                    delimiter = ",";
                }

                epl.Write(" from SupportBean");
                env.CompileDeploy(epl.ToString()).AddListener("s0");

                env.SendEventBean(new SupportBean("x", 0));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        for (var i = 0; i < numColumns; i++) {
                            Assert.IsTrue(@event.EventType.IsProperty("c" + i));
                            Assert.AreEqual("x" + i, @event.Get("c" + i));
                        }
                    });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "representation=" +
                       representation +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }

        private static StringWriter SetupTable(
            int numColumns,
            string selector,
            string tableColType,
            BiConsumer<StringWriter, int> intoTable)
        {
            var epl = new StringWriter();
            epl.Write("@name('table') create table MyTable(");
            var delimiter = "";
            for (var i = 0; i < numColumns; i++) {
                epl.Write(delimiter);
                epl.Write("c");
                epl.Write(i.ToString());
                epl.Write(" ");
                epl.Write(tableColType);
                delimiter = ",";
            }

            epl.Write(");\n");

            epl.Write("into table MyTable select ");
            delimiter = "";
            for (var i = 0; i < numColumns; i++) {
                epl.Write(delimiter);
                intoTable.Invoke(epl, i);
                epl.Write(" as c");
                epl.Write(i.ToString());
                delimiter = ",";
            }

            epl.Write(" from ");
            epl.Write(selector);
            epl.Write(";\n");
            return epl;
        }

        private static void AssertTableSum(
            RegressionEnvironment env,
            int numColumns,
            int intPrimitive)
        {
            env.AssertIterator(
                "table",
                enumerator => {
                    var result = enumerator.Advance();
                    for (var i = 0; i < numColumns; i++) {
                        Assert.IsTrue(result.EventType.IsProperty("c" + i));
                        Assert.AreEqual(intPrimitive + i, result.Get("c" + i));
                    }
                });
        }

        private static void AssertTableReset(
            RegressionEnvironment env,
            int numColumns)
        {
            env.AssertIterator(
                "table",
                enumerator => {
                    var result = enumerator.Advance();
                    for (var i = 0; i < numColumns; i++) {
                        Assert.IsNull(result.Get("c" + i));
                    }
                });
        }

        private static void AssertTableWindow(
            RegressionEnvironment env,
            int numColumns,
            SupportBean sb)
        {
            env.AssertIterator(
                "table",
                iterator => {
                    var result = iterator.Advance();
                    for (var i = 0; i < numColumns; i++) {
                        var beans = (SupportBean[])result.Get("c" + i);
                        Assert.AreEqual(1, beans.Length);
                        Assert.AreEqual(beans[0], sb);
                    }
                });
        }
    }
}