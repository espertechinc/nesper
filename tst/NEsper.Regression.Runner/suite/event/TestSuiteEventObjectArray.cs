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
    public class TestSuiteEventObjectArray : AbstractTestBase
    {
        public TestSuiteEventObjectArray() : base(Configure)
        {
        }

        [Test, RunInApplicationDomain]
        public void TestEventObjectArrayCore()
        {
            RegressionRunner.Run(_session, EventObjectArrayCore.Executions());
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

        [Test, RunInApplicationDomain]
        public void TestEventObjectArrayEventNested()
        {
            RegressionRunner.Run(_session, EventObjectArrayEventNested.Executions());
        }

        public static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[] { typeof(SupportBean) })
            {
                configuration.Common.AddEventType(clazz);
            }

            string[] myObjectArrayEvent = { "MyInt", "MyString", "beanA" };
            object[] MyObjectArrayTypes = { typeof(int?), typeof(string), typeof(SupportBeanComplexProps) };
            configuration.Common.AddEventType("MyObjectArrayEvent", myObjectArrayEvent, MyObjectArrayTypes);

            string[] myArrayOAProps = { "P0", "P1" };
            object[] MyArrayOATypes = { typeof(int[]), typeof(SupportBean[]) };
            configuration.Common.AddEventType("MyArrayOA", myArrayOAProps, MyArrayOATypes);

            configuration.Common.AddEventType("MyArrayOAMapOuter", new string[] { "outer" }, new object[] { "MyArrayOA" });

            IDictionary<string, object> mappedDef = MakeMap(new object[][] {
                new object[]{"P0", typeof(IDictionary<string, object>)}
            });
            configuration.Common.AddEventType("MyMappedPropertyMap", mappedDef);

            IDictionary<string, object> mappedDefOuter = MakeMap(new object[][] {
                new object[]{"outer", mappedDef}
            });
            configuration.Common.AddEventType("MyMappedPropertyMapOuter", mappedDefOuter);

            IDictionary<string, object> mappedDefOuterTwo = MakeMap(new object[][] {
                new object[]{"outerTwo", typeof(SupportBeanComplexProps)}
            });
            configuration.Common.AddEventType("MyMappedPropertyMapOuterTwo", mappedDefOuterTwo);

            IDictionary<string, object> namedDef = MakeMap(new object[][] {
                new object[]{"n0", typeof(int)}
            });
            configuration.Common.AddEventType("MyNamedMap", namedDef);

            IDictionary<string, object> eventDef = MakeMap(
                new object[][] {
                    new object[] {"p0", "MyNamedMap"},
                    new object[] {"p1", "MyNamedMap[]"}
                });
            configuration.Common.AddEventType("MyMapWithAMap", eventDef);
            configuration.Common.AddEventType("MyObjectArrayMapOuter",
                new string[] { "outer" }, new object[] { eventDef });

            configuration.Common.AddEventType("MyOAWithAMap", new string[] { "p0", "p1" }, new object[] { "MyNamedMap", "MyNamedMap[]" });

            configuration.Common.AddEventType("TypeLev1", new string[] { "p1id" }, new object[] { typeof(int) });
            configuration.Common.AddEventType("TypeLev0", new string[] { "p0id", "p1" }, new object[] { typeof(int), "TypeLev1" });
            configuration.Common.AddEventType("TypeRoot", new string[] { "rootId", "p0" }, new object[] { typeof(int), "TypeLev0" });

            Pair<string[], object[]> pair = GetTestDef();
            configuration.Common.AddEventType("NestedObjectArr", pair.First, pair.Second);

            configuration.Common.AddEventType("MyNested", new string[] { "bean" }, new object[] { typeof(EventObjectArrayEventNestedPono.MyNested) });

            configuration.Common.AddEventType("RootEvent", new string[] { "base" }, new object[] { typeof(string) });
            configuration.Common.AddEventType("Sub1Event", new string[] { "sub1" }, new object[] { typeof(string) });
            configuration.Common.AddEventType("Sub2Event", new string[] { "sub2" }, new object[] { typeof(string) });
            configuration.Common.AddEventType("SubAEvent", new string[] { "suba" }, new object[] { typeof(string) });
            configuration.Common.AddEventType("SubBEvent", new string[] { "subb" }, new object[] { typeof(string) });

            configuration.Common.AddObjectArraySuperType("Sub1Event", "RootEvent");
            configuration.Common.AddObjectArraySuperType("Sub2Event", "RootEvent");
            configuration.Common.AddObjectArraySuperType("SubAEvent", "Sub1Event");
            configuration.Common.AddObjectArraySuperType("SubBEvent", "SubAEvent");

            IDictionary<string, object> nestedOALev2def = new Dictionary<string, object>();
            nestedOALev2def.Put("sb", "SupportBean");
            IDictionary<string, object> nestedOALev1def = new Dictionary<string, object>();
            nestedOALev1def.Put("lev1name", nestedOALev2def);
            configuration.Common.AddEventType("MyMapNestedObjectArray", new string[] { "lev0name" }, new object[] { nestedOALev1def });
        }

        private static Pair<string[], object[]> GetTestDef()
        {
            IDictionary<string, object> levelThree = MakeMap(new object[][]{
                new object[]{"SimpleThree", typeof(long?)},
                new object[]{"ObjectThree", typeof(SupportBean_B)},
            });

            IDictionary<string, object> levelTwo = MakeMap(new object[][]{
                new object[]{"SimpleTwo", typeof(int?)},
                new object[]{"ObjectTwo", typeof(SupportBeanCombinedProps)},
                new object[]{"NodefmapTwo", typeof(IDictionary<string, object>)},
                new object[]{"MapTwo", levelThree},
            });

            IDictionary<string, object> levelOne = MakeMap(new object[][]{
                new object[]{"SimpleOne", typeof(int?)},
                new object[]{"ObjectOne", typeof(SupportBeanComplexProps)},
                new object[]{"NodefmapOne", typeof(IDictionary<string, object>)},
                new object[]{"MapOne", levelTwo}
            });

            string[] levelZeroProps = {
                "Simple",
                "Object",
                "Nodefmap", 
                "Map"
            };
            object[] levelZeroTypes = {
                typeof(string),
                typeof(SupportBean_A),
                typeof(IDictionary<string, object>),
                levelOne
            };
            return new Pair<string[], object[]>(levelZeroProps, levelZeroTypes);
        }
    }
} // end of namespace