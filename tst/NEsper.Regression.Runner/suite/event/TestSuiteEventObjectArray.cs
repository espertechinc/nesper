///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.@event.objectarray;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.suite.@event.map.EventMapCore;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventObjectArray : AbstractTestBase {
        public TestSuiteEventObjectArray() : base(Configure)
        {
        }

        [Test, RunInApplicationDomain]
        public void TestEventObjectArrayNestedMap()
        {
            RegressionRunner.Run(_session, new EventObjectArrayNestedMap());
        }

        [Test, RunInApplicationDomain]
        public void TestEventObjectArrayInheritanceConfigInit()
        {
            RegressionRunner.Run(_session, new EventObjectArrayInheritanceConfigInit());
        }

        [Test, RunInApplicationDomain]
        public void TestEventObjectArrayEventNestedPono()
        {
            RegressionRunner.Run(_session, new EventObjectArrayEventNestedPono());
        }

        public static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] { typeof(SupportBean) }) {
                configuration.Common.AddEventType(clazz);
            }

            string[] myObjectArrayEvent =
            {
                "MyInt",
                "MyString",
                "beanA"
            };

            object[] MyObjectArrayTypes =
            {
                typeof(int?),
                typeof(string),
                typeof(SupportBeanComplexProps)
            };

            configuration.Common.AddEventType("MyObjectArrayEvent", myObjectArrayEvent, MyObjectArrayTypes);

            string[] myArrayOAProps =
            {
                "P0",
                "P1"
            };

            object[] MyArrayOATypes =
            {
                typeof(int[]),
                typeof(SupportBean[])
            };

            configuration.Common.AddEventType("MyArrayOA", myArrayOAProps, MyArrayOATypes);
            configuration.Common.AddEventType("MyArrayOAMapOuter", new[] { "outer" },
                new object[] { "MyArrayOA" });

            var mappedDef = MakeMap(new[] { new object[] { "P0", typeof(IDictionary<string, string>) } });
            configuration.Common.AddEventType("MyMappedPropertyMap", mappedDef);

            var mappedDefOuter = MakeMap(new[] { new object[] { "outer", mappedDef } });
            configuration.Common.AddEventType("MyMappedPropertyMapOuter", mappedDefOuter);

            var mappedDefOuterTwo = MakeMap(new[] { new object[] { "outerTwo", typeof(SupportBeanComplexProps) } });
            configuration.Common.AddEventType("MyMappedPropertyMapOuterTwo", mappedDefOuterTwo);

            var namedDef = MakeMap(new[] { new object[] { "n0", typeof(int) } });
            configuration.Common.AddEventType("MyNamedMap", namedDef);

            var eventDef = MakeMap(new[]
            {
                new object[] { "p0", "MyNamedMap" },
                new object[] { "p1", "MyNamedMap[]" }
            });

            configuration.Common.AddEventType("MyMapWithAMap", eventDef);
            configuration.Common.AddEventType("MyObjectArrayMapOuter", new[] { "outer" }, new object[] { eventDef });
            configuration.Common.AddEventType("MyOAWithAMap", new[] { "p0", "p1" }, new object[] { "MyNamedMap", "MyNamedMap[]" });
            configuration.Common.AddEventType("TypeLev1", new[] { "p1id" }, new object[] { typeof(int) });
            configuration.Common.AddEventType("TypeLev0", new[] { "p0id", "p1" }, new object[] { typeof(int), "TypeLev1" });
            configuration.Common.AddEventType("TypeRoot", new[] { "rootId", "p0" }, new object[] { typeof(int), "TypeLev0" });

            var pair = GetTestDef();
            
            configuration.Common.AddEventType("NestedObjectArr", pair.First, pair.Second);
            configuration.Common.AddEventType("MyNested", new[] { "bean" }, new object[] { typeof(EventObjectArrayEventNestedPono.MyNested) });
            configuration.Common.AddEventType("RootEvent", new[] { "base" }, new object[] { typeof(string) });
            configuration.Common.AddEventType("Sub1Event", new[] { "sub1" }, new object[] { typeof(string) });
            configuration.Common.AddEventType("Sub2Event", new[] { "sub2" }, new object[] { typeof(string) });
            configuration.Common.AddEventType("SubAEvent", new[] { "suba" }, new object[] { typeof(string) });
            configuration.Common.AddEventType("SubBEvent", new[] { "subb" }, new object[] { typeof(string) });
            configuration.Common.AddObjectArraySuperType("Sub1Event", "RootEvent");
            configuration.Common.AddObjectArraySuperType("Sub2Event", "RootEvent");
            configuration.Common.AddObjectArraySuperType("SubAEvent", "Sub1Event");
            configuration.Common.AddObjectArraySuperType("SubBEvent", "SubAEvent");

            IDictionary<string, object> nestedOALev2def = new Dictionary<string, object>();
            nestedOALev2def.Put("sb", "SupportBean");

            IDictionary<string, object> nestedOALev1def = new Dictionary<string, object>();
            nestedOALev1def.Put("lev1name", nestedOALev2def);

            configuration.Common.AddEventType(
                "MyMapNestedObjectArray", 
                new[] { "lev0name" },
                new object[] { nestedOALev1def });
        }

        private static Pair<string[], object[]> GetTestDef()
        {
            var levelThree = MakeMap(new[]
            {
                new object[] { "SimpleThree", typeof(long?) }, new object[] { "ObjectThree", typeof(SupportBean_B) },
            });
            var levelTwo = MakeMap(new[]
            {
                new object[] { "SimpleTwo", typeof(int?) },
                new object[] { "ObjectTwo", typeof(SupportBeanCombinedProps) },
                new object[] { "NodefmapTwo", typeof(IDictionary<string, object>) },
                new object[] { "MapTwo", levelThree },
            });
            var levelOne = MakeMap(new[]
            {
                new object[] { "SimpleOne", typeof(int?) },
                new object[] { "ObjectOne", typeof(SupportBeanComplexProps) },
                new object[] { "NodefmapOne", typeof(IDictionary<string, object>) }, new object[] { "MapOne", levelTwo }
            });
            string[] levelZeroProps =
            {
                "Simple",
                "Object",
                "Nodefmap",
                "Map"
            };
            object[] levelZeroTypes =
            {
                typeof(string),
                typeof(SupportBean_A),
                typeof(IDictionary<string, object>),
                levelOne
            };
            return new Pair<string[], object[]>(levelZeroProps, levelZeroTypes);
        }

        /// <summary>
        /// Auto-test(s): EventObjectArrayCore
        /// <code>
        /// RegressionRunner.Run(_session, EventObjectArrayCore.Executions());
        /// </code>
        /// </summary>
        public class TestEventObjectArrayCore : AbstractTestBase {
            public TestEventObjectArrayCore() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMetadata() => RegressionRunner.Run(_session, EventObjectArrayCore.WithMetadata());

            [Test, RunInApplicationDomain]
            public void WithNestedObjects() =>
                RegressionRunner.Run(_session, EventObjectArrayCore.WithNestedObjects());

            [Test, RunInApplicationDomain]
            public void WithQueryFields() =>
                RegressionRunner.Run(_session, EventObjectArrayCore.WithQueryFields());

            [Test, RunInApplicationDomain]
            public void WithNestedEventBeanArray() =>
                RegressionRunner.Run(_session, EventObjectArrayCore.WithNestedEventBeanArray());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EventObjectArrayCore.WithInvalid());
        }

        /// <summary>
        /// Auto-test(s): EventObjectArrayEventNested
        /// <code>
        /// RegressionRunner.Run(_session, EventObjectArrayEventNested.Executions());
        /// </code>
        /// </summary>
        public class TestEventObjectArrayEventNested : AbstractTestBase {
            public TestEventObjectArrayEventNested() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithArrayProperty() => RegressionRunner.Run(_session, EventObjectArrayEventNested.WithArrayProperty());

            [Test, RunInApplicationDomain]
            public void WithMappedProperty() => RegressionRunner.Run(_session, EventObjectArrayEventNested.WithMappedProperty());

            [Test, RunInApplicationDomain]
            public void WithMapNamePropertyNested() => RegressionRunner.Run(_session, EventObjectArrayEventNested.WithMapNamePropertyNested());

            [Test, RunInApplicationDomain]
            public void WithMapNameProperty() => RegressionRunner.Run(_session, EventObjectArrayEventNested.WithMapNameProperty());

            [Test, RunInApplicationDomain]
            public void WithObjectArrayNested() => RegressionRunner.Run(_session, EventObjectArrayEventNested.WithObjectArrayNested());
        }
    }
} // end of namespace
