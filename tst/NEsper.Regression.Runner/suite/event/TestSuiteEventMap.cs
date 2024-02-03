///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.@event.map;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventMap : AbstractTestBase
    {
        [Test]
        [RunInApplicationDomain]
        public void TestEventMapObjectArrayInterUse()
        {
            RegressionRunner.Run(_session, new EventMapObjectArrayInterUse());
        }

        [Test]
        [RunInApplicationDomain]
        public void TestEventMapInheritanceInitTime()
        {
            RegressionRunner.Run(_session, new EventMapInheritanceInitTime());
        }

        [Test]
        [RunInApplicationDomain]
        public void TestEventMapNestedEscapeDot()
        {
            RegressionRunner.Run(_session, new EventMapNestedEscapeDot());
        }

        [Test]
        [RunInApplicationDomain]
        public void TestEventMapNestedConfigStatic()
        {
            RegressionRunner.Run(_session, new EventMapNestedConfigStatic());
        }

        public static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] { typeof(SupportBean) })
            {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            var myMapEvent = new Properties();
            myMapEvent.Put("MyInt", "int");
            myMapEvent.Put("MyString", "string");
            myMapEvent.Put("beanA", typeof(SupportBeanComplexProps).FullName);
            myMapEvent.Put("MyStringArray", "string[]");
            configuration.Common.AddEventType("myMapEvent", myMapEvent);
            
            IDictionary<string, object> myMapLev2def = new Dictionary<string, object>();
            myMapLev2def.Put("sb", "SupportBean");
            
            IDictionary<string, object> myMapLev1def = new Dictionary<string, object>();
            myMapLev1def.Put("lev1name", myMapLev2def);
            
            IDictionary<string, object> myMapLev0def = new Dictionary<string, object>();
            myMapLev0def.Put("lev0name", myMapLev1def);
            configuration.Common.AddEventType("MyMap", myMapLev0def);
            
            var root = EventMapCore.MakeMap(new[] { new object[] { "base", typeof(string) } });
            var sub1 = EventMapCore.MakeMap(new[] { new object[] { "sub1", typeof(string) } });
            var sub2 = EventMapCore.MakeMap(new[] { new object[] { "sub2", typeof(string) } });
            var suba = EventMapCore.MakeProperties(new[] { new object[] { "suba", typeof(string) } });
            var subb = EventMapCore.MakeMap(new[] { new object[] { "subb", typeof(string) } });
            configuration.Common.AddEventType("RootEvent", root);
            configuration.Common.AddEventType("Sub1Event", sub1);
            configuration.Common.AddEventType("Sub2Event", sub2);
            configuration.Common.AddEventType("SubAEvent", suba);
            configuration.Common.AddEventType("SubBEvent", subb);
            configuration.Common.AddMapSuperType("Sub1Event", "RootEvent");
            configuration.Common.AddMapSuperType("Sub2Event", "RootEvent");
            configuration.Common.AddMapSuperType("SubAEvent", "Sub1Event");
            configuration.Common.AddMapSuperType("SubBEvent", "Sub1Event");
            configuration.Common.AddMapSuperType("SubBEvent", "Sub2Event");
            
            var nestedMapLevelThree = EventMapCore.MakeMap(new[]
            {
                new object[] { "simpleThree", typeof(long?) },
                new object[] { "objectThree", typeof(SupportBean_B) }
            });
            
            var nestedMapLevelTwo = EventMapCore.MakeMap(new[]
            {
                new object[] { "simpleTwo", typeof(int?) },
                new object[] { "objectTwo", typeof(SupportBeanCombinedProps) },
                new object[] { "nodefmapTwo", typeof(IDictionary<string, object>) },
                new object[] { "mapTwo", nestedMapLevelThree }
            });
            var nestedMapLevelOne = EventMapCore.MakeMap(new[]
            {
                new object[] { "simpleOne", typeof(int?) },
                new object[] { "objectOne", typeof(SupportBeanComplexProps) },
                new object[] { "nodefmapOne", typeof(IDictionary<string, object>) },
                new object[] { "mapOne", nestedMapLevelTwo }
            });
            var nestedMapLevelZero = EventMapCore.MakeMap(new[]
            {
                new object[] { "simple", typeof(string) }, new object[] { "object", typeof(SupportBean_A) },
                new object[] { "nodefmap", typeof(IDictionary<string, object>) },
                new object[] { "map", nestedMapLevelOne }
            });
            configuration.Common.AddEventType("NestedMap", nestedMapLevelZero);
            var type = EventMapCore.MakeMap(new[]
            {
                new object[] { "base1", typeof(string) },
                new object[] { "base2", EventMapCore.MakeMap(new[] { new object[] { "n1", typeof(int) } }) }
            });
            
            configuration.Common.AddEventType("MyEvent", type);
            var properties = new Properties();
            properties.Put("myInt", typeof(int).FullName);
            properties.Put("byteArr", typeof(byte[]).FullName);
            properties.Put("myInt2", "int");
            properties.Put("double", "double");
            properties.Put("boolean", "boolean");
            properties.Put("long", "long");
            properties.Put("astring", "string");
            configuration.Common.AddEventType("MyPrimMapEvent", properties);
            var myLevel2 = new Properties();
            myLevel2.Put("Innermap", typeof(IDictionary<string, object>).FullName);
            configuration.Common.AddEventType("MyLevel2", myLevel2);
            // create a named map
            var namedDef = EventMapCore.MakeMap(new[] { new object[] { "n0", typeof(int) } });
            configuration.Common.AddEventType("MyNamedMap", namedDef);
            // create a map using the name
            var eventDef = EventMapCore.MakeMap(new[]
                { new object[] { "P0", "MyNamedMap" }, new object[] { "P1", "MyNamedMap[]" } });
            configuration.Common.AddEventType("MyMapWithAMap", eventDef);
            // test map containing first-level property that is an array of primitive or Class
            var arrayDef = EventMapCore.MakeMap(new[]
                { new object[] { "P0", typeof(int[]) }, new object[] { "P1", typeof(SupportBean[]) } });
            configuration.Common.AddEventType("MyArrayMap", arrayDef);
            // test map at the second level of a nested map that is an array of primitive or Class
            var arrayDefOuter = EventMapCore.MakeMap(new[] { new object[] { "outer", arrayDef } });
            configuration.Common.AddEventType("MyArrayMapOuter", arrayDefOuter);
            // test map containing first-level property that is an array of primitive or Class
            var mappedDef = EventMapCore.MakeMap(new[] { new object[] { "P0", typeof(IDictionary<string, object>) } });
            configuration.Common.AddEventType("MyMappedPropertyMap", mappedDef);
            // test map at the second level of a nested map that is an array of primitive or Class
            var mappedDefOuter = EventMapCore.MakeMap(new[] { new object[] { "outer", mappedDef } });
            configuration.Common.AddEventType("MyMappedPropertyMapOuter", mappedDefOuter);
            var mappedDefOuterTwo =
                EventMapCore.MakeMap(new[] { new object[] { "outerTwo", typeof(SupportBeanComplexProps) } });
            configuration.Common.AddEventType("MyMappedPropertyMapOuterTwo", mappedDefOuterTwo);
            // create a named map
            var myNamedMap = EventMapCore.MakeMap(new[] { new object[] { "n0", typeof(int) } });
            configuration.Common.AddEventType("MyNamedMap", myNamedMap);
            // create a map using the name
            var myMapWithAMap = EventMapCore.MakeMap(new[]
                { new object[] { "P0", "MyNamedMap" }, new object[] { "P1", "MyNamedMap[]" } });
            configuration.Common.AddEventType("MyMapWithAMap", myMapWithAMap);
            // test named-map at the second level of a nested map
            var myArrayMapTwo = EventMapCore.MakeMap(new[] { new object[] { "outer", myMapWithAMap } });
            configuration.Common.AddEventType("MyArrayMapTwo", myArrayMapTwo);
            configuration.Common.AddEventType("MapType", Collections.SingletonDataMap("im", typeof(string)));
            configuration.Common.AddEventType("OAType", new[] { "p0", "p1", "p2", "p3" },
                new object[]
                    { typeof(string), "MapType", "MapType[]", Collections.SingletonDataMap("om", typeof(string)) });
            var definition = EventMapCore.MakeMap(new[]
            {
                new object[] { "a.b", typeof(int) }, new object[] { "a.b.c", typeof(int) },
                new object[] { "nes.", typeof(int) },
                new object[] { "nes.nes2", EventMapCore.MakeMap(new[] { new object[] { "x.y", typeof(int) } }) }
            });
            configuration.Common.AddEventType("DotMap", definition);
            IDictionary<string, object> nmwspPropertiesNestedNested = new Dictionary<string, object>();
            nmwspPropertiesNestedNested.Put("n1n1", typeof(string));
            IDictionary<string, object> nmwspPropertiesNested = new Dictionary<string, object>();
            nmwspPropertiesNested.Put("n1", typeof(string));
            nmwspPropertiesNested.Put("n2", nmwspPropertiesNestedNested);
            IDictionary<string, object> nmwspRoot = new Dictionary<string, object>();
            nmwspRoot.Put("Nested", nmwspPropertiesNested);
            configuration.Common.AddEventType("NestedMapWithSimpleProps", nmwspRoot);
        }

        /// <summary>
        ///     Auto-test(s): EventMapCore
        ///     <code>
        /// RegressionRunner.Run(_session, EventMapCore.Executions());
        /// </code>
        /// </summary>
        public class TestEventMapCore : AbstractTestBase
        {
            public TestEventMapCore() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMapNestedEventType()
            {
                RegressionRunner.Run(_session, EventMapCore.WithMapNestedEventType());
            }

            [Test, RunInApplicationDomain]
            public void WithMetadata()
            {
                RegressionRunner.Run(_session, EventMapCore.WithMetadata());
            }

            [Test, RunInApplicationDomain]
            public void WithNestedObjects()
            {
                RegressionRunner.Run(_session, EventMapCore.WithNestedObjects());
            }

            [Test, RunInApplicationDomain]
            public void WithQueryFields()
            {
                RegressionRunner.Run(_session, EventMapCore.WithQueryFields());
            }

            [Test, RunInApplicationDomain]
            public void WithInvalidStatement()
            {
                RegressionRunner.Run(_session, EventMapCore.WithInvalidStatement());
            }
        }

        /// <summary>
        ///     Auto-test(s): EventMapNested
        ///     <code>
        /// RegressionRunner.Run(_session, EventMapNested.Executions());
        /// </code>
        /// </summary>
        public class TestEventMapNested : AbstractTestBase
        {
            public TestEventMapNested() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInsertInto()
            {
                RegressionRunner.Run(_session, EventMapNested.WithInsertInto());
            }

            [Test, RunInApplicationDomain]
            public void WithEventType()
            {
                RegressionRunner.Run(_session, EventMapNested.WithEventType());
            }

            [Test, RunInApplicationDomain]
            public void WithNestedPono()
            {
                RegressionRunner.Run(_session, EventMapNested.WithNestedPono());
            }

            [Test, RunInApplicationDomain]
            public void WithIsExists()
            {
                RegressionRunner.Run(_session, EventMapNested.WithIsExists());
            }
        }

        /// <summary>
        ///     Auto-test(s): EventMapProperties
        ///     <code>
        /// RegressionRunner.Run(_session, EventMapProperties.Executions());
        /// </code>
        /// </summary>
        public class TestEventMapProperties : AbstractTestBase
        {
            public TestEventMapProperties() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithArrayProperty()
            {
                RegressionRunner.Run(_session, EventMapProperties.WithArrayProperty());
            }

            [Test, RunInApplicationDomain]
            public void WithMappedProperty()
            {
                RegressionRunner.Run(_session, EventMapProperties.WithMappedProperty());
            }

            [Test, RunInApplicationDomain]
            public void WithMapNamePropertyNested()
            {
                RegressionRunner.Run(_session, EventMapProperties.WithMapNamePropertyNested());
            }

            [Test, RunInApplicationDomain]
            public void WithMapNameProperty()
            {
                RegressionRunner.Run(_session, EventMapProperties.WithMapNameProperty());
            }
        }

        /// <summary>
        ///     Auto-test(s): EventMapPropertyDynamic
        /// <code>
        /// RegressionRunner.Run(_session, EventMapPropertyDynamic.Executions());
        /// </code>
        /// </summary>
        public class TestEventMapPropertyDynamic : AbstractTestBase
        {
            public TestEventMapPropertyDynamic() : base(Configure)
            {
            }
            
            [Test, RunInApplicationDomain]
            public void WithMapWithinMap() => 
                RegressionRunner.Run(_session, EventMapPropertyDynamic.WithMapWithinMap());


            [Test, RunInApplicationDomain]
            public void WithMapWithinMapExists() =>
                RegressionRunner.Run(_session, EventMapPropertyDynamic.WithMapWithinMapExists());

            [Test, RunInApplicationDomain]
            public void WithMapWithinMap2LevelsInvalid() =>
                RegressionRunner.Run(_session, EventMapPropertyDynamic.WithMapWithinMap2LevelsInvalid());
        }
    }
} // end of namespace