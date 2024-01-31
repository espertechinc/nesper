///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.@event.objectarray
{
    public class EventObjectArrayCore
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithMetadata(execs);
            WithNestedObjects(execs);
            WithQueryFields(execs);
            WithNestedEventBeanArray(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventObjectArrayInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithNestedEventBeanArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventObjectArrayNestedEventBeanArray());
            return execs;
        }

        public static IList<RegressionExecution> WithQueryFields(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventObjectArrayQueryFields());
            return execs;
        }

        public static IList<RegressionExecution> WithNestedObjects(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventObjectArrayNestedObjects());
            return execs;
        }

        public static IList<RegressionExecution> WithMetadata(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventObjectArrayMetadata());
            return execs;
        }

        private class EventObjectArrayNestedEventBeanArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var schemas = "@buseventtype @public create objectarray schema NBAL_1(val string);\n" +
                              "@buseventtype @public create objectarray schema NBAL_2 (lvl1s NBAL_1[]);\n";
                env.CompileDeploy(schemas, path);
                env.CompileDeploy("@name('s0') select * from NBAL_1", path).AddListener("s0");

                var oa = new object[] { "somevalue" };
                env.SendEventObjectArray(oa, "NBAL_1");
                env.AssertEventNew("s0", @event => { });
                env.UndeployModuleContaining("s0");

                // add containing-type
                env.CompileDeploy("@name('s0') select lvl1s[0] as c0 from NBAL_2", path).AddListener("s0");

                env.SendEventObjectArray(new object[] { new object[] { oa } }, "NBAL_2");
                env.AssertEventNew("s0", @event => ClassicAssert.AreEqual("somevalue", ((object[])@event.Get("c0"))[0]));

                env.UndeployAll();
            }
        }

        private class EventObjectArrayMetadata : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AssertThat(
                    () => {
                        var type = env.Runtime.EventTypeService.GetEventTypePreconfigured("MyObjectArrayEvent");
                        ClassicAssert.AreEqual(EventTypeApplicationType.OBJECTARR, type.Metadata.ApplicationType);
                        ClassicAssert.AreEqual("MyObjectArrayEvent", type.Metadata.Name);

                        SupportEventPropUtil.AssertPropsEquals(
                            type.PropertyDescriptors.ToArray(),
                            new SupportEventPropDesc("MyInt", typeof(int?)),
                            new SupportEventPropDesc("MyString", typeof(string)),
                            new SupportEventPropDesc("beanA", typeof(SupportBeanComplexProps)).WithFragment());
                    });
            }
        }

        private class EventObjectArrayNestedObjects : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText = "@name('s0') select beanA.SimpleProperty as simple," +
                                    "beanA.Nested.NestedValue as nested," +
                                    "beanA.Indexed[1] as indexed," +
                                    "beanA.Nested.NestedNested.NestedNestedValue as nestednested " +
                                    "from MyObjectArrayEvent#length(5)";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventObjectArray(
                    new object[] { 3, "some string", SupportBeanComplexProps.MakeDefaultBean() },
                    "MyObjectArrayEvent");
                env.AssertEventNew(
                    "s0",
                    @event => {
                        ClassicAssert.AreEqual("NestedValue", @event.Get("nested"));
                        ClassicAssert.AreEqual(2, @event.Get("indexed"));
                        ClassicAssert.AreEqual("NestedNestedValue", @event.Get("nestednested"));
                    });

                env.UndeployAll();
            }
        }

        private class EventObjectArrayQueryFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText =
                    "@name('s0') select MyInt + 2 as intVal, 'x' || MyString || 'x' as stringVal from MyObjectArrayEvent#length(5)";
                env.CompileDeploy(statementText).AddListener("s0");

                // send Map<String, Object> event
                env.SendEventObjectArray(
                    new object[] { 3, "some string", SupportBeanComplexProps.MakeDefaultBean() },
                    "MyObjectArrayEvent");
                env.AssertEventNew(
                    "s0",
                    @event => {
                        ClassicAssert.AreEqual(5, @event.Get("intVal"));
                        ClassicAssert.AreEqual("xsome stringx", @event.Get("stringVal"));
                    });

                // send Map base event
                env.SendEventObjectArray(new object[] { 4, "string2", null }, "MyObjectArrayEvent");
                env.AssertEventNew(
                    "s0",
                    @event => {
                        ClassicAssert.AreEqual(6, @event.Get("intVal"));
                        ClassicAssert.AreEqual("xstring2x", @event.Get("stringVal"));
                    });

                env.UndeployAll();
            }
        }

        private class EventObjectArrayInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile("select XXX from MyObjectArrayEvent#length(5)", "skip");
                env.TryInvalidCompile("select MyString * 2 from MyObjectArrayEvent#length(5)", "skip");
                env.TryInvalidCompile("select String.trim(MyInt) from MyObjectArrayEvent#length(5)", "skip");
            }
        }

        internal static object GetNestedKeyOA(
            object[] array,
            int index,
            string keyTwo)
        {
            var map = (IDictionary<string, object>)array[index];
            return map.Get(keyTwo);
        }

        internal static object GetNestedKeyOA(
            object[] array,
            int index,
            string keyTwo,
            string keyThree)
        {
            var map = (IDictionary<string, object>)array[index];
            map = (IDictionary<string, object>)map.Get(keyTwo);
            return map.Get(keyThree);
        }
    }
} // end of namespace