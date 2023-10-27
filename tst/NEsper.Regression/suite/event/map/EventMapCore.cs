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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.map
{
    public class EventMapCore
    {
        private static IDictionary<string, object> map;

        static EventMapCore()
        {
            map = new Dictionary<string, object>();
            map.Put("myInt", 3);
            map.Put("myString", "some string");
            map.Put("beanA", SupportBeanComplexProps.MakeDefaultBean());
        }

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithMapNestedEventType(execs);
            WithMetadata(execs);
            WithNestedObjects(execs);
            WithQueryFields(execs);
            WithInvalidStatement(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidStatement(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventMapCoreInvalidStatement());
            return execs;
        }

        public static IList<RegressionExecution> WithQueryFields(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventMapCoreQueryFields());
            return execs;
        }

        public static IList<RegressionExecution> WithNestedObjects(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventMapCoreNestedObjects());
            return execs;
        }

        public static IList<RegressionExecution> WithMetadata(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventMapCoreMetadata());
            return execs;
        }

        public static IList<RegressionExecution> WithMapNestedEventType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventMapCoreMapNestedEventType());
            return execs;
        }

        private class EventMapCoreMapNestedEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AssertThat(() => Assert.NotNull(env.Runtime.EventTypeService.GetEventTypePreconfigured("MyMap")));
                env.CompileDeploy("@name('s0') select lev0name.lev1name.sb.TheString as val from MyMap")
                    .AddListener("s0");

                IDictionary<string, object> lev2data = new Dictionary<string, object>();
                lev2data.Put("sb", new SupportBean("E1", 0));
                IDictionary<string, object> lev1data = new Dictionary<string, object>();
                lev1data.Put("lev1name", lev2data);
                IDictionary<string, object> lev0data = new Dictionary<string, object>();
                lev0data.Put("lev0name", lev1data);

                env.SendEventMap(lev0data, "MyMap");
                env.AssertEqualsNew("s0", "val", "E1");

                env.AssertThat(
                    () => {
                        try {
                            env.SendEventObjectArray(Array.Empty<object>(), "MyMap");
                            Assert.Fail();
                        }
                        catch (EPException ex) {
                            Assert.AreEqual(
                                "Event type named 'MyMap' has not been defined or is not a Object-array event type, the name 'MyMap' refers to a java.util.Map event type",
                                ex.Message);
                        }
                    });
                env.UndeployAll();
            }
        }

        private class EventMapCoreMetadata : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AssertThat(
                    () => {
                        var type = env.Runtime.EventTypeService.GetEventTypePreconfigured("myMapEvent");
                        Assert.AreEqual(EventTypeApplicationType.MAP, type.Metadata.ApplicationType);
                        Assert.AreEqual("myMapEvent", type.Metadata.Name);

                        SupportEventPropUtil.AssertPropsEquals(
                            type.PropertyDescriptors.ToArray(),
                            new SupportEventPropDesc("myInt", typeof(int?)),
                            new SupportEventPropDesc("myString", typeof(string)),
                            new SupportEventPropDesc("beanA", typeof(SupportBeanComplexProps)).WithFragment(),
                            new SupportEventPropDesc("myStringArray", typeof(string[]))
                                .WithComponentType(typeof(string))
                                .WithIndexed());
                    });
            }
        }

        private class EventMapCoreNestedObjects : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText = "@name('s0') select beanA.SimpleProperty as simple," +
                                    "beanA.Nested.NestedValue as nested," +
                                    "beanA.indexed[1] as indexed," +
                                    "beanA.Nested.NestedNested.NestedNestedValue as nestednested " +
                                    "from myMapEvent#length(5)";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventMap(map, "myMapEvent");
                env.AssertEventNew(
                    "s0",
                    @event => {
                        Assert.AreEqual("NestedValue", @event.Get("Nested"));
                        Assert.AreEqual(2, @event.Get("indexed"));
                        Assert.AreEqual("NestedNestedValue", @event.Get("nestednested"));
                    });

                env.UndeployAll();
            }
        }

        private class EventMapCoreQueryFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText =
                    "@name('s0') select myInt as intVal, myString as stringVal from myMapEvent#length(5)";
                env.CompileDeploy(statementText).AddListener("s0");

                // send Map<String, Object> event
                env.SendEventMap(map, "myMapEvent");
                env.AssertEventNew(
                    "s0",
                    @event => {
                        Assert.AreEqual(3, @event.Get("intVal"));
                        Assert.AreEqual("some string", @event.Get("stringVal"));
                    });

                // send Map base event
                IDictionary<string, object> mapNoType = new Dictionary<string, object>();
                mapNoType.Put("myInt", 4);
                mapNoType.Put("myString", "string2");
                env.SendEventMap(mapNoType, "myMapEvent");
                env.AssertEventNew(
                    "s0",
                    @event => {
                        Assert.AreEqual(4, @event.Get("intVal"));
                        Assert.AreEqual("string2", @event.Get("stringVal"));
                    });

                env.UndeployAll();
            }
        }

        private class EventMapCoreInvalidStatement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile("select XXX from myMapEvent#length(5)", "skip");
                env.TryInvalidCompile("select myString * 2 from myMapEvent#length(5)", "skip");
                env.TryInvalidCompile("select String.trim(myInt) from myMapEvent#length(5)", "skip");
            }
        }

        public static IDictionary<string, object> MakeMap(string nameValuePairs)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            var elements = nameValuePairs.SplitCsv();
            for (var i = 0; i < elements.Length; i++) {
                var pair = elements[i].Split("=");
                if (pair.Length == 2) {
                    result.Put(pair[0], pair[1]);
                }
            }

            return result;
        }

        public static IDictionary<string, object> MakeMap(object[][] entries)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            if (entries == null) {
                return result;
            }

            for (var i = 0; i < entries.Length; i++) {
                result.Put((string)entries[i][0], entries[i][1]);
            }

            return result;
        }

        public static Properties MakeProperties(object[][] entries)
        {
            var result = new Properties();
            for (var i = 0; i < entries.Length; i++) {
                var clazz = (Type)entries[i][1];
                result.Put((string)entries[i][0], clazz.Name);
            }

            return result;
        }

        public static object GetNestedKeyMap(
            IDictionary<string, object> root,
            string keyOne,
            string keyTwo)
        {
            var map = (IDictionary<string, object>)root.Get(keyOne);
            return map.Get(keyTwo);
        }

        internal static object GetNestedKeyMap(
            IDictionary<string, object> root,
            string keyOne,
            string keyTwo,
            string keyThree)
        {
            var map = (IDictionary<string, object>)root.Get(keyOne);
            map = (IDictionary<string, object>)map.Get(keyTwo);
            return map.Get(keyThree);
        }
    }
} // end of namespace