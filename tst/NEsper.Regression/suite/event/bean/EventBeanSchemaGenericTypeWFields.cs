///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanSchemaGenericTypeWFields
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithPlain(execs);
            WithMapped(execs);
            WithIndexed(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithIndexed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventBeanCreateSchemaTypeParamIndexed());
            return execs;
        }

        public static IList<RegressionExecution> WithMapped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventBeanCreateSchemaTypeParamMapped());
            return execs;
        }

        public static IList<RegressionExecution> WithPlain(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventBeanCreateSchemaTypeParamPlain());
            return execs;
        }

        private class EventBeanCreateSchemaTypeParamPlain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('schema') @public @buseventtype create schema MyEvent as " +
                    typeof(SupportBeanParameterizedWFieldSinglePlain<>).FullName +
                    "<Integer>;\n" +
                    "@name('s0') select SimpleProperty as c0, simpleField as c1 from MyEvent;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "schema",
                    statement => {
                        var type = env.Statement("schema").EventType.UnderlyingType;
                        ClassicAssert.AreEqual(typeof(SupportBeanParameterizedWFieldSinglePlain<int?>), type);
                        SupportEventPropUtil.AssertPropsEquals(
                            env.Statement("schema").EventType.PropertyDescriptors.ToArray(),
                            new SupportEventPropDesc("SimpleProperty", typeof(int?)),
                            new SupportEventPropDesc("simpleField", typeof(int?)));

                        SupportEventPropUtil.AssertPropsEquals(
                            env.Statement("s0").EventType.PropertyDescriptors.ToArray(),
                            new SupportEventPropDesc("c0", typeof(int?)),
                            new SupportEventPropDesc("c1", typeof(int?)));
                    });

                env.SendEventBean(new SupportBeanParameterizedWFieldSinglePlain<int?>(10), "MyEvent");
                env.AssertPropsNew("s0", "c0,c1".Split(","), new object[] { 10, 10 });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        private class EventBeanCreateSchemaTypeParamMapped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('schema') @public @buseventtype create schema MyEvent as " +
                    typeof(SupportBeanParameterizedWFieldSingleMapped<>).FullName +
                    "<Integer>;\n" +
                    "@name('s0') select mapProperty as c0, mapField as c1, mapProperty('key') as c2, mapField('key') as c3, mapKeyed('key') as c4 from MyEvent;\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStatement(
                    "schema",
                    statement => SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("mapProperty", typeof(IDictionary<string, int?>)).WithMapped(),
                        new SupportEventPropDesc("mapField", typeof(IDictionary<string, int?>)).WithMapped(),
                        new SupportEventPropDesc("mapKeyed", typeof(int?)).WithMapped().WithMappedRequiresKey()
                    ));

                env.AssertStatement(
                    "s0",
                    statement => SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("c0", typeof(IDictionary<string, int?>)).WithMapped(),
                        new SupportEventPropDesc("c1", typeof(IDictionary<string, int?>)).WithMapped(),
                        new SupportEventPropDesc("c2", typeof(int?)),
                        new SupportEventPropDesc("c3", typeof(int?)),
                        new SupportEventPropDesc("c4", typeof(int?))));

                env.SendEventBean(new SupportBeanParameterizedWFieldSingleMapped<int?>(10), "MyEvent");
                env.AssertPropsNew(
                    "s0",
                    "c0,c1,c2,c3,c4".Split(","),
                    new object[]
                        { CollectionUtil.BuildMap("key", 10), CollectionUtil.BuildMap("key", 10), 10, 10, 10 });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        private class EventBeanCreateSchemaTypeParamIndexed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('schema') @public @buseventtype create schema MyEvent as " +
                    typeof(SupportBeanParameterizedWFieldSingleIndexed<>).FullName +
                    "<Integer>;\n" +
                    "@name('s0') select indexedArrayProperty as c0, indexedArrayField as c1, indexedArrayProperty[0] as c2, indexedArrayField[0] as c3," +
                    "indexedListProperty as c4, indexedListField as c5, indexedListProperty[0] as c6, indexedListField[0] as c7," +
                    "indexedArrayAtIndex[0] as c8 from MyEvent;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "schema",
                    statement => {
                        SupportEventPropUtil.AssertPropsEquals(
                            statement.EventType.PropertyDescriptors.ToArray(),
                            new SupportEventPropDesc("indexedArrayProperty", typeof(int?[])).WithIndexed(),
                            new SupportEventPropDesc("indexedArrayField", typeof(int?[])).WithIndexed(),
                            new SupportEventPropDesc("indexedListProperty", typeof(IList<int?>)).WithIndexed(),
                            new SupportEventPropDesc("indexedListField", typeof(IList<int?>)).WithIndexed(),
                            new SupportEventPropDesc("indexedArrayAtIndex", typeof(int?)).WithIndexed()
                                .WithIndexedRequiresIndex());
                    });

                env.AssertStatement(
                    "s0",
                    statement => SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("c0", typeof(int?[])).WithIndexed(),
                        new SupportEventPropDesc("c1", typeof(int?[])).WithIndexed(),
                        new SupportEventPropDesc("c2", typeof(int?)),
                        new SupportEventPropDesc("c3", typeof(int?)),
                        new SupportEventPropDesc("c4", typeof(IList<int?>)).WithIndexed(),
                        new SupportEventPropDesc("c5", typeof(IList<int?>)).WithIndexed(),
                        new SupportEventPropDesc("c6", typeof(int?)),
                        new SupportEventPropDesc("c7", typeof(int?)),
                        new SupportEventPropDesc("c8", typeof(int?))));

                env.SendEventBean(new SupportBeanParameterizedWFieldSingleIndexed<int?>(10), "MyEvent");
                env.AssertPropsNew(
                    "s0",
                    "c0,c1,c2,c3,c4,c5,c6,c7,c8".Split(","),
                    new object[] {
                        new int?[] { 10 }, new int?[] { 10 }, 10, 10,
                        Collections.SingletonList(10), Collections.SingletonList(10), 10, 10, 10
                    });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }
    }
} // end of namespace