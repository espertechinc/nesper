///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestEnumExceptIntersectUnion
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();

            config.AddEventType("SupportBean_ST0_Container", typeof (SupportBean_ST0_Container));
            config.AddEventType("SupportBean_ST0", typeof (SupportBean_ST0));
            config.AddEventType("SupportBean", typeof (SupportBean));
            config.AddEventType("SupportCollection", typeof (SupportCollection));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        #endregion

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        public void RunAssertionInheritance(EventRepresentationEnum eventRepresentationEnum)
        {
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema BaseEvent as (b1 string)");
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema SubEvent as (s1 string) inherits BaseEvent");
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema OuterEvent as (bases BaseEvent[], subs SubEvent[])");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " select bases.union(subs) as val from OuterEvent");

            stmt.Events += _listener.Update;

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(
                    new Object[] {
                        new Object[][] { new Object[] { "b10" } },
                        new Object[][] { new Object[] { "b10", "s10" } }
                    }, "OuterEvent");
            }
            else
            {
                IDictionary<String, Object> baseEvent = MakeMap("b1", "b10");
                IDictionary<String, Object> subEvent = MakeMap("s1", "s10");
                IDictionary<String, Object> outerEvent = MakeMap(
                    "bases", new Map[] { baseEvent }, 
                    "subs",  new Map[] { subEvent });

                _epService.EPRuntime.SendEvent(outerEvent, "OuterEvent");
            }

            var result = (ICollection<object>) _listener.AssertOneGetNewAndReset().Get("val");
            Assert.AreEqual(2, result.Count);

            _epService.Initialize();
        }

        private void TryInvalid(String epl, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        private IDictionary<String, Object> MakeMap(String key, Object value)
        {
            IDictionary<String, Object> map = new LinkedHashMap<String, Object>();

            map.Put(key, value);
            return map;
        }

        private IDictionary<String, Object> MakeMap(String key, Object value, String key2, Object value2)
        {
            IDictionary<String, Object> map = MakeMap(key, value);

            map.Put(key2, value2);
            return map;
        }

        [Test]
        public void TestStringArrayIntersection() 
        {
            String epl =
                "create objectarray schema Event(meta1 string[], meta2 string[]);\n" +
                "@Name('Out') select * from Event(meta1.intersect(meta2).countOf() > 0);\n";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            _epService.EPAdministrator.GetStatement("Out").AddListener(_listener);

            SendAndAssert("a,b", "a,b", true);
            SendAndAssert("c,d", "a,b", false);
            SendAndAssert("c,d", "a,d", true);
            SendAndAssert("a,d,a,a", "b,c", false);
            SendAndAssert("a,d,a,a", "b,d", true);
        }

        [Test]
        public void TestInheritance()
        {
            RunAssertionInheritance(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionInheritance(EventRepresentationEnum.MAP);
            RunAssertionInheritance(EventRepresentationEnum.DEFAULT);
        }

        [Test]
        public void TestInvalid()
        {
            String epl;

            epl = "select contained.union(true) from SupportBean_ST0_Container";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'contained.union(true)': Enumeration method 'union' requires an expression yielding an event-collection as input paramater [select contained.union(true) from SupportBean_ST0_Container]");

            epl = "select contained.union(prevwindow(s1)) from SupportBean_ST0_Container.std:lastevent(), SupportBean.win:keepall() s1";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'contained.union(prevwindow(s1))': Enumeration method 'union' expects event type 'SupportBean_ST0' but receives event type 'SupportBean' [select contained.union(prevwindow(s1)) from SupportBean_ST0_Container.std:lastevent(), SupportBean.win:keepall() s1]");
        }

        [Test]
        public void TestSetLogicWithContained()
        {
            String epl = "select " + "contained.except(containedTwo) as val0,"
                         + "contained.intersect(containedTwo) as val1, "
                         + "contained.union(containedTwo) as val2 "
                         + " from SupportBean_ST0_Container";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);

            stmt.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val0".Split(','),
                                            new Type[] {typeof (ICollection<object>)}
                );

            List<SupportBean_ST0> first = SupportBean_ST0_Container.Make2ValueList(
                "E1,1", "E2,10", "E3,1", "E4,10", "E5,11");
            List<SupportBean_ST0> second = SupportBean_ST0_Container.Make2ValueList(
                "E1,1", "E3,1", "E4,10");

            _epService.EPRuntime.SendEvent(
                new SupportBean_ST0_Container(first, second));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E2,E5");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E1,E3,E4");
            LambdaAssertionUtil.AssertST0Id(_listener, "val2",
                                            "E1,E2,E3,E4,E5,E1,E3,E4");
            _listener.Reset();
        }

        [Test]
        public void TestSetLogicWithEvents()
        {
            String epl = "expression last10A {"
                         + " (select * from SupportBean_ST0(key0 like 'A%').win:length(2)) "
                         + "}" + "expression last10NonZero {"
                         + " (select * from SupportBean_ST0(p00 > 0).win:length(2)) "
                         + "}" + "select " + "last10A().except(last10NonZero()) as val0,"
                         + "last10A().intersect(last10NonZero()) as val1, "
                         + "last10A().union(last10NonZero()) as val2 "
                         + "from SupportBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);

            stmt.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val0".Split(','),
                                            new Type[] {typeof (ICollection<object>)}
                );

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", "A1", 10)); // in both
            _epService.EPRuntime.SendEvent(
                new SupportBean());
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val2", "E1,E1");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E2", "A1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean());
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E2");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val2", "E1,E2,E1");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E3", "B1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean());
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E2");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val2", "E1,E2,E1");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E4", "A2", -1));
            _epService.EPRuntime.SendEvent(new SupportBean());
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E2,E4");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "");
            LambdaAssertionUtil.AssertST0Id(_listener, "val2", "E2,E4,E1");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E5", "A3", -2));
            _epService.EPRuntime.SendEvent(new SupportBean());
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E4,E5");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "");
            LambdaAssertionUtil.AssertST0Id(_listener, "val2", "E4,E5,E1");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E6", "A6", 11)); // in both
            _epService.EPRuntime.SendEvent(
                new SupportBean_ST0("E7", "A7", 12)); // in both
            _epService.EPRuntime.SendEvent(new SupportBean());
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E6,E7");
            LambdaAssertionUtil.AssertST0Id(_listener, "val2", "E6,E7,E6,E7");
            _listener.Reset();
        }

        [Test]
        public void TestSetLogicWithScalar()
        {
            String epl = "select "
                + "Strvals.except(Strvalstwo) as val0,"
                + "Strvals.intersect(Strvalstwo) as val1, "
                + "Strvals.union(Strvalstwo) as val2 "
                + " from SupportCollection as bean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);

            stmt.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val0".Split(','),
                                            new Type[] {typeof (ICollection<object>)}
                );

            _epService.EPRuntime.SendEvent(
                SupportCollection.MakeString("E1,E2", "E3,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E1", "E2");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val1");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val2", "E1", "E2",
                                                        "E3", "E4");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(
                SupportCollection.MakeString(null, "E3,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0",
                                                        null);
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val1",
                                                        null);
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val2",
                                                        null);
            _listener.Reset();

            _epService.EPRuntime.SendEvent(
                SupportCollection.MakeString("", "E3,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val1");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val2", "E3", "E4");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(
                SupportCollection.MakeString("E1,E3,E5", "E3,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E1", "E5");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val1", "E3");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val2", "E1", "E3",
                                                        "E5", "E3", "E4");
            _listener.Reset();
        }

        [Test]
        public void TestUnionWhere()
        {
            String epl = "expression one {"
                         + "  x => x.contained.Where(y => p00 = 10)" + "} " + ""
                         + "expression two {" + "  x => x.contained.Where(y => p00 = 11)"
                         + "} " + ""
                         + "select one(bean).union(two(bean)) as val0 from SupportBean_ST0_Container as bean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);

            stmt.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val0".Split(','),
                                            new Type[] {typeof (ICollection<object>)}
                );

            _epService.EPRuntime.SendEvent(
                SupportBean_ST0_Container.Make2Value("E1,1", "E2,10", "E3,1",
                                                     "E4,10", "E5,11"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E2,E4,E5");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(
                SupportBean_ST0_Container.Make2Value("E1,10", "E2,1", "E3,1"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E1");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(
                SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E3,10",
                                                     "E4,11"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E3,E4");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(
                SupportBean_ST0_Container.Make2Value(null));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", null);
            _listener.Reset();

            _epService.EPRuntime.SendEvent(
                SupportBean_ST0_Container.Make2Value());
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "");
            _listener.Reset();
        }

        private void SendAndAssert(String metaOne, String metaTwo, bool expected)
        {
            _epService.EPRuntime.SendEvent(new Object[] { metaOne.SplitCsv(), metaTwo.SplitCsv() }, "Event");
            Assert.AreEqual(expected, _listener.IsInvokedAndReset());
        }
    }
}
