///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.extend.aggmultifunc;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendAggregationMultiFunction
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSimpleState(execs);
            WithScalarOnly(execs);
            WithScalarArray(execs);
            WithScalarColl(execs);
            WithSingleEvent(execs);
            WithCollEvent(execs);
            WithSameProviderGroupedReturnSingleEvent(execs);
            WithWithTable(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWithTable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationMFManagedWithTable());
            return execs;
        }

        public static IList<RegressionExecution> WithSameProviderGroupedReturnSingleEvent(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationMFManagedSameProviderGroupedReturnSingleEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithCollEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationMFManagedCollEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationMFManagedSingleEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithScalarColl(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationMFManagedScalarColl());
            return execs;
        }

        public static IList<RegressionExecution> WithScalarArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationMFManagedScalarArray());
            return execs;
        }

        public static IList<RegressionExecution> WithScalarOnly(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationMFManagedScalarOnly());
            return execs;
        }

        public static IList<RegressionExecution> WithSimpleState(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationMFManagedSimpleState());
            return execs;
        }

        public void Run(RegressionEnvironment env)
        {
        }

        private class ClientExtendAggregationMFManagedWithTable : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create table MyTable(col collectEvents())", path);
                env.CompileDeploy("into table MyTable select collectEvents(*) as col from SupportBean#length(2)", path);
                env.CompileDeploy("@name('s0') on SupportBean_S0 select col as c0 from MyTable", path)
                    .AddListener("s0");

                var e1 = new SupportBean("E1", 1);
                env.SendEventBean(e1);
                SendAssertList(env, e1);

                var e2 = new SupportBean("E2", 2);
                env.SendEventBean(e2);
                SendAssertList(env, e1, e2);

                env.Milestone(0);

                var e3 = new SupportBean("E3", 3);
                env.SendEventBean(e3);
                SendAssertList(env, e2, e3);

                env.UndeployAll();
            }
        }

        private class ClientExtendAggregationMFManagedCollEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldsEnumEvent = "c0,c1,c2".SplitCsv();
                var eplEnumEvent = "@name('s0') select " +
                                   "ee() as c0, " +
                                   "ee().allOf(v => v.TheString = 'E1') as c1, " +
                                   "ee().allOf(v => v.IntPrimitive = 1) as c2 " +
                                   "from SupportBean";
                env.CompileDeploy(eplEnumEvent).AddListener("s0");

                var expectedEnumEvent = new object[][] {
                    new object[] { "c0", typeof(SupportBean[]), typeof(SupportBean).FullName, true },
                    new object[] { "c1", typeof(bool?), null, null },
                    new object[] { "c2", typeof(bool?), null, null }
                };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        expectedEnumEvent,
                        statement.EventType,
                        SupportEventTypeAssertionEnumExtensions.GetSetWithFragment()));

                var eventEnumOne = new SupportBean("E1", 1);
                env.SendEventBean(eventEnumOne);
                env.AssertPropsNew(
                    "s0",
                    fieldsEnumEvent,
                    new object[] { new SupportBean[] { eventEnumOne }, true, true });

                var eventEnumTwo = new SupportBean("E2", 2);
                env.SendEventBean(eventEnumTwo);
                env.AssertPropsNew(
                    "s0",
                    fieldsEnumEvent,
                    new object[] { new SupportBean[] { eventEnumOne, eventEnumTwo }, false, false });

                env.UndeployAll();
            }
        }

        private class ClientExtendAggregationMFManagedSingleEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test single-event return
                var fieldsSingleEvent = "c0,c1,c2,c3,c4".SplitCsv();
                var eplSingleEvent = "@name('s0') select " +
                                     "se1() as c0, " +
                                     "se1().allOf(v => v.TheString = 'E1') as c1, " +
                                     "se1().allOf(v => v.IntPrimitive = 1) as c2, " +
                                     "se1().TheString as c3, " +
                                     "se1().IntPrimitive as c4 " +
                                     "from SupportBean";
                env.CompileDeploy(eplSingleEvent).AddListener("s0");

                var expectedSingleEvent = new object[][] {
                    new object[] { "c0", typeof(SupportBean), typeof(SupportBean).FullName, false },
                    new object[] { "c1", typeof(bool?), null, null }, new object[] { "c2", typeof(bool?), null, null },
                    new object[] { "c3", typeof(string), null, null }, new object[] { "c4", typeof(int?), null, null },
                };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        expectedSingleEvent,
                        statement.EventType,
                        SupportEventTypeAssertionEnumExtensions.GetSetWithFragment()));

                var eventOne = new SupportBean("E1", 1);
                env.SendEventBean(eventOne);
                env.AssertPropsNew("s0", fieldsSingleEvent, new object[] { eventOne, true, true, "E1", 1 });

                env.Milestone(0);

                var eventTwo = new SupportBean("E2", 2);
                env.SendEventBean(eventTwo);
                env.AssertPropsNew("s0", fieldsSingleEvent, new object[] { eventTwo, false, false, "E2", 2 });

                env.UndeployAll();
            }
        }

        private class ClientExtendAggregationMFManagedScalarColl : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test scalar-collection only
                var fieldsScalarColl = "c2,c3".SplitCsv();
                var eplScalarColl = "@name('s0') select " +
                                    "sc(TheString) as c0, " +
                                    "sc(IntPrimitive) as c1, " +
                                    "sc(TheString).allOf(v => v = 'E1') as c2, " +
                                    "sc(IntPrimitive).allOf(v => v = 1) as c3 " +
                                    "from SupportBean";
                env.CompileDeploy(eplScalarColl).AddListener("s0");

                var expectedScalarColl = new object[][] {
                    new object[] { "c0", typeof(ICollection<string>), null, null },
                    new object[] { "c1", typeof(ICollection<int?>), null, null },
                    new object[] { "c2", typeof(bool?), null, null },
                    new object[] { "c3", typeof(bool?), null, null },
                };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        expectedScalarColl,
                        statement.EventType,
                        SupportEventTypeAssertionEnumExtensions.GetSetWithFragment()));

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        EPAssertionUtil.AssertEqualsExactOrder(
                            new object[] { "E1" },
                            @event.Get("c0").Unwrap<object>());
                        EPAssertionUtil.AssertEqualsExactOrder(
                            new object[] { 1 },
                            @event.Get("c1").Unwrap<object>());
                        EPAssertionUtil.AssertProps(@event, fieldsScalarColl, new object[] { true, true });
                    });

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        EPAssertionUtil.AssertEqualsExactOrder(
                            new object[] { "E1", "E2" },
                            @event.Get("c0").Unwrap<object>());
                        EPAssertionUtil.AssertEqualsExactOrder(
                            new object[] { 1, 2 },
                            @event.Get("c1").Unwrap<object>());
                        EPAssertionUtil.AssertProps(@event, fieldsScalarColl, new object[] { false, false });
                    });

                env.UndeployAll();
            }
        }

        private class ClientExtendAggregationMFManagedScalarArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldsScalarArray = "c0,c1,c2,c3".SplitCsv();
                var eplScalarArray = "@name('s0') select " +
                                     "sa(TheString) as c0, " +
                                     "sa(IntPrimitive) as c1, " +
                                     "sa(TheString).allOf(v => v = 'E1') as c2, " +
                                     "sa(IntPrimitive).allOf(v => v = 1) as c3 " +
                                     "from SupportBean";
                env.CompileDeploy(eplScalarArray).AddListener("s0");

                var expectedScalarArray = new object[][] {
                    new object[] { "c0", typeof(string[]), null, null },
                    new object[] { "c1", typeof(int?[]), null, null },
                    new object[] { "c2", typeof(bool?), null, null },
                    new object[] { "c3", typeof(bool?), null, null },
                };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        expectedScalarArray,
                        statement.EventType,
                        SupportEventTypeAssertionEnumExtensions.GetSetWithFragment()));

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew(
                    "s0",
                    fieldsScalarArray,
                    new object[] {
                        new string[] { "E1" }, new int?[] { 1 }, true, true
                    });

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew(
                    "s0",
                    fieldsScalarArray,
                    new object[] {
                        new string[] { "E1", "E2" }, new int?[] { 1, 2 }, false, false
                    });

                env.UndeployAll();
            }
        }

        private class ClientExtendAggregationMFManagedScalarOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldsScalar = "c0,c1".SplitCsv();
                var eplScalar = "@name('s0') select ss(TheString) as c0, ss(IntPrimitive) as c1 from SupportBean";
                env.CompileDeploy(eplScalar).AddListener("s0");

                var expectedScalar = new object[][] {
                    new object[] { "c0", typeof(string), null, null },
                    new object[] { "c1", typeof(int?), null, null }
                };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        expectedScalar,
                        statement.EventType,
                        SupportEventTypeAssertionEnumExtensions.GetSetWithFragment()));

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fieldsScalar, new object[] { "E1", 1 });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("s0", fieldsScalar, new object[] { "E2", 2 });

                env.UndeployAll();
            }
        }

        private class ClientExtendAggregationMFManagedSimpleState : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select collectEvents(*) as c0 from SupportBean#length(2)")
                    .AddListener("s0");

                var e1 = new SupportBean("E1", 1);
                env.SendEventBean(e1);
                AssertList(env, e1);

                var e2 = new SupportBean("E2", 2);
                env.SendEventBean(e2);
                AssertList(env, e1, e2);

                env.Milestone(0);

                var e3 = new SupportBean("E3", 3);
                env.SendEventBean(e3);
                AssertList(env, e2, e3);

                env.UndeployAll();
            }
        }

        private class ClientExtendAggregationMFManagedSameProviderGroupedReturnSingleEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select se1() as c0, se2() as c1 from SupportBean#keepall group by TheString";

                // test regular
                SupportAggMFMultiRTForge.Reset();
                SupportAggMFMultiRTHandler.Reset();
                SupportAggMFMultiRTSingleEventStateFactory.Reset();

                env.CompileDeploy(epl).AddListener("s0");
                TryAssertion(env);

                // test SODA
                var model = env.EplToModel(epl);
                SupportAggMFMultiRTForge.Reset();
                SupportAggMFMultiRTHandler.Reset();
                SupportAggMFMultiRTSingleEventStateFactory.Reset();
                ClassicAssert.AreEqual(epl, model.ToEPL());
                env.CompileDeploy(model).AddListener("s0");
                TryAssertion(env);
            }
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            var fields = "c0,c1".SplitCsv();

            env.AssertStatement(
                "s0",
                statement => {
                    var eventType = statement.EventType;
                    foreach (var prop in fields) {
                        ClassicAssert.AreEqual(typeof(SupportBean), eventType.GetPropertyDescriptor(prop).PropertyType);
                        ClassicAssert.AreEqual(true, eventType.GetPropertyDescriptor(prop).IsFragment);
                        ClassicAssert.AreEqual(
                            typeof(SupportBean).FullName,
                            eventType.GetFragmentType(prop).FragmentType.Name);
                    }
                });

            env.AssertThat(
                () => {
                    // there should be just 1 forge instance for all of the registered functions for this statement
                    ClassicAssert.AreEqual(1, SupportAggMFMultiRTForge.Forges.Count);
                    ClassicAssert.AreEqual(2, SupportAggMFMultiRTForge.FunctionDeclContexts.Count);
                    for (var i = 0; i < 2; i++) {
                        var contextDecl =
                            SupportAggMFMultiRTForge.FunctionDeclContexts[i];
                        ClassicAssert.AreEqual(i == 0 ? "se1" : "se2", contextDecl.FunctionName);
                        ClassicAssert.IsFalse(contextDecl.IsDistinct);
                        ClassicAssert.IsNotNull(contextDecl.Configuration);

                        var contextValid =
                            SupportAggMFMultiRTForge.FunctionHandlerValidationContexts[i];
                        ClassicAssert.AreEqual(i == 0 ? "se1" : "se2", contextValid.FunctionName);
                        ClassicAssert.IsNotNull(contextValid.ParameterExpressions);
                        ClassicAssert.IsNotNull(contextValid.AllParameterExpressions);
                        ClassicAssert.AreEqual(
                            "someinfovalue",
                            contextValid.Config.AdditionalConfiguredProperties.Get("someinfokey"));
                        ClassicAssert.IsNotNull(contextValid.EventTypes);
                        ClassicAssert.IsNotNull(contextValid.ValidationContext);
                        ClassicAssert.IsNotNull(contextValid.StatementName);
                    }

                    ClassicAssert.AreEqual(2, SupportAggMFMultiRTHandler.ProviderKeys.Count);
                    if (!SupportAggMFMultiRTHandler.AccessorModes.IsEmpty()) {
                        ClassicAssert.AreEqual(2, SupportAggMFMultiRTHandler.AccessorModes.Count);
                        ClassicAssert.AreEqual(1, SupportAggMFMultiRTHandler.StateFactoryModes.Count);
                    }

                    ClassicAssert.AreEqual(0, SupportAggMFMultiRTSingleEventStateFactory.StateContexts.Count);
                });

            // group 1
            var eventOne = new SupportBean("E1", 1);
            env.SendEventBean(eventOne);
            env.AssertPropsNew("s0", fields, new object[] { eventOne, eventOne });
            if (!SupportAggMFMultiRTSingleEventStateFactory.StateContexts.IsEmpty()) {
                ClassicAssert.AreEqual(1, SupportAggMFMultiRTSingleEventStateFactory.StateContexts.Count);
                var context =
                    SupportAggMFMultiRTSingleEventStateFactory.StateContexts[0];
                // Not available: ClassicAssert.AreEqual("E1", context.getGroupKey());
            }

            // group 2
            var eventTwo = new SupportBean("E2", 2);
            env.SendEventBean(eventTwo);
            env.AssertPropsNew("s0", fields, new object[] { eventTwo, eventTwo });
            if (!SupportAggMFMultiRTSingleEventStateFactory.StateContexts.IsEmpty()) {
                ClassicAssert.AreEqual(2, SupportAggMFMultiRTSingleEventStateFactory.StateContexts.Count);
            }

            env.UndeployAll();
        }

        private static void SendAssertList(
            RegressionEnvironment env,
            params SupportBean[] events)
        {
            env.SendEventBean(new SupportBean_S0(1));
            AssertList(env, events);
        }

        private static void AssertList(
            RegressionEnvironment env,
            params SupportBean[] events)
        {
            env.AssertEventNew(
                "s0",
                @event => {
                    var @out = @event.Get("c0").UnwrapIntoArray<object>();
                    EPAssertionUtil.AssertEqualsExactOrder(@out, events);
                });
        }
    }
} // end of namespace