///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.variable;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.threading;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.runtime.@internal.kernel.service;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.epl.variable
{
    public class EPLVariables
    {
        public enum MyEnumWithOverride
        {
            LONG,
            SHORT
        }

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLVariableSimplePreconfigured());
            execs.Add(new EPLVariableSimpleSameModule());
            execs.Add(new EPLVariableSimpleTwoModules());
            execs.Add(new EPLVariableSimpleSet());
            execs.Add(new EPLVariableSimpleSetSceneTwo());
            execs.Add(new EPLVariableCompile());
            execs.Add(new EPLVariableEPRuntime());
            execs.Add(new EPLVariableObjectModel());
            execs.Add(new EPLVariableOnSetWithFilter());
            execs.Add(new EPLVariableDotVariableSeparateThread());
            execs.Add(new EPLVariableInvokeMethod());
            execs.Add(new EPLVariableConstantVariable());
            execs.Add(new EPLVariableSetSubquery());
            execs.Add(new EPLVariableVariableInFilterBoolean());
            execs.Add(new EPLVariableVariableInFilter());
            execs.Add(new EPLVariableAssignmentOrderNoDup());
            execs.Add(new EPLVariableAssignmentOrderDup());
            execs.Add(new EPLVariableRuntimeOrderMultiple());
            execs.Add(new EPLVariableCoercion());
            execs.Add(new EPLVariableInvalidSet());
            execs.Add(new EPLVariableFilterConstantCustomTypePreconfigured());
            return execs;
        }

        private static SupportBean_A SendSupportBean_A(
            RegressionEnvironment env,
            string id)
        {
            var bean = new SupportBean_A(id);
            env.SendEventBean(bean);
            return bean;
        }

        private static SupportBean SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
            return bean;
        }

        private static SupportBean SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int? intBoxed)
        {
            var bean = MakeSupportBean(theString, intPrimitive, intBoxed);
            env.SendEventBean(bean);
            return bean;
        }

        private static void SendSupportBeanNewThread(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int? intBoxed)
        {
            try {
                var t = new Thread(
                    () => {
                        var bean = MakeSupportBean(theString, intPrimitive, intBoxed);
                        env.SendEventBean(bean);
                    });
                t.Start();
                t.Join();
            }
            catch (ThreadInterruptedException ex) {
                Assert.Fail(ex.Message);
            }
        }

        private static void SendSupportBeanS0NewThread(
            RegressionEnvironment env,
            int id,
            string p00,
            string p01)
        {
            try {
                var t = new Thread(() => env.SendEventBean(new SupportBean_S0(id, p00, p01)));
                t.Start();
                t.Join();
            }
            catch (ThreadInterruptedException ex) {
                Assert.Fail(ex.Message);
            }
        }

        private static SupportBean MakeSupportBean(
            string theString,
            int intPrimitive,
            int? intBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            return bean;
        }

        private static void AssertVariableValuesPreconfigured(
            RegressionEnvironment env,
            string[] names,
            object[] values)
        {
            Assert.AreEqual(names.Length, values.Length);

            // assert one-by-one
            for (var i = 0; i < names.Length; i++) {
                Assert.AreEqual(values[i], env.Runtime.VariableService.GetVariableValue(null, names[i]));
            }

            // get and assert all
            var all = env.Runtime.VariableService.GetVariableValueAll();
            for (var i = 0; i < names.Length; i++) {
                Assert.AreEqual(values[i], all.Get(new DeploymentIdNamePair(null, names[i])));
            }

            // get by request
            ISet<DeploymentIdNamePair> nameSet = new HashSet<DeploymentIdNamePair>();
            foreach (var name in names) {
                nameSet.Add(new DeploymentIdNamePair(null, name));
            }

            var valueSet = env.Runtime.VariableService.GetVariableValue(nameSet);
            for (var i = 0; i < names.Length; i++) {
                Assert.AreEqual(values[i], valueSet.Get(new DeploymentIdNamePair(null, names[i])));
            }
        }

        private static void TryInvalidSetAPI(
            RegressionEnvironment env,
            string deploymentId,
            string variableName,
            object newValue)
        {
            try {
                env.Runtime.VariableService.SetVariableValue(deploymentId, variableName, newValue);
                Assert.Fail();
            }
            catch (VariableConstantValueException ex) {
                Assert.AreEqual(
                    ex.Message,
                    "Variable by name '" +
                    variableName +
                    "' is declared as constant and may not be assigned a new value");
            }

            try {
                env.Runtime
                    .VariableService.SetVariableValue(
                        Collections.SingletonMap(new DeploymentIdNamePair(deploymentId, variableName), newValue));
                Assert.Fail();
            }
            catch (VariableConstantValueException ex) {
                Assert.AreEqual(
                    ex.Message,
                    "Variable by name '" +
                    variableName +
                    "' is declared as constant and may not be assigned a new value");
            }
        }

        private static void TryOperator(
            RegressionEnvironment env,
            RegressionPath path,
            string @operator,
            object[][] testdata)
        {
            env.CompileDeploy(
                "@Name('s0') select TheString as c0,intPrimitive as c1 from SupportBean(" + @operator + ")",
                path);
            env.AddListener("s0");

            // initiate
            env.SendEventBean(new SupportBean_S0(10, "S01"));

            for (var i = 0; i < testdata.Length; i++) {
                var bean = new SupportBean();
                var testValue = testdata[i][0];
                if (testValue is int?) {
                    bean.IntBoxed = (int?) testValue;
                }
                else if (testValue is SupportEnum) {
                    bean.EnumValue = (SupportEnum) testValue;
                }
                else {
                    bean.ShortBoxed = testValue.AsShort();
                }

                var expected = (bool) testdata[i][1];

                env.SendEventBean(bean);
                Assert.AreEqual(expected, env.Listener("s0").GetAndClearIsInvoked(), "Failed at " + i);
            }

            // assert type of expression
            var item = SupportFilterHelper.GetFilterSingle(env.Statement("s0"));
            Assert.IsTrue(item.Op != FilterOperator.BOOLEAN_EXPRESSION);

            env.UndeployModuleContaining("s0");
        }

        public static int GetValue(MyEnumWithOverride value)
        {
            switch (value) {
                case MyEnumWithOverride.LONG:
                    return 1;

                case MyEnumWithOverride.SHORT:
                    return -1;

                default:
                    throw new ArgumentException();
            }
        }

        internal class EPLVariableFilterConstantCustomTypePreconfigured : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from MyVariableCustomEvent(name=my_variable_custom_typed)")
                    .AddListener("s0");

                env.SendEventBean(new MyVariableCustomEvent(MyVariableCustomType.Of("abc")));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLVariableSimplePreconfigured : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select var_simple_preconfig_const as c0 from SupportBean")
                    .AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 0));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.Milestone(1);

                env.UndeployAll();
            }
        }

        internal class EPLVariableSimpleSameModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create variable boolean var_simple_module_const = true;\n" +
                          "@Name('s0') select var_simple_module_const as c0 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.Milestone(0);
                env.SendEventBean(new SupportBean("E1", 0));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
                env.UndeployAll();
            }
        }

        internal class EPLVariableSimpleTwoModules : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create variable boolean var_simple_twomodule_const = true", path);
                env.CompileDeploy("@Name('s0') select var_simple_twomodule_const as c0 from SupportBean", path);
                env.AddListener("s0");
                env.Milestone(0);
                env.SendEventBean(new SupportBean("E1", 0));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
                env.UndeployAll();
            }
        }

        internal class EPLVariableSimpleSet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create variable boolean var_simple_set = true;\n" +
                          "@Name('set') on SupportBean_S0 set var_simple_set = false;\n" +
                          "@Name('s0') select var_simple_set as c0 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 0));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.UndeployAll();
            }
        }

        internal class CustomSubscriber
        {
            private readonly CountDownLatch latch;
            private readonly IList<string> values;

            public CustomSubscriber(
                CountDownLatch latch,
                IList<string> values)
            {
                this.latch = latch;
                this.values = values;
            }

            public void Update(IDictionary<string, object> @event)
            {
                var value = (string) @event.Get("c0");
                values.Add(value);
                latch.CountDown();
            }
        }

        internal class EPLVariableDotVariableSeparateThread : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.Runtime.VariableService.SetVariableValue(
                    null,
                    "mySimpleVariableService",
                    new MySimpleVariableService());

                var epStatement = env
                    .CompileDeploy("@Name('s0') select mySimpleVariableService.doSomething() as c0 from SupportBean")
                    .Statement("s0");

                var latch = new CountDownLatch(1);
                IList<string> values = new List<string>();
                epStatement.SetSubscriber(new CustomSubscriber(latch, values));

                var executorService = Executors.NewSingleThreadExecutor();
                executorService.Submit(() => env.SendEventBean(new SupportBean()));

                try {
                    latch.Await();
                }
                catch (ThreadInterruptedException e) {
                    Assert.Fail();
                }

                executorService.Shutdown();

                Assert.AreEqual(1, values.Count);
                Assert.AreEqual("hello", values[0]);

                env.UndeployAll();
            }
        }

        internal class EPLVariableInvokeMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // declared via EPL
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create constant variable MySimpleVariableService myService = MySimpleVariableServiceFactory.makeService()",
                    path);

                // exercise
                var epl = "@Name('s0') select " +
                          "myService.doSomething() as c0, " +
                          "myInitService.doSomething() as c1 " +
                          "from SupportBean";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1".SplitCsv(),
                    new object[] {"hello", "hello"});

                env.UndeployAll();
            }
        }

        internal class EPLVariableConstantVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                env.CompileDeploy("create const variable int MYCONST = 10", path);
                TryOperator(
                    env,
                    path,
                    "MYCONST = intBoxed",
                    new[] {new object[] {10, true}, new object[] {9, false}, new object[] {null, false}});

                TryOperator(
                    env,
                    path,
                    "MYCONST > intBoxed",
                    new[] {
                        new object[] {11, false}, new object[] {10, false}, new object[] {9, true},
                        new object[] {8, true}
                    });
                TryOperator(
                    env,
                    path,
                    "MYCONST >= intBoxed",
                    new[] {
                        new object[] {11, false}, new object[] {10, true}, new object[] {9, true},
                        new object[] {8, true}
                    });
                TryOperator(
                    env,
                    path,
                    "MYCONST < intBoxed",
                    new[] {
                        new object[] {11, true}, new object[] {10, false}, new object[] {9, false},
                        new object[] {8, false}
                    });
                TryOperator(
                    env,
                    path,
                    "MYCONST <= intBoxed",
                    new[] {
                        new object[] {11, true}, new object[] {10, true}, new object[] {9, false},
                        new object[] {8, false}
                    });

                TryOperator(
                    env,
                    path,
                    "intBoxed < MYCONST",
                    new[] {
                        new object[] {11, false}, new object[] {10, false}, new object[] {9, true},
                        new object[] {8, true}
                    });
                TryOperator(
                    env,
                    path,
                    "intBoxed <= MYCONST",
                    new[] {
                        new object[] {11, false}, new object[] {10, true}, new object[] {9, true},
                        new object[] {8, true}
                    });
                TryOperator(
                    env,
                    path,
                    "intBoxed > MYCONST",
                    new[] {
                        new object[] {11, true}, new object[] {10, false}, new object[] {9, false},
                        new object[] {8, false}
                    });
                TryOperator(
                    env,
                    path,
                    "intBoxed >= MYCONST",
                    new[] {
                        new object[] {11, true}, new object[] {10, true}, new object[] {9, false},
                        new object[] {8, false}
                    });

                TryOperator(
                    env,
                    path,
                    "intBoxed in (MYCONST)",
                    new[] {
                        new object[] {11, false}, new object[] {10, true}, new object[] {9, false},
                        new object[] {8, false}
                    });
                TryOperator(
                    env,
                    path,
                    "intBoxed between MYCONST and MYCONST",
                    new[] {
                        new object[] {11, false}, new object[] {10, true}, new object[] {9, false},
                        new object[] {8, false}
                    });

                TryOperator(
                    env,
                    path,
                    "MYCONST != intBoxed",
                    new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, false}});
                TryOperator(
                    env,
                    path,
                    "intBoxed != MYCONST",
                    new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, false}});

                TryOperator(
                    env,
                    path,
                    "intBoxed not in (MYCONST)",
                    new[] {
                        new object[] {11, true}, new object[] {10, false}, new object[] {9, true},
                        new object[] {8, true}
                    });
                TryOperator(
                    env,
                    path,
                    "intBoxed not between MYCONST and MYCONST",
                    new[] {
                        new object[] {11, true}, new object[] {10, false}, new object[] {9, true},
                        new object[] {8, true}
                    });

                TryOperator(
                    env,
                    path,
                    "MYCONST is intBoxed",
                    new[] {new object[] {10, true}, new object[] {9, false}, new object[] {null, false}});
                TryOperator(
                    env,
                    path,
                    "intBoxed is MYCONST",
                    new[] {new object[] {10, true}, new object[] {9, false}, new object[] {null, false}});

                TryOperator(
                    env,
                    path,
                    "MYCONST is not intBoxed",
                    new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, true}});
                TryOperator(
                    env,
                    path,
                    "intBoxed is not MYCONST",
                    new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, true}});

                // try coercion
                TryOperator(
                    env,
                    path,
                    "MYCONST = shortBoxed",
                    new[] {
                        new object[] {(short) 10, true}, new object[] {(short) 9, false}, new object[] {null, false}
                    });
                TryOperator(
                    env,
                    path,
                    "shortBoxed = MYCONST",
                    new[] {
                        new object[] {(short) 10, true}, new object[] {(short) 9, false}, new object[] {null, false}
                    });

                TryOperator(
                    env,
                    path,
                    "MYCONST > shortBoxed",
                    new[] {
                        new object[] {(short) 11, false}, new object[] {(short) 10, false},
                        new object[] {(short) 9, true},
                        new object[] {(short) 8, true}
                    });
                TryOperator(
                    env,
                    path,
                    "shortBoxed < MYCONST",
                    new[] {
                        new object[] {(short) 11, false}, new object[] {(short) 10, false},
                        new object[] {(short) 9, true},
                        new object[] {(short) 8, true}
                    });

                TryOperator(
                    env,
                    path,
                    "shortBoxed in (MYCONST)",
                    new[] {
                        new object[] {(short) 11, false}, new object[] {(short) 10, true},
                        new object[] {(short) 9, false},
                        new object[] {(short) 8, false}
                    });

                // test SODA
                env.UndeployAll();

                var epl = "@Name('variable') create constant variable int MYCONST = 10";
                env.EplToModelCompileDeploy(epl);

                // test invalid
                TryInvalidCompile(
                    env,
                    path,
                    "on SupportBean set MYCONST = 10",
                    "Variable by name 'MYCONST' is declared constant and may not be set [on SupportBean set MYCONST = 10]");
                TryInvalidCompile(
                    env,
                    path,
                    "select * from SupportBean output when true then set MYCONST=1",
                    "Error in the output rate limiting clause: Variable by name 'MYCONST' is declared constant and may not be set [select * from SupportBean output when true then set MYCONST=1]");

                // assure no update via API
                TryInvalidSetAPI(env, env.DeploymentId("variable"), "MYCONST", 1);

                // add constant variable via config API
                TryInvalidSetAPI(env, null, "MYCONST_TWO", "dummy");
                TryInvalidSetAPI(env, null, "MYCONST_THREE", false);

                // try ESPER-653
                env.CompileDeploy(
                    "@Name('s0') create constant variable java.util.Date START_TIME = java.util.Calendar.getInstance().getTime()");
                var value = env.GetEnumerator("s0").Advance().Get("START_TIME");
                Assert.IsNotNull(value);
                env.UndeployModuleContaining("s0");

                // test array constant
                env.UndeployAll();
                env.CompileDeploy("create constant variable string[] var_strings = {'E1', 'E2'}", path);
                env.CompileDeploy("@Name('s0') select var_strings from SupportBean", path);
                Assert.AreEqual(typeof(string[]), env.Statement("s0").EventType.GetPropertyType("var_strings"));
                env.UndeployModuleContaining("s0");

                TryAssertionArrayVar(env, path, "var_strings");

                TryOperator(
                    env,
                    path,
                    "intBoxed in (10, 8)",
                    new[] {
                        new object[] {11, false}, new object[] {10, true}, new object[] {9, false},
                        new object[] {8, true}
                    });

                env.CompileDeploy("create constant variable int [ ] var_ints = {8, 10}", path);
                TryOperator(
                    env,
                    path,
                    "intBoxed in (var_ints)",
                    new[] {
                        new object[] {11, false}, new object[] {10, true}, new object[] {9, false},
                        new object[] {8, true}
                    });

                env.CompileDeploy("create constant variable int[]  var_intstwo = {9}", path);
                TryOperator(
                    env,
                    path,
                    "intBoxed in (var_ints, var_intstwo)",
                    new[] {
                        new object[] {11, false}, new object[] {10, true}, new object[] {9, true},
                        new object[] {8, true}
                    });

                TryInvalidCompile(
                    env,
                    "create constant variable SupportBean[] var_beans",
                    "Cannot create variable 'var_beans', type 'SupportBean' cannot be declared as an array type [create constant variable SupportBean[] var_beans]");

                // test array of primitives
                env.CompileDeploy("@Name('s0') create variable byte[] myBytesBoxed");
                object[][] expectedType = {
                    new object[] {"myBytesBoxed", typeof(byte[])}
                };
                SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                    expectedType,
                    env.Statement("s0").EventType,
                    SupportEventTypeAssertionEnum.NAME,
                    SupportEventTypeAssertionEnum.TYPE);
                env.UndeployModuleContaining("s0");

                env.CompileDeploy("@Name('s0') create variable byte[primitive] myBytesPrimitive");
                expectedType = new[] {
                    new object[] {"myBytesPrimitive", typeof(byte[])}
                };
                SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                    expectedType,
                    env.Statement("s0").EventType,
                    SupportEventTypeAssertionEnum.NAME,
                    SupportEventTypeAssertionEnum.TYPE);
                env.UndeployAll();

                // test enum constant
                env.CompileDeploy("create constant variable SupportEnum var_enumone = SupportEnum.ENUM_VALUE_2", path);
                TryOperator(
                    env,
                    path,
                    "var_enumone = enumValue",
                    new[] {
                        new object[] {SupportEnum.ENUM_VALUE_3, false}, new object[] {SupportEnum.ENUM_VALUE_2, true},
                        new object[] {SupportEnum.ENUM_VALUE_1, false}
                    });

                env.CompileDeploy(
                    "create constant variable SupportEnum[] var_enumarr = {SupportEnum.ENUM_VALUE_2, SupportEnum.ENUM_VALUE_1}",
                    path);
                TryOperator(
                    env,
                    path,
                    "enumValue in (var_enumarr, var_enumone)",
                    new[] {
                        new object[] {SupportEnum.ENUM_VALUE_3, false}, new object[] {SupportEnum.ENUM_VALUE_2, true},
                        new object[] {SupportEnum.ENUM_VALUE_1, true}
                    });

                env.CompileDeploy("create variable SupportEnum var_enumtwo = SupportEnum.ENUM_VALUE_2", path);
                env.CompileDeploy("on SupportBean set var_enumtwo = enumValue", path);

                env.UndeployAll();
            }

            private static void TryAssertionArrayVar(
                RegressionEnvironment env,
                RegressionPath path,
                string varName)
            {
                env.CompileDeploy("@Name('s0') select * from SupportBean(theString in (" + varName + "))", path)
                    .AddListener("s0");

                SendBeanAssert(env, "E1", true);
                SendBeanAssert(env, "E2", true);
                SendBeanAssert(env, "E3", false);

                env.UndeployAll();
            }

            private static void SendBeanAssert(
                RegressionEnvironment env,
                string theString,
                bool expected)
            {
                env.SendEventBean(new SupportBean(theString, 1));
                Assert.AreEqual(expected, env.Listener("s0").GetAndClearIsInvoked());
            }
        }

        internal class EPLVariableEPRuntime : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var runtimeSPI = (EPVariableServiceSPI) env.Runtime.VariableService;
                var types = runtimeSPI.VariableTypeAll;
                Assert.AreEqual(typeof(int?), types.Get(new DeploymentIdNamePair(null, "var1")));
                Assert.AreEqual(typeof(string), types.Get(new DeploymentIdNamePair(null, "var2")));

                Assert.AreEqual(typeof(int?), runtimeSPI.GetVariableType(null, "var1"));
                Assert.AreEqual(typeof(string), runtimeSPI.GetVariableType(null, "var2"));

                var stmtTextSet = "on SupportBean set var1 = intPrimitive, var2 = theString";
                env.CompileDeploy(stmtTextSet);

                AssertVariableValuesPreconfigured(
                    env,
                    new[] {"var1", "var2"},
                    new object[] {-1, "abc"});
                SendSupportBean(env, null, 99);
                AssertVariableValuesPreconfigured(
                    env,
                    new[] {"var1", "var2"},
                    new object[] {99, null});

                env.Runtime.VariableService.SetVariableValue(null, "var2", "def");
                AssertVariableValuesPreconfigured(
                    env,
                    new[] {"var1", "var2"},
                    new object[] {99, "def"});

                env.Milestone(0);

                AssertVariableValuesPreconfigured(
                    env,
                    new[] {"var1", "var2"},
                    new object[] {99, "def"});
                env.Runtime.VariableService.SetVariableValue(null, "var1", 123);
                AssertVariableValuesPreconfigured(
                    env,
                    new[] {"var1", "var2"},
                    new object[] {123, "def"});

                env.Milestone(1);

                AssertVariableValuesPreconfigured(
                    env,
                    new[] {"var1", "var2"},
                    new object[] {123, "def"});
                IDictionary<DeploymentIdNamePair, object> newValues = new Dictionary<DeploymentIdNamePair, object>();
                newValues.Put(new DeploymentIdNamePair(null, "var1"), 20);
                env.Runtime.VariableService.SetVariableValue(newValues);
                AssertVariableValuesPreconfigured(
                    env,
                    new[] {"var1", "var2"},
                    new object[] {20, "def"});

                newValues.Put(new DeploymentIdNamePair(null, "var1"), (byte) 21);
                newValues.Put(new DeploymentIdNamePair(null, "var2"), "test");
                env.Runtime.VariableService.SetVariableValue(newValues);
                AssertVariableValuesPreconfigured(
                    env,
                    new[] {"var1", "var2"},
                    new object[] {21, "test"});

                newValues.Put(new DeploymentIdNamePair(null, "var1"), null);
                newValues.Put(new DeploymentIdNamePair(null, "var2"), null);
                env.Runtime.VariableService.SetVariableValue(newValues);
                AssertVariableValuesPreconfigured(
                    env,
                    new[] {"var1", "var2"},
                    new object[] {null, null});

                // try variable not found
                try {
                    env.Runtime.VariableService.SetVariableValue(null, "dummy", null);
                    Assert.Fail();
                }
                catch (VariableNotFoundException ex) {
                    // expected
                    Assert.AreEqual("Variable by name 'dummy' has not been declared", ex.Message);
                }

                // try variable not found
                try {
                    newValues.Put(new DeploymentIdNamePair(null, "dummy2"), 20);
                    env.Runtime.VariableService.SetVariableValue(newValues);
                    Assert.Fail();
                }
                catch (VariableNotFoundException ex) {
                    // expected
                    Assert.AreEqual("Variable by name 'dummy2' has not been declared", ex.Message);
                }

                // create new variable on the fly
                env.CompileDeploy("@Name('create') create variable int dummy = 20 + 20");
                Assert.AreEqual(40, env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "dummy"));

                // try type coercion
                try {
                    env.Runtime.VariableService.SetVariableValue(env.DeploymentId("create"), "dummy", "abc");
                    Assert.Fail();
                }
                catch (VariableValueException ex) {
                    // expected
                    Assert.AreEqual(
                        "Variable 'dummy' of declared type System.Integer cannot be assigned a value of type System.String",
                        ex.Message);
                }

                try {
                    env.Runtime.VariableService.SetVariableValue(env.DeploymentId("create"), "dummy", 100L);
                    Assert.Fail();
                }
                catch (VariableValueException ex) {
                    // expected
                    Assert.AreEqual(
                        "Variable 'dummy' of declared type System.Integer cannot be assigned a value of type System.Long",
                        ex.Message);
                }

                try {
                    env.Runtime.VariableService.SetVariableValue(null, "var2", 0);
                    Assert.Fail();
                }
                catch (VariableValueException ex) {
                    // expected
                    Assert.AreEqual(
                        "Variable 'var2' of declared type System.String cannot be assigned a value of type System.Integer",
                        ex.Message);
                }

                // coercion
                env.Runtime.VariableService.SetVariableValue(null, "var1", (short) -1);
                AssertVariableValuesPreconfigured(
                    env,
                    new[] {"var1", "var2"},
                    new object[] {-1, null});

                // rollback for coercion failed
                newValues = new LinkedHashMap<DeploymentIdNamePair, object>(); // preserve order
                newValues.Put(new DeploymentIdNamePair(null, "var2"), "xyz");
                newValues.Put(new DeploymentIdNamePair(null, "var1"), 4.4d);
                try {
                    env.Runtime.VariableService.SetVariableValue(newValues);
                    Assert.Fail();
                }
                catch (VariableValueException ex) {
                    // expected
                }

                AssertVariableValuesPreconfigured(
                    env,
                    new[] {"var1", "var2"},
                    new object[] {-1, null});

                // rollback for variable not found
                newValues = new LinkedHashMap<DeploymentIdNamePair, object>(); // preserve order
                newValues.Put(new DeploymentIdNamePair(null, "var2"), "xyz");
                newValues.Put(new DeploymentIdNamePair(null, "var1"), 1);
                newValues.Put(new DeploymentIdNamePair(null, "notfoundvariable"), null);
                try {
                    env.Runtime.VariableService.SetVariableValue(newValues);
                    Assert.Fail();
                }
                catch (VariableNotFoundException) {
                    // expected
                }

                AssertVariableValuesPreconfigured(
                    env,
                    new[] {"var1", "var2"},
                    new object[] {-1, null});

                env.UndeployAll();
            }
        }

        internal class EPLVariableSetSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet =
                    "@Name('s0') on SupportBean_S0 as s0str set var1SS = (select p10 from SupportBean_S1#lastevent), var2SS = (select p11||s0str.p01 from SupportBean_S1#lastevent)";
                env.CompileDeploy(stmtTextSet);
                string[] fieldsVar = {"var1SS", "var2SS"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsVar,
                    new[] {new object[] {"a", "b"}});

                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsVar,
                    new[] {new object[] {null, null}});

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(0, "x", "y"));
                env.SendEventBean(new SupportBean_S0(1, "1", "2"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsVar,
                    new[] {new object[] {"x", "y2"}});

                env.UndeployAll();
            }
        }

        internal class EPLVariableVariableInFilterBoolean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet = "@Name('set') on SupportBean_S0 set var1IFB = p00, var2IFB = p01";
                env.CompileDeploy(stmtTextSet).AddListener("set");
                string[] fieldsVar = {"var1IFB", "var2IFB"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {null, null}});

                var stmtTextSelect =
                    "@Name('s0') select TheString, intPrimitive from SupportBean(theString = var1IFB or theString = var2IFB)";
                string[] fieldsSelect = {"TheString", "IntPrimitive"};
                env.CompileDeploy(stmtTextSelect).AddListener("s0");

                SendSupportBean(env, null, 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                SendSupportBeanS0NewThread(env, 100, "a", "b");
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {"a", "b"});

                SendSupportBean(env, "a", 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {"a", 2});

                SendSupportBean(env, null, 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                SendSupportBean(env, "b", 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {"b", 3});

                SendSupportBean(env, "c", 4);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                SendSupportBeanS0NewThread(env, 100, "e", "c");
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {"e", "c"});

                SendSupportBean(env, "c", 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {"c", 5});

                SendSupportBean(env, "e", 6);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {"e", 6});

                env.UndeployAll();
            }
        }

        internal class EPLVariableVariableInFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet = "@Name('set') on SupportBean_S0 set var1IF = p00";
                env.CompileDeploy(stmtTextSet).AddListener("set");
                string[] fieldsVar = {"var1IF"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {null}});

                var stmtTextSelect = "@Name('s0') select TheString, intPrimitive from SupportBean(theString = var1IF)";
                string[] fieldsSelect = {"TheString", "IntPrimitive"};
                env.CompileDeploy(stmtTextSelect).AddListener("s0");

                SendSupportBean(env, null, 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendSupportBeanS0NewThread(env, 100, "a", "b");
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {"a"});

                SendSupportBean(env, "a", 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {"a", 2});

                env.Milestone(0);

                SendSupportBean(env, null, 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendSupportBeanS0NewThread(env, 100, "e", "c");
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {"e"});

                env.Milestone(1);

                SendSupportBean(env, "c", 5);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendSupportBean(env, "e", 6);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {"e", 6});

                env.UndeployAll();
            }
        }

        internal class EPLVariableAssignmentOrderNoDup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet =
                    "@Name('set') on SupportBean set var1OND = intPrimitive, var2OND = var1OND + 1, var3OND = var1OND + var2OND";
                env.CompileDeploy(stmtTextSet).AddListener("set");
                string[] fieldsVar = {"var1OND", "var2OND", "var3OND"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {12, 2, null}});

                SendSupportBean(env, "S1", 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {3, 4, 7});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {3, 4, 7}});

                env.Milestone(0);

                SendSupportBean(env, "S1", -1);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {-1, 0, -1});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {-1, 0, -1}});

                SendSupportBean(env, "S1", 90);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {90, 91, 181});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {90, 91, 181}});

                env.UndeployAll();
            }
        }

        internal class EPLVariableAssignmentOrderDup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet =
                    "@Name('set') on SupportBean set var1OD = intPrimitive, var2OD = var2OD, var1OD = intBoxed, var3OD = var3OD + 1";
                env.CompileDeploy(stmtTextSet).AddListener("set");
                string[] fieldsVar = {"var1OD", "var2OD", "var3OD"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {0, 1, 2}});

                SendSupportBean(env, "S1", -1, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {10, 1, 3});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {10, 1, 3}});

                SendSupportBean(env, "S2", -2, 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {20, 1, 4});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {20, 1, 4}});

                env.Milestone(0);

                SendSupportBeanNewThread(env, "S3", -3, 30);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {30, 1, 5});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {30, 1, 5}});

                SendSupportBeanNewThread(env, "S4", -4, 40);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {40, 1, 6});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {40, 1, 6}});

                env.UndeployAll();
            }
        }

        internal class EPLVariableObjectModel : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("var1OM", "var2OM", "id");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_A"));

                var path = new RegressionPath();
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model, path);
                var stmtText = "@Name('s0') select var1OM, var2OM, id from SupportBean_A";
                Assert.AreEqual(stmtText, model.ToEPL());
                env.AddListener("s0");

                string[] fieldsSelect = {"var1OM", "var2OM", "id"};
                SendSupportBean_A(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {10d, 11L, "E1"});

                model = new EPStatementObjectModel();
                model.OnExpr = OnClause.CreateOnSet(
                        Expressions.Eq(Expressions.Property("var1OM"), Expressions.Property("IntPrimitive")))
                    .AddAssignment(Expressions.Eq(Expressions.Property("var2OM"), Expressions.Property("IntBoxed")));
                model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).Name));
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("set"));
                var stmtTextSet = "@Name('set') on SupportBean set var1OM=intPrimitive, var2OM=intBoxed";
                env.CompileDeploy(model, path).AddListener("set");
                Assert.AreEqual(stmtTextSet, model.ToEPL());

                var typeSet = env.Statement("set").EventType;
                Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("var1OM"));
                Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var2OM"));
                Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
                string[] fieldsVar = {"var1OM", "var2OM"};
                EPAssertionUtil.AssertEqualsAnyOrder(fieldsVar, typeSet.PropertyNames);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {10d, 11L}});
                SendSupportBean(env, "S1", 3, 4);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {3d, 4L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {3d, 4L}});

                SendSupportBean_A(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {3d, 4L, "E2"});

                env.UndeployModuleContaining("set");
                env.UndeployModuleContaining("s0");
            }
        }

        internal class EPLVariableSimpleSetSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                var textVar = "@Name('s0_0') create variable int resvar = 1";
                env.CompileDeploy(textVar, path).AddListener("s0_0");
                string[] fieldsVarOne = {"resvar"};

                textVar = "@Name('s0_1') create variable int durvar = 10";
                env.CompileDeploy(textVar, path).AddListener("s0_1");
                string[] fieldsVarTwo = {"durvar"};

                var textSet = "@Name('s1') on SupportBean set resvar = intPrimitive, durvar = intPrimitive";
                env.CompileDeploy(textSet, path).AddListener("s1");
                string[] fieldsVarSet = {"resvar", "durvar"};

                var textSelect = "@Name('s2') select irstream resvar, durvar, symbol from SupportMarketDataBean";
                env.CompileDeploy(textSelect, path).AddListener("s2");
                string[] fieldsSelect = {"resvar", "durvar", "symbol"};

                env.Milestone(0);

                // read values
                SendMarketDataEvent(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {1, 10, "E1"});

                env.Milestone(1);

                // set new value
                SendSupportBean(env, 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0_0").LastNewData[0],
                    fieldsVarOne,
                    new object[] {20});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0_1").LastNewData[0],
                    fieldsVarTwo,
                    new object[] {20});
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fieldsVarSet,
                    new object[] {20, 20});
                env.Listener("s0_0").Reset();

                env.Milestone(2);

                // read values
                SendMarketDataEvent(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {20, 20, "E2"});

                env.Milestone(3);

                // set new value
                SendSupportBean(env, 1000);

                env.Milestone(4);

                // read values
                SendMarketDataEvent(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {1000, 1000, "E3"});

                env.Milestone(5);

                env.UndeployModuleContaining("s1");
                env.UndeployModuleContaining("s2");
                env.UndeployModuleContaining("s0_0");
                env.UndeployModuleContaining("s0_1");
            }

            private static void SendMarketDataEvent(
                RegressionEnvironment env,
                string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, null);
                env.SendEventBean(bean);
            }

            private static void SendSupportBean(
                RegressionEnvironment env,
                int intPrimitive)
            {
                var bean = new SupportBean("", intPrimitive);
                env.SendEventBean(bean);
            }
        }

        internal class EPLVariableCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select var1C, var2C, id from SupportBean_A";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0");

                string[] fieldsSelect = {"var1C", "var2C", "id"};
                SendSupportBean_A(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {10d, 11L, "E1"});

                var stmtTextSet = "@Name('set') on SupportBean set var1C=intPrimitive, var2C=intBoxed";
                env.EplToModelCompileDeploy(stmtTextSet).AddListener("set");

                var typeSet = env.Statement("set").EventType;
                Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("var1C"));
                Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var2C"));
                Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
                string[] fieldsVar = {"var1C", "var2C"};
                EPAssertionUtil.AssertEqualsAnyOrder(fieldsVar, typeSet.PropertyNames);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {10d, 11L}});
                SendSupportBean(env, "S1", 3, 4);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {3d, 4L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {3d, 4L}});

                SendSupportBean_A(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {3d, 4L, "E2"});

                env.UndeployModuleContaining("set");
                env.UndeployModuleContaining("s0");
            }
        }

        internal class EPLVariableRuntimeConfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select var1RTC, theString from SupportBean(theString like 'E%')";
                env.CompileDeploy(stmtText).AddListener("s0");

                string[] fieldsSelect = {"var1RTC", "TheString"};
                SendSupportBean(env, "E1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {10, "E1"});

                env.Milestone(0);

                SendSupportBean(env, "E2", 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {10, "E2"});

                var stmtTextSet = "@Name('set') on SupportBean(theString like 'S%') set var1RTC = intPrimitive";
                env.CompileDeploy(stmtTextSet).AddListener("set");

                var typeSet = env.Statement("set").EventType;
                Assert.AreEqual(typeof(int?), typeSet.GetPropertyType("var1RTC"));
                Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
                Assert.IsTrue(Equals(typeSet.PropertyNames, new[] {"var1RTC"}));

                string[] fieldsVar = {"var1RTC"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {10}});

                SendSupportBean(env, "S1", 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {3});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {3}});

                env.Milestone(0);

                SendSupportBean(env, "E3", 4);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {3, "E3"});

                SendSupportBean(env, "S2", -1);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {-1});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {-1}});

                SendSupportBean(env, "E4", 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {-1, "E4"});

                env.UndeployAll();
            }
        }

        internal class EPLVariableRuntimeOrderMultiple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet =
                    "@Name('set') on SupportBean(theString like 'S%' or theString like 'B%') set var1ROM = intPrimitive, var2ROM = intBoxed";
                env.CompileDeploy(stmtTextSet).AddListener("set");
                string[] fieldsVar = {"var1ROM", "var2ROM"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {null, 1}});

                var typeSet = env.Statement("set").EventType;
                Assert.AreEqual(typeof(int?), typeSet.GetPropertyType("var1ROM"));
                Assert.AreEqual(typeof(int?), typeSet.GetPropertyType("var2ROM"));
                Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
                EPAssertionUtil.AssertEqualsAnyOrder(new[] {"var1ROM", "var2ROM"}, typeSet.PropertyNames);

                SendSupportBean(env, "S1", 3, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {3, null});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {3, null}});

                env.Milestone(0);

                SendSupportBean(env, "S1", -1, -2);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {-1, -2});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {-1, -2}});

                var stmtText =
                    "@Name('s0') select var1ROM, var2ROM, theString from SupportBean(theString like 'E%' or theString like 'B%')";
                env.CompileDeploy(stmtText).AddListener("s0");
                string[] fieldsSelect = {"var1ROM", "var2ROM", "TheString"};
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fieldsSelect, null);

                env.Milestone(1);

                SendSupportBean(env, "E1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {-1, -2, "E1"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {-1, -2}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsSelect,
                    new[] {new object[] {-1, -2, "E1"}});

                SendSupportBean(env, "S1", 11, 12);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {11, 12});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {11, 12}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsSelect,
                    new[] {new object[] {11, 12, "E1"}});

                SendSupportBean(env, "E2", 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {11, 12, "E2"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsSelect,
                    new[] {new object[] {11, 12, "E2"}});

                env.UndeployAll();
            }
        }

        internal class EPLVariableOnSetWithFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet =
                    "@Name('set') on SupportBean(theString like 'S%') set papi_1 = 'end', papi_2 = false, papi_3 = null";
                env.CompileDeploy(stmtTextSet).AddListener("set");
                string[] fieldsVar = {"papi_1", "papi_2", "papi_3"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {"begin", true, "value"}});

                var typeSet = env.Statement("set").EventType;
                Assert.AreEqual(typeof(string), typeSet.GetPropertyType("papi_1"));
                Assert.AreEqual(typeof(bool?), typeSet.GetPropertyType("papi_2"));
                Assert.AreEqual(typeof(string), typeSet.GetPropertyType("papi_3"));
                Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
                Array.Sort(typeSet.PropertyNames);
                Assert.IsTrue(Equals(typeSet.PropertyNames, fieldsVar));

                SendSupportBean(env, "S1", 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {"end", false, null});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {"end", false, null}});

                env.Milestone(0);

                SendSupportBean(env, "S2", 4);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {"end", false, null});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {"end", false, null}});

                env.UndeployAll();
            }
        }

        internal class EPLVariableCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet =
                    "@Name('set') on SupportBean set var1COE = intPrimitive, var2COE = intPrimitive, var3COE=intBoxed";
                env.CompileDeploy(stmtTextSet).AddListener("set");
                string[] fieldsVar = {"var1COE", "var2COE", "var3COE"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {null, null, null}});

                var typeSet = env.Statement("set").EventType;
                Assert.AreEqual(typeof(float?), typeSet.GetPropertyType("var1COE"));
                Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("var2COE"));
                Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var3COE"));
                Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
                EPAssertionUtil.AssertEqualsAnyOrder(typeSet.PropertyNames, fieldsVar);

                var stmtText = "@Name('s0') select irstream var1COE, var2COE, var3COE, id from SupportBean_A#length(2)";
                env.CompileDeploy(stmtText).AddListener("s0");
                string[] fieldsSelect = {"var1COE", "var2COE", "var3COE", "id"};
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fieldsSelect, null);

                SendSupportBean_A(env, "A1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {null, null, null, "A1"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsSelect,
                    new[] {new object[] {null, null, null, "A1"}});

                SendSupportBean(env, "S1", 1, 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {1f, 1d, 2L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {1f, 1d, 2L}});

                env.Milestone(0);

                SendSupportBean_A(env, "A2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {1f, 1d, 2L, "A2"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsSelect,
                    new[] {new object[] {1f, 1d, 2L, "A1"}, new object[] {1f, 1d, 2L, "A2"}});

                SendSupportBean(env, "S1", 10, 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {10f, 10d, 20L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("set"),
                    fieldsVar,
                    new[] {new object[] {10f, 10d, 20L}});

                SendSupportBean_A(env, "A3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fieldsSelect,
                    new object[] {10f, 10d, 20L, "A3"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fieldsSelect,
                    new object[] {10f, 10d, 20L, "A1"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsSelect,
                    new[] {new object[] {10f, 10d, 20L, "A2"}, new object[] {10f, 10d, 20L, "A3"}});

                env.UndeployAll();
            }
        }

        internal class EPLVariableInvalidSet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "on SupportBean set dummy = 100",
                    "Variable by name 'dummy' has not been created or configured");

                TryInvalidCompile(
                    env,
                    "on SupportBean set var1IS = 1",
                    "Variable 'var1IS' of declared type System.String cannot be assigned a value of type int");

                TryInvalidCompile(
                    env,
                    "on SupportBean set var3IS = 'abc'",
                    "Variable 'var3IS' of declared type System.Integer cannot be assigned a value of type System.String");

                TryInvalidCompile(
                    env,
                    "on SupportBean set var3IS = doublePrimitive",
                    "Variable 'var3IS' of declared type System.Integer cannot be assigned a value of type System.Double");

                TryInvalidCompile(env, "on SupportBean set var2IS = 'false'", "skip");
                TryInvalidCompile(env, "on SupportBean set var3IS = 1.1", "skip");
                TryInvalidCompile(env, "on SupportBean set var3IS = 22222222222222", "skip");
                TryInvalidCompile(
                    env,
                    "on SupportBean set var3IS",
                    "Missing variable assignment expression in assignment number 0 [");
            }
        }

        [Serializable]
        public class A
        {
            public string Value => "";
        }

        public class B
        {
        }

        public class MySimpleVariableServiceFactory
        {
            public static MySimpleVariableService MakeService()
            {
                return new MySimpleVariableService();
            }
        }

        public class MySimpleVariableService
        {
            public string DoSomething()
            {
                return "hello";
            }
        }

        public class MyVariableCustomEvent
        {
            internal MyVariableCustomEvent(MyVariableCustomType name)
            {
                Name = name;
            }

            public MyVariableCustomType Name { get; }
        }

        public class MyVariableCustomType
        {
            internal MyVariableCustomType(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public static MyVariableCustomType Of(string name)
            {
                return new MyVariableCustomType(name);
            }

            protected bool Equals(MyVariableCustomType other)
            {
                return string.Equals(Name, other.Name);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }

                if (ReferenceEquals(this, obj)) {
                    return true;
                }

                if (obj.GetType() != GetType()) {
                    return false;
                }

                return Equals((MyVariableCustomType) obj);
            }

            public override int GetHashCode()
            {
                return Name != null ? Name.GetHashCode() : 0;
            }
        }
    }
} // end of namespace