///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanSchemaGenericType
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSingleParameter(execs);
            WithTwoParameter(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventBeanSchemaParamsInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoParameter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventBeanSchemaParamsTwoParameter());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleParameter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventBeanSchemaParamsSingleParameter());
            return execs;
        }

        private class EventBeanSchemaParamsInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "create schema MyEvent as " + typeof(MyLocalUnparameterized).FullName + "<Integer>";
                env.TryInvalidCompile(
                    epl,
                    "Number of type parameters mismatch, the class '" +
                    typeof(MyLocalUnparameterized).FullName +
                    "' has 0 type parameters but specified are 1 type parameters");

                epl = "create schema MyEvent as " + typeof(MyLocalOneParameter<>).FullName + "<Integer, String>";
                env.TryInvalidCompile(
                    epl,
                    "Number of type parameters mismatch, the class '" +
                    typeof(MyLocalOneParameter<>).FullName +
                    "' has 1 type parameters but specified are 2 type parameters");

                epl = "create schema MyEvent as " + typeof(MyLocalUnparameterized).FullName + "[]";
                env.TryInvalidCompile(
                    epl,
                    "Array dimensions are not allowed");

                epl = "create schema MyEvent as " + typeof(MyLocalOneParameter<>).FullName + "<Dummy>";
                env.TryInvalidCompile(
                    epl,
                    "Failed to resolve type parameter 0 of type 'Dummy': Could not load class by name 'Dummy', please check imports");

                epl = "create schema MyEvent as " + typeof(MyLocalBoundParameter<>).FullName + "<String>";
                env.TryInvalidCompile(
                    epl,
                    "Bound type parameters 0 named 'T' expects 'java.lang.Number' but receives 'java.lang.String'");

                epl = "create schema MyEvent as " + typeof(MyLocalBoundParameter<>).FullName + "<int>";
                env.TryInvalidCompile(
                    epl,
                    "Failed to resolve type parameter 0 of type 'int': Could not load class by name 'int', please check imports");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class EventBeanSchemaParamsSingleParameter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var single = typeof(SupportBeanParameterizedSingle<>).FullName;

                RunAssertionSingleParam(
                    env,
                    single + "<Integer>",
                    typeof(SupportBeanParameterizedSingle<int?>),
                    typeof(int?),
                    new SupportBeanParameterizedSingle<int?>(10),
                    10);

                RunAssertionSingleParam(
                    env,
                    single + "<String>",
                    typeof(SupportBeanParameterizedSingle<string>),
                    (typeof(string)),
                    new SupportBeanParameterizedSingle<string>("x"),
                    "x");

                var data = "a,b".Split(",");

                RunAssertionSingleParam(
                    env,
                    single + "<String[]>",
                    typeof(SupportBeanParameterizedSingle<string[]>),
                    typeof(string[]),
                    new SupportBeanParameterizedSingle<string[]>(data),
                    data);

                RunAssertionSingleParam(
                    env,
                    single,
                    typeof(SupportBeanParameterizedSingle<object>),
                    typeof(object),
                    new SupportBeanParameterizedSingle<object>(100L),
                    100L);

                Nullable<long> optionalLongValue = 10L;
                RunAssertionSingleParam(
                    env,
                    single + "<" + typeof(long?).CleanName() + ">",
                    typeof(SupportBeanParameterizedSingle<long?>),
                    typeof(long?),
                    new SupportBeanParameterizedSingle<long?>(optionalLongValue),
                    optionalLongValue);

                RunAssertionSingleParam(
                    env,
                    typeof(MyLocalBoundParameter<>).FullName + "<Long>",
                    (typeof(MyLocalBoundParameter<long?>)),
                    (typeof(long?)),
                    new MyLocalBoundParameter<long?>(100L),
                    100L);
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        public class EventBeanSchemaParamsTwoParameter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var two = typeof(SupportBeanParameterizedTwo<,>).FullName;

                RunAssertionTwoParam(
                    env,
                    two + "<Double, String>",
                    typeof(SupportBeanParameterizedTwo<double?, string>),
                    typeof(double?),
                    typeof(string),
                    new SupportBeanParameterizedTwo<double?, string>(10d, "A"),
                    10d,
                    "A");

                var dtx = DateTimeEx.NowUtc();
                var buf = new ByteBuffer(Array.Empty<byte>());
                RunAssertionTwoParam(
                    env,
                    $"{two}<{typeof(ByteBuffer).FullName}, {typeof(DateTimeEx).FullName}>",
                    typeof(SupportBeanParameterizedTwo<ByteBuffer, DateTimeEx>),
                    typeof(ByteBuffer),
                    typeof(DateTimeEx),
                    new SupportBeanParameterizedTwo<ByteBuffer, DateTimeEx>(buf, dtx),
                    buf,
                    dtx);

                RunAssertionTwoParam(
                    env,
                    two,
                    typeof(SupportBeanParameterizedTwo<int, string>),
                    typeof(object),
                    typeof(object),
                    new SupportBeanParameterizedTwo<int, string>(1, "a"),
                    1,
                    "a");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        private static void RunAssertionSingleParam(
            RegressionEnvironment env,
            string typeName,
            Type expectedUnderlying,
            Type expectedProperty,
            object @event,
            object expected)
        {
            var epl =
                "@name('schema') @public @buseventtype create schema MyEvent as " +
                typeName +
                ";\n" +
                "@name('s0') select SimpleProperty as c0 from MyEvent;\n";
            env.CompileDeploy(epl).AddListener("s0");

            env.AssertStatement(
                "schema",
                statement => {
                    var schemaType = statement.EventType;
                    ClassicAssert.AreEqual(expectedUnderlying, schemaType.UnderlyingType);
                    var received = schemaType.PropertyDescriptors.ToArray();
                    var fragment = received[0].IsFragment; // ignore fragment, mapped, indexed flags
                    var indexed = received[0].IsIndexed; // ignore fragment, mapped, indexed flags
                    var mapped = received[0].IsMapped; // ignore fragment, mapped, indexed flags
                    SupportEventPropUtil.AssertPropsEquals(
                        received,
                        new SupportEventPropDesc("SimpleProperty", expectedProperty)
                            .WithFragment(fragment)
                            .WithIndexed(indexed)
                            .WithMapped(mapped));

                    SupportEventPropUtil.AssertPropsEquals(
                        env.Statement("s0").EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("c0", expectedProperty)
                            .WithFragment(fragment)
                            .WithIndexed(indexed)
                            .WithMapped(mapped));
                });

            env.SendEventBean(@event, "MyEvent");
            env.AssertEqualsNew("s0", "c0", expected);

            env.UndeployAll();
        }

        private static void RunAssertionTwoParam(
            RegressionEnvironment env,
            string typeName,
            Type expectedUnderlying,
            Type expectedOne,
            Type expectedTwo,
            object @event,
            object valueOne,
            object valueTwo)
        {
            var epl =
                "@name('schema') @public @buseventtype create schema MyEvent as " +
                typeName +
                ";\n" +
                "@name('s0') select one as c0, two as c1 from MyEvent;\n";
            env.CompileDeploy(epl).AddListener("s0");

            env.AssertStatement(
                "s0",
                statement => {
                    var schemaType = env.Statement("schema").EventType;
                    ClassicAssert.AreEqual(expectedUnderlying, schemaType.UnderlyingType);
                    ClassicAssert.AreEqual(expectedOne, schemaType.GetPropertyType("one"));
                    ClassicAssert.AreEqual(expectedTwo, schemaType.GetPropertyType("two"));

                    var s0Type = statement.EventType;
                    ClassicAssert.AreEqual(expectedOne, s0Type.GetPropertyType("c0"));
                    ClassicAssert.AreEqual(expectedTwo, s0Type.GetPropertyType("c1"));
                });

            env.SendEventBean(@event, "MyEvent");
            env.AssertPropsNew("s0", "c0,c1".Split(","), new object[] { valueOne, valueTwo });

            env.UndeployAll();
        }

        public class MyLocalUnparameterized
        {
        }

        public class MyLocalOneParameter<T>
        {
        }

        public class MyLocalBoundParameter<T>
        {
            private T simpleProperty;

            public MyLocalBoundParameter(T simpleProperty)
            {
                this.simpleProperty = simpleProperty;
            }

            public T SimpleProperty => simpleProperty;
        }
    }
} // end of namespace