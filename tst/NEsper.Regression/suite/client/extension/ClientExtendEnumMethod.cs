///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.enummethod;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;


namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendEnumMethod
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithScalarNoParamMedian(execs);
            WithEventLambdaMedian(execs);
            WithScalarLambdaMedian(execs);
            WithScalarNoLambdaWithParams(execs);
            WithScalarEarlyExit(execs);
            WithPredicateReturnEvents(execs);
            WithPredicateReturnSingleEvent(execs);
            WithTwoLambdaParameters(execs);
            WithLambdaEventInputValueAndIndex(execs);
            WithLambdaScalarInputValueAndIndex(execs);
            WithLambdaScalarStateAndValue(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithLambdaScalarStateAndValue(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendEnumLambdaScalarStateAndValue());
            return execs;
        }

        public static IList<RegressionExecution> WithLambdaScalarInputValueAndIndex(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendEnumLambdaScalarInputValueAndIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithLambdaEventInputValueAndIndex(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendEnumLambdaEventInputValueAndIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoLambdaParameters(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendEnumTwoLambdaParameters());
            return execs;
        }

        public static IList<RegressionExecution> WithPredicateReturnSingleEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendEnumPredicateReturnSingleEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithPredicateReturnEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendEnumPredicateReturnEvents());
            return execs;
        }

        public static IList<RegressionExecution> WithScalarEarlyExit(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendEnumScalarEarlyExit());
            return execs;
        }

        public static IList<RegressionExecution> WithScalarNoLambdaWithParams(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendEnumScalarNoLambdaWithParams());
            return execs;
        }

        public static IList<RegressionExecution> WithScalarLambdaMedian(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendEnumScalarLambdaMedian());
            return execs;
        }

        public static IList<RegressionExecution> WithEventLambdaMedian(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendEnumEventLambdaMedian());
            return execs;
        }

        public static IList<RegressionExecution> WithScalarNoParamMedian(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendEnumScalarNoParamMedian());
            return execs;
        }

        private class ClientExtendEnumLambdaScalarStateAndValue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select Strvals.enumPlugInLambdaScalarWStateAndValue('X', (r, v) => r || v) as c0 " +
                    "from SupportCollection";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("c0")));

                SendAssert(env, "Xa", "a");
                SendAssert(env, "Xab", "a,b");

                env.UndeployAll();
            }

            private static void SendAssert(
                RegressionEnvironment env,
                string expected,
                string csv)
            {
                env.SendEventBean(SupportCollection.MakeString(csv));
                env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { expected });
            }
        }

        private class ClientExtendEnumLambdaScalarInputValueAndIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select Intvals.enumPlugInLambdaScalarWPredicateAndIndex((v, ind) => v > 0 and ind < 3) as c0 " +
                    "from SupportCollection";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => ClassicAssert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("c0")));

                SendAssert(env, 0, "-1,-2");
                SendAssert(env, 1, "-1,2");
                SendAssert(env, 2, "2,-1,2");
                SendAssert(env, 3, "2,2,2");
                SendAssert(env, 3, "2,2,2,2");

                env.UndeployAll();
            }

            private static void SendAssert(
                RegressionEnvironment env,
                int? expected,
                string csv)
            {
                env.SendEventBean(SupportCollection.MakeNumeric(csv));
                env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { expected });
            }
        }

        private class ClientExtendEnumLambdaEventInputValueAndIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "(select * from SupportBean#keepall).enumPlugInLambdaEventWPredicateAndIndex((v, ind) => v.IntPrimitive > 0 and ind < 3) as c0 " +
                          "from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => ClassicAssert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("c0")));

                SendAssert(env, null);

                env.SendEventBean(new SupportBean("E1", -1));
                SendAssert(env, 0);

                env.SendEventBean(new SupportBean("E2", 10));
                SendAssert(env, 1);

                env.SendEventBean(new SupportBean("E3", 20));
                SendAssert(env, 2);

                env.SendEventBean(new SupportBean("E4", 3));
                SendAssert(env, 2);

                env.UndeployAll();
            }

            private static void SendAssert(
                RegressionEnvironment env,
                int? expected)
            {
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "c0", expected);
            }
        }

        private class ClientExtendEnumTwoLambdaParameters : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "(select * from SupportBean#keepall).enumPlugInTwoLambda(l1 -> 2*IntPrimitive, l2 -> 3*IntPrimitive) as c0 " +
                          "from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => ClassicAssert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("c0")));

                SendAssert(env, null);

                env.SendEventBean(new SupportBean("E1", 0));
                SendAssert(env, 0);

                env.SendEventBean(new SupportBean("E2", 2));
                SendAssert(env, 10);

                env.SendEventBean(new SupportBean("E3", 4));
                SendAssert(env, 30);

                env.UndeployAll();
            }

            private static void SendAssert(
                RegressionEnvironment env,
                int? expected)
            {
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "c0", expected);
            }
        }

        private class ClientExtendEnumPredicateReturnSingleEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "(select * from SupportBean#keepall).enumPlugInReturnSingleEvent(v => IntPrimitive > 0).TheString as c0 " +
                          "from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("c0")));

                SendAssert(env, null);

                env.SendEventBean(new SupportBean("E1", -1));
                SendAssert(env, null);

                env.SendEventBean(new SupportBean("E2", 1));
                SendAssert(env, "E2");

                env.SendEventBean(new SupportBean("E3", 0));
                SendAssert(env, "E2");

                env.SendEventBean(new SupportBean("E4", 1));
                SendAssert(env, "E2");

                env.UndeployAll();
            }

            private static void SendAssert(
                RegressionEnvironment env,
                string expected)
            {
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "c0", expected);
            }
        }

        private class ClientExtendEnumPredicateReturnEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "(select * from SupportBean#keepall).enumPlugInReturnEvents(v => IntPrimitive > 0).lastOf().TheString as c0 " +
                          "from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("c0")));

                SendAssert(env, null);

                env.SendEventBean(new SupportBean("E1", -1));
                SendAssert(env, null);

                env.SendEventBean(new SupportBean("E2", 1));
                SendAssert(env, "E2");

                env.SendEventBean(new SupportBean("E3", 0));
                SendAssert(env, "E2");

                env.SendEventBean(new SupportBean("E4", 1));
                SendAssert(env, "E4");

                env.UndeployAll();
            }

            private static void SendAssert(
                RegressionEnvironment env,
                string expected)
            {
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "c0", expected);
            }
        }

        private class ClientExtendEnumScalarEarlyExit : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0".SplitCsv();
                var epl = "@name('s0') select " +
                          "Intvals.enumPlugInEarlyExit() as val0 " +
                          "from SupportCollection";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtTypes("s0", fields, new Type[] { typeof(int?) });

                SendAssert(env, fields, 12, "12,1,1");
                SendAssert(env, fields, 10, "5,5,5");

                env.UndeployAll();
            }

            private static void SendAssert(
                RegressionEnvironment env,
                string[] fields,
                int? expected,
                string csv)
            {
                env.SendEventBean(SupportCollection.MakeNumeric(csv));
                env.AssertPropsNew("s0", fields, new object[] { expected });
            }
        }

        private class ClientExtendEnumScalarNoLambdaWithParams : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0".SplitCsv();
                var epl = "@name('s0') select " +
                          "Intvals.enumPlugInOne(10, 20) as val0 " +
                          "from SupportCollection";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtTypes("s0", fields, new Type[] { typeof(int?) });

                SendAssert(env, fields, 11, "1,2,11,3");
                SendAssert(env, fields, 0, "");
                SendAssert(env, fields, 23, "11,12");

                env.UndeployAll();
            }

            private static void SendAssert(
                RegressionEnvironment env,
                string[] fields,
                int? expected,
                string csv)
            {
                env.SendEventBean(SupportCollection.MakeNumeric(csv));
                env.AssertPropsNew("s0", fields, new object[] { expected });
            }
        }

        private class ClientExtendEnumScalarLambdaMedian : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var epl = "@name('s0') select " +
                          "Strvals.enumPlugInMedian(v => extractNum(v)) as c0," +
                          "Strvals.enumPlugInMedian((v, i) => extractNum(v) + i*10) as c1," +
                          "Strvals.enumPlugInMedian((v, i, s) => extractNum(v) + i*10+s*100) as c2 " +
                          "from SupportCollection";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtTypes("s0", fields, new Type[] { typeof(double?), typeof(double?), typeof(double?) });

                SendAssert(env, fields, 3d, 18d, 418d, "E2,E1,E5,E4");
                SendAssert(env, fields, null, null, null, "E1");
                SendAssert(env, fields, null, null, null, "");
                SendAssert(env, fields, null, null, null, null);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                string[] fields,
                double? c0,
                double? c1,
                double? c2,
                string csv)
            {
                env.SendEventBean(SupportCollection.MakeString(csv));
                env.AssertPropsNew("s0", fields, new object[] { c0, c1, c2 });
            }
        }

        private class ClientExtendEnumEventLambdaMedian : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0".SplitCsv();
                var epl = "@name('s0') select " +
                          "Contained.enumPlugInMedian(x => P00) as val0 " +
                          "from SupportBean_ST0_Container";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStmtTypes("s0", fields, new Type[] { typeof(double?) });

                SendAssert(env, fields, 11d, "E1,12", "E2,11", "E3,2");
                SendAssert(env, fields, null, null);
                SendAssert(env, fields, null);
                SendAssert(env, fields, 0d, "E1,1", "E2,0", "E3,0");

                env.UndeployAll();
            }

            private static void SendAssert(
                RegressionEnvironment env,
                string[] fields,
                double? expected,
                params string[] values)
            {
                env.SendEventBean(SupportBean_ST0_Container.Make2Value(values));
                env.AssertPropsNew("s0", fields, new object[] { expected });
            }
        }

        private class ClientExtendEnumScalarNoParamMedian : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0".SplitCsv();
                var eplFragment = "@name('s0') select Intvals.enumPlugInMedian() as val0 from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.AssertStmtTypes("s0", fields, new Type[] { typeof(double?) });

                SendAssert(env, fields, 2d, "1,2,2,4");
                SendAssert(env, fields, 2d, "1,2,2,10");
                SendAssert(env, fields, 2.5d, "1,2,3,4");
                SendAssert(env, fields, 2d, "1,2,2,3,4");
                SendAssert(env, fields, 2d, "1,1,2,2,3,4");
                SendAssert(env, fields, 1d, "1,1");
                SendAssert(env, fields, 1.5d, "1,2");
                SendAssert(env, fields, 2d, "1,3");
                SendAssert(env, fields, 2.5d, "1,4");
                SendAssert(env, fields, null, "1");
                SendAssert(env, fields, null, "");

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                string[] fields,
                double? expected,
                string intcsv)
            {
                env.SendEventBean(SupportCollection.MakeNumeric(intcsv));
                env.AssertPropsNew("s0", fields, new object[] { expected });
            }
        }

        public static int ExtractNum(string arg)
        {
            return int.Parse(arg.Substring(1));
        }

        public class MyLocalEnumMethodForgeMedian : EnumMethodForgeFactory
        {
            public static readonly DotMethodFP[] FOOTPRINTS = new DotMethodFP[] {
                new DotMethodFP(DotMethodFPInputEnum.SCALAR_NUMERIC),
                new DotMethodFP(
                    DotMethodFPInputEnum.SCALAR_ANY,
                    new DotMethodFPParam(1, "value-selector", EPLExpressionParamType.NUMERIC)),
                new DotMethodFP(
                    DotMethodFPInputEnum.EVENTCOLL,
                    new DotMethodFPParam(1, "value-selector", EPLExpressionParamType.NUMERIC)),
                new DotMethodFP(
                    DotMethodFPInputEnum.SCALAR_ANY,
                    new DotMethodFPParam(2, "(value-selector, index)", EPLExpressionParamType.NUMERIC)),
                new DotMethodFP(
                    DotMethodFPInputEnum.EVENTCOLL,
                    new DotMethodFPParam(2, "(value-selector, index)", EPLExpressionParamType.NUMERIC)),
                new DotMethodFP(
                    DotMethodFPInputEnum.SCALAR_ANY,
                    new DotMethodFPParam(3, "(value-selector, index, size)", EPLExpressionParamType.NUMERIC)),
                new DotMethodFP(
                    DotMethodFPInputEnum.EVENTCOLL,
                    new DotMethodFPParam(3, "(value-selector, index, size)", EPLExpressionParamType.NUMERIC))
            };

            public EnumMethodDescriptor Initialize(EnumMethodInitializeContext context)
            {
                return new EnumMethodDescriptor(FOOTPRINTS);
            }

            public EnumMethodMode Validate(EnumMethodValidateContext context)
            {
                var stateClass = typeof(MyLocalEnumMethodMedianState); // the class providing state
                var serviceClass = typeof(MyLocalEnumMethodMedianService); // the class providing the processing method
                var methodName = "Next"; // the name of the method for processing an item of input values
                EPChainableType
                    returnType =
                        new EPChainableTypeClass(typeof(double?)); // indicate that we are returning a Double-type value
                var earlyExit = false;

                var mode = new EnumMethodModeStaticMethod(stateClass, serviceClass, methodName, returnType, earlyExit);

                // we allow 1, 2 or 3 parameters
                mode.LambdaParameters = descriptor => {
                    if (descriptor.LambdaParameterNumber == 1) {
                        return EnumMethodLambdaParameterTypeIndex.INSTANCE;
                    }

                    if (descriptor.LambdaParameterNumber == 2) {
                        return EnumMethodLambdaParameterTypeSize.INSTANCE;
                    }

                    return EnumMethodLambdaParameterTypeValue.INSTANCE;
                };

                return mode;
            }
        }

        public class MyLocalEnumMethodMedianState : EnumMethodState
        {
            private List<int> list = new List<int>();

            public object State {
                get {
                    list.Sort();
                    // get count of scores
                    var totalElements = list.Count;
                    if (totalElements < 2) {
                        return null;
                    }

                    // check if total number of scores is even
                    if (totalElements % 2 == 0) {
                        var sumOfMiddleElements = list[totalElements / 2] + list[totalElements / 2 - 1];
                        // calculate average of middle elements
                        return (double)sumOfMiddleElements / 2;
                    }

                    return (double)list[totalElements / 2];
                }
            }

            public void Add(int value)
            {
                list.Add(value);
            }

            public void SetParameter(
                int parameterNumber,
                object value)
            {
            }

            public bool IsCompleted => false;
        }

        public class MyLocalEnumMethodMedianService
        {
            public static void Next(
                MyLocalEnumMethodMedianState state,
                object element,
                object valueSelectorResult)
            {
                state.Add(valueSelectorResult.AsInt32());
            }

            public static void Next(
                MyLocalEnumMethodMedianState state,
                object element)
            {
                state.Add(element.AsInt32());
            }
        }

        public class MyLocalEnumMethodForgeOne : EnumMethodForgeFactory
        {
            public static readonly DotMethodFP[] FOOTPRINTS = new DotMethodFP[] {
                new DotMethodFP(
                    DotMethodFPInputEnum.SCALAR_NUMERIC,
                    new DotMethodFPParam("from", EPLExpressionParamType.NUMERIC),
                    new DotMethodFPParam("to", EPLExpressionParamType.NUMERIC))
            };

            public EnumMethodDescriptor Initialize(EnumMethodInitializeContext context)
            {
                return new EnumMethodDescriptor(FOOTPRINTS);
            }

            public EnumMethodMode Validate(EnumMethodValidateContext context)
            {
                return new EnumMethodModeStaticMethod(
                    typeof(MyLocalEnumMethodForgeOneState),
                    typeof(MyLocalEnumMethodForgeOneState),
                    "Next",
                    new EPChainableTypeClass(typeof(int?)),
                    false);
            }
        }

        public class MyLocalEnumMethodForgeOneState : EnumMethodState
        {
            private int from;
            private int to;
            private int sum;

            public void SetParameter(
                int parameterNumber,
                object value)
            {
                if (parameterNumber == 0) {
                    from = value.AsInt32();
                }

                if (parameterNumber == 1) {
                    to = value.AsInt32();
                }
            }

            public static void Next(
                MyLocalEnumMethodForgeOneState state,
                object num)
            {
                state.Add(num.AsInt32());
            }

            public void Add(int value)
            {
                if (value >= from && value <= to) {
                    sum += value;
                }
            }

            public object State => sum;

            public bool IsCompleted => false;
        }

        public class MyLocalEnumMethodForgeEarlyExit : EnumMethodForgeFactory
        {
            public static readonly DotMethodFP[] FOOTPRINTS = new DotMethodFP[] {
                new DotMethodFP(DotMethodFPInputEnum.SCALAR_NUMERIC)
            };

            public EnumMethodDescriptor Initialize(EnumMethodInitializeContext context)
            {
                return new EnumMethodDescriptor(FOOTPRINTS);
            }

            public EnumMethodMode Validate(EnumMethodValidateContext context)
            {
                return new EnumMethodModeStaticMethod(
                    typeof(MyLocalEnumMethodForgeEarlyExitState),
                    typeof(MyLocalEnumMethodForgeEarlyExitState),
                    "Next",
                    new EPChainableTypeClass(typeof(int?)),
                    true);
            }
        }

        public class MyLocalEnumMethodForgeEarlyExitState : EnumMethodState
        {
            private int sum;

            public static void Next(
                MyLocalEnumMethodForgeEarlyExitState state,
                object num)
            {
                state.sum += num.AsInt32();
            }

            public bool IsCompleted => sum >= 10;

            public object State => sum;

            public void SetParameter(
                int parameterNumber,
                object value)
            {
            }
        }

        public class MyLocalEnumMethodForgePredicateReturnEvents : EnumMethodForgeFactory
        {
            public EnumMethodDescriptor Initialize(EnumMethodInitializeContext context)
            {
                var footprints = new DotMethodFP[] {
                    new DotMethodFP(
                        DotMethodFPInputEnum.EVENTCOLL,
                        new DotMethodFPParam(1, "predicate", EPLExpressionParamType.BOOLEAN))
                };
                return new EnumMethodDescriptor(footprints);
            }

            public EnumMethodMode Validate(EnumMethodValidateContext context)
            {
                var type = EPChainableTypeHelper.CollectionOfEvents(context.InputEventType);
                return new EnumMethodModeStaticMethod(
                    typeof(MyLocalEnumMethodForgePredicateReturnEventsState),
                    typeof(MyLocalEnumMethodForgePredicateReturnEvents),
                    "Next",
                    type,
                    false);
            }

            public static void Next(
                MyLocalEnumMethodForgePredicateReturnEventsState state,
                EventBean @event,
                bool? pass)
            {
                if (pass ?? false) {
                    state.Add(@event);
                }
            }
        }

        public class MyLocalEnumMethodForgePredicateReturnEventsState : EnumMethodState
        {
            List<EventBean> events = new List<EventBean>();

            public object State => events;

            public void Add(EventBean @event)
            {
                events.Add(@event);
            }

            public void SetParameter(
                int parameterNumber,
                object value)
            {
            }

            public bool IsCompleted => false;
        }

        public class MyLocalEnumMethodForgePredicateReturnSingleEvent : EnumMethodForgeFactory
        {
            public EnumMethodDescriptor Initialize(EnumMethodInitializeContext context)
            {
                var footprints = new DotMethodFP[] {
                    new DotMethodFP(
                        DotMethodFPInputEnum.EVENTCOLL,
                        new DotMethodFPParam(1, "predicate", EPLExpressionParamType.BOOLEAN))
                };
                return new EnumMethodDescriptor(footprints);
            }

            public EnumMethodMode Validate(EnumMethodValidateContext context)
            {
                var type = new EPChainableTypeEventSingle(context.InputEventType);
                return new EnumMethodModeStaticMethod(
                    typeof(MyLocalEnumMethodForgePredicateReturnSingleEventState),
                    typeof(MyLocalEnumMethodForgePredicateReturnSingleEvent),
                    "Next",
                    type,
                    true);
            }

            public static void Next(
                MyLocalEnumMethodForgePredicateReturnSingleEventState state,
                EventBean @event,
                bool? pass)
            {
                if (pass ?? false) {
                    state.Add(@event);
                }
            }
        }

        public class MyLocalEnumMethodForgePredicateReturnSingleEventState : EnumMethodState
        {
            EventBean _event;

            public object State => _event;

            public void Add(EventBean @event)
            {
                _event = @event;
            }

            public bool IsCompleted => _event != null;

            public void SetParameter(
                int parameterNumber,
                object value)
            {
            }
        }

        public class MyLocalEnumMethodForgeTwoLambda : EnumMethodForgeFactory
        {
            public EnumMethodDescriptor Initialize(EnumMethodInitializeContext context)
            {
                var footprints = new DotMethodFP[] {
                    new DotMethodFP(
                        DotMethodFPInputEnum.EVENTCOLL,
                        new DotMethodFPParam(1, "v1", EPLExpressionParamType.ANY),
                        new DotMethodFPParam(1, "v2", EPLExpressionParamType.ANY))
                };
                return new EnumMethodDescriptor(footprints);
            }

            public EnumMethodMode Validate(EnumMethodValidateContext context)
            {
                return new EnumMethodModeStaticMethod(
                    typeof(MyLocalEnumMethodForgeTwoLambdaState),
                    typeof(MyLocalEnumMethodForgeTwoLambdaState),
                    "Next",
                    new EPChainableTypeClass(typeof(int?)),
                    false);
            }
        }

        public class MyLocalEnumMethodForgeTwoLambdaState : EnumMethodState
        {
            private int? sum;

            public object State => sum;

            public static void Next(
                MyLocalEnumMethodForgeTwoLambdaState state,
                EventBean @event,
                object v1,
                object v2)
            {
                state.Add((int?)v1, (int?)v2);
            }

            void Add(
                int? v1,
                int? v2)
            {
                if (sum == null) {
                    sum = 0;
                }

                sum += v1;
                sum += v2;
            }

            public void SetParameter(
                int parameterNumber,
                object value)
            {
            }

            public bool IsCompleted => false;
        }

        public class MyLocalEnumMethodForgeThree : EnumMethodForgeFactory
        {
            public EnumMethodDescriptor Initialize(EnumMethodInitializeContext context)
            {
                var footprints = new DotMethodFP[] {
                    new DotMethodFP(
                        DotMethodFPInputEnum.ANY,
                        new DotMethodFPParam(2, "value, index", EPLExpressionParamType.BOOLEAN))
                };

                return new EnumMethodDescriptor(footprints);
            }

            public EnumMethodMode Validate(EnumMethodValidateContext context)
            {
                var mode = new EnumMethodModeStaticMethod(
                    typeof(MyLocalEnumMethodForgeThreeState),
                    typeof(MyLocalEnumMethodForgeThree),
                    "Next",
                    new EPChainableTypeClass(typeof(int?)),
                    false);
                mode.LambdaParameters = descriptor => {
                    if (descriptor.LambdaParameterNumber == 0) {
                        return EnumMethodLambdaParameterTypeValue.INSTANCE;
                    }

                    return EnumMethodLambdaParameterTypeIndex.INSTANCE;
                };
                return mode;
            }

            public static void Next(
                MyLocalEnumMethodForgeThreeState state,
                object value,
                bool? pass)
            {
                if (pass ?? false) {
                    state.Increment();
                }
            }

            public static void Next(
                MyLocalEnumMethodForgeThreeState state,
                EventBean @event,
                bool? pass)
            {
                if (pass ?? false) {
                    state.Increment();
                }
            }
        }

        public class MyLocalEnumMethodForgeThreeState : EnumMethodState
        {
            int count;

            public void Increment()
            {
                count++;
            }

            public object State => count;

            public void SetParameter(
                int parameterNumber,
                object value)
            {
            }

            public bool IsCompleted => false;
        }

        public class MyLocalEnumMethodForgeStateWValue : EnumMethodForgeFactory
        {
            public EnumMethodDescriptor Initialize(EnumMethodInitializeContext context)
            {
                var footprints = new DotMethodFP[] {
                    new DotMethodFP(
                        DotMethodFPInputEnum.ANY,
                        new DotMethodFPParam(0, "initialvalue", EPLExpressionParamType.ANY),
                        new DotMethodFPParam(2, "result, index", EPLExpressionParamType.ANY))
                };
                return new EnumMethodDescriptor(footprints);
            }

            public EnumMethodMode Validate(EnumMethodValidateContext context)
            {
                var mode = new EnumMethodModeStaticMethod(
                    typeof(MyLocalEnumMethodForgeStateWValueState),
                    typeof(MyLocalEnumMethodForgeStateWValueState),
                    "Next",
                    new EPChainableTypeClass(typeof(string)),
                    false);
                mode.LambdaParameters = descriptor => {
                    if (descriptor.LambdaParameterNumber == 0) {
                        return new EnumMethodLambdaParameterTypeStateGetter(typeof(string), "Result");
                    }

                    return EnumMethodLambdaParameterTypeValue.INSTANCE;
                };
                return mode;
            }
        }

        public class MyLocalEnumMethodForgeStateWValueState : EnumMethodState
        {
            private string _result;

            public void SetParameter(
                int parameterNumber,
                object value)
            {
                this._result = (string)value;
            }

            public string Result => _result;

            public static void Next(
                MyLocalEnumMethodForgeStateWValueState state,
                object value,
                string result)
            {
                state._result = result;
            }

            public object State => _result;

            public bool IsCompleted => false;
        }
    }
}