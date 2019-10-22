///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.@event.map;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventMap
    {
        private RegressionSession session;

        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        [Test, RunInApplicationDomain]
        public void TestEventMapCore()
        {
            RegressionRunner.Run(session, EventMapCore.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEventMapPropertyDynamic()
        {
            RegressionRunner.Run(session, new EventMapPropertyDynamic());
        }

        [Test, RunInApplicationDomain]
        public void TestEventMapObjectArrayInterUse()
        {
            RegressionRunner.Run(session, new EventMapObjectArrayInterUse());
        }

        [Test, RunInApplicationDomain]
        public void TestEventMapInheritanceInitTime()
        {
            RegressionRunner.Run(session, new EventMapInheritanceInitTime());
        }

        [Test, RunInApplicationDomain]
        public void TestEventMapNestedEscapeDot()
        {
            RegressionRunner.Run(session, new EventMapNestedEscapeDot());
        }

        [Test, RunInApplicationDomain]
        public void TestEventMapNestedConfigStatic()
        {
            RegressionRunner.Run(session, new EventMapNestedConfigStatic());
        }

        [Test, RunInApplicationDomain]
        public void TestEventMapNested()
        {
            RegressionRunner.Run(session, EventMapNested.Executions());
        }

        [Test]
        public void TestEventMapProperties()
        {
            RegressionRunner.Run(session, EventMapProperties.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[] {typeof(SupportBean)}) {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            Properties myMapEvent = new Properties();
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

            IDictionary<string, object> root = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"base", typeof(string)}
                });
            IDictionary<string, object> sub1 = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"sub1", typeof(string)}
                });
            IDictionary<string, object> sub2 = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"sub2", typeof(string)}
                });
            Properties suba = EventMapCore.MakeProperties(
                new object[][] {
                    new object[] {"suba", typeof(string)}
                });
            IDictionary<string, object> subb = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"subb", typeof(string)}
                });
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

            IDictionary<string, object> nestedMapLevelThree = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"simpleThree", typeof(long?)},
                    new object[] {"objectThree", typeof(SupportBean_B)},
                });
            IDictionary<string, object> nestedMapLevelTwo = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"simpleTwo", typeof(int?)},
                    new object[] {"objectTwo", typeof(SupportBeanCombinedProps)},
                    new object[] {"nodefmapTwo", typeof(IDictionary<string, object>)},
                    new object[] {"mapTwo", nestedMapLevelThree},
                });
            IDictionary<string, object> nestedMapLevelOne = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"simpleOne", typeof(int?)},
                    new object[] {"objectOne", typeof(SupportBeanComplexProps)},
                    new object[] {"nodefmapOne", typeof(IDictionary<string, object>)},
                    new object[] {"mapOne", nestedMapLevelTwo}
                });
            IDictionary<string, object> nestedMapLevelZero = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"simple", typeof(string)},
                    new object[] {"object", typeof(SupportBean_A)},
                    new object[] {"nodefmap", typeof(IDictionary<string, object>)},
                    new object[] {"map", nestedMapLevelOne}
                });
            configuration.Common.AddEventType("NestedMap", nestedMapLevelZero);

            IDictionary<string, object> type = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"base1", typeof(string)},
                    new object[] {
                        "base2", EventMapCore.MakeMap(
                            new object[][] {
                                new object[] {"n1", typeof(int)}
                            })
                    }
                });
            configuration.Common.AddEventType("MyEvent", type);

            Properties properties = new Properties();
            properties.Put("myInt", typeof(int).FullName);
            properties.Put("byteArr", typeof(byte[]).FullName);
            properties.Put("myInt2", "int");
            properties.Put("double", "double");
            properties.Put("boolean", "boolean");
            properties.Put("long", "long");
            properties.Put("astring", "string");
            configuration.Common.AddEventType("MyPrimMapEvent", properties);

            Properties myLevel2 = new Properties();
            myLevel2.Put("innermap", typeof(IDictionary<string, object>).FullName);
            configuration.Common.AddEventType("MyLevel2", myLevel2);

            // create a named map
            IDictionary<string, object> namedDef = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"n0", typeof(int)}
                });
            configuration.Common.AddEventType("MyNamedMap", namedDef);

            // create a map using the name
            IDictionary<string, object> eventDef = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"P0", "MyNamedMap"},
                    new object[] {"P1", "MyNamedMap[]"}
                });
            configuration.Common.AddEventType("MyMapWithAMap", eventDef);

            // test map containing first-level property that is an array of primitive or Class
            IDictionary<string, object> arrayDef = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"P0", typeof(int[])},
                    new object[] {"P1", typeof(SupportBean[])}
                });
            configuration.Common.AddEventType("MyArrayMap", arrayDef);

            // test map at the second level of a nested map that is an array of primitive or Class
            IDictionary<string, object> arrayDefOuter = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"outer", arrayDef}
                });
            configuration.Common.AddEventType("MyArrayMapOuter", arrayDefOuter);

            // test map containing first-level property that is an array of primitive or Class
            IDictionary<string, object> mappedDef = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"P0", typeof(IDictionary<string, object>)}
                });
            configuration.Common.AddEventType("MyMappedPropertyMap", mappedDef);

            // test map at the second level of a nested map that is an array of primitive or Class
            IDictionary<string, object> mappedDefOuter = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"outer", mappedDef}
                });
            configuration.Common.AddEventType("MyMappedPropertyMapOuter", mappedDefOuter);

            IDictionary<string, object> mappedDefOuterTwo = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"outerTwo", typeof(SupportBeanComplexProps)}
                });
            configuration.Common.AddEventType("MyMappedPropertyMapOuterTwo", mappedDefOuterTwo);

            // create a named map
            IDictionary<string, object> myNamedMap = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"n0", typeof(int)}
                });
            configuration.Common.AddEventType("MyNamedMap", myNamedMap);

            // create a map using the name
            IDictionary<string, object> myMapWithAMap = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"P0", "MyNamedMap"},
                    new object[] {"P1", "MyNamedMap[]"}
                });
            configuration.Common.AddEventType("MyMapWithAMap", myMapWithAMap);

            // test named-map at the second level of a nested map
            IDictionary<string, object> myArrayMapTwo = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"outer", myMapWithAMap}
                });
            configuration.Common.AddEventType("MyArrayMapTwo", myArrayMapTwo);

            configuration.Common.AddEventType("MapType", Collections.SingletonDataMap("im", typeof(string)));
            configuration.Common.AddEventType(
                "OAType",
                new[] {"p0", "p1", "p2", "p3"},
                new object[]
                    {typeof(string), "MapType", "MapType[]", Collections.SingletonDataMap("om", typeof(string))});

            IDictionary<string, object> definition = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"a.b", typeof(int)},
                    new object[] {"a.b.c", typeof(int)},
                    new object[] {"nes.", typeof(int)},
                    new object[] {
                        "nes.nes2", EventMapCore.MakeMap(
                            new object[][] {
                                new object[] {"x.y", typeof(int)}
                            })
                    }
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
    }
} // end of namespace