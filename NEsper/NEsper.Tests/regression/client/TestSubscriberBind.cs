///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestSubscriberBind
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            String pkg = typeof (SupportBean).Namespace;

            config.AddEventTypeAutoName(pkg);
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        #endregion

        private EPServiceProvider _epService;
        private readonly String[] _fields = "TheString,IntPrimitive".Split(',');

        private void RunAssertionOutputLimitNoJoin(EventRepresentationEnum eventRepresentationEnum)
        {
            var subscriber = new MySubscriberRowByRowSpecific();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " select TheString, IntPrimitive from SupportBean output every 2 events");

            stmt.Subscriber = subscriber;
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(),
                            stmt.EventType.UnderlyingType);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(0, subscriber.GetAndResetIndicate().Count);
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertEqualsExactOrder(new Object[][]
            {
                new Object[]
                {
                    "E1", 1
                }
                ,
                new Object[]
                {
                    "E2", 2
                }
            }
                                                   , subscriber.GetAndResetIndicate());

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionBindObjectArr(EventRepresentationEnum eventRepresentationEnum)
        {
            var subscriber = new MySubscriberMultirowObjectArr();
            String stmtText = eventRepresentationEnum.GetAnnotationText()
                              + " select irstream TheString, IntPrimitive from "
                              + typeof (SupportBean).FullName + ".win:length_batch(2)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Subscriber = subscriber;
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(),
                            stmt.EventType.UnderlyingType);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(0, subscriber.IndicateArr.Count);
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual(1, subscriber.IndicateArr.Count);
            UniformPair<Object[][]> result = subscriber.GetAndResetIndicateArr()[0];

            Assert.IsNull(result.Second);
            Assert.AreEqual(2, result.First.Length);
            EPAssertionUtil.AssertEqualsExactOrder(
                result.First, _fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E1", 1
                    }
                    ,
                    new Object[]
                    {
                        "E2", 2
                    }
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.AreEqual(0, subscriber.IndicateArr.Count);
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.AreEqual(1, subscriber.IndicateArr.Count);
            result = subscriber.GetAndResetIndicateArr()[0];
            Assert.AreEqual(2, result.First.Length);
            Assert.AreEqual(2, result.Second.Length);
            EPAssertionUtil.AssertEqualsExactOrder(
                result.First, _fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E3", 3
                    }
                    ,
                    new Object[]
                    {
                        "E4", 4
                    }
                }
                );
            EPAssertionUtil.AssertEqualsExactOrder(
                result.Second, _fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E1", 1
                    }
                    ,
                    new Object[]
                    {
                        "E2", 2
                    }
                }
                );

            _epService.EPAdministrator.DestroyAllStatements();
        }

        public void RunAssertBindMap(EventRepresentationEnum eventRepresentationEnum)
        {
            var subscriber = new MySubscriberMultirowMap();
            String stmtText = eventRepresentationEnum.GetAnnotationText()
                              + " select irstream TheString, IntPrimitive from "
                              + typeof (SupportBean).FullName + ".win:length_batch(2)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Subscriber = subscriber;
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(),
                            stmt.EventType.UnderlyingType);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(0, subscriber.IndicateMap.Count);
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual(1, subscriber.IndicateMap.Count);
            UniformPair<Map[]> result = subscriber.GetAndResetIndicateMap()[0];

            Assert.IsNull(result.Second);
            Assert.AreEqual(2, result.First.Length);
            EPAssertionUtil.AssertPropsPerRow(
                result.First, _fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E1", 1
                    }
                    ,
                    new Object[]
                    {
                        "E2", 2
                    }
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.AreEqual(0, subscriber.IndicateMap.Count);
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.AreEqual(1, subscriber.IndicateMap.Count);
            result = subscriber.GetAndResetIndicateMap()[0];
            Assert.AreEqual(2, result.First.Length);
            Assert.AreEqual(2, result.Second.Length);
            EPAssertionUtil.AssertPropsPerRow(
                result.First, _fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E3", 3
                    }
                    ,
                    new Object[]
                    {
                        "E4", 4
                    }
                }
                );
            EPAssertionUtil.AssertPropsPerRow(
                result.Second, _fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E1", 1
                    }
                    ,
                    new Object[]
                    {
                        "E2", 2
                    }
                }
                );

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionWidening(EventRepresentationEnum eventRepresentationEnum)
        {
            var subscriber = new MySubscriberRowByRowSpecific();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " select BytePrimitive, IntPrimitive, LongPrimitive, FloatPrimitive from SupportBean(TheString='E1')");

            stmt.Subscriber = subscriber;
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(),
                            stmt.EventType.UnderlyingType);

            var bean = new SupportBean();

            bean.TheString = "E1";
            bean.BytePrimitive = 1;
            bean.IntPrimitive = 2;
            bean.LongPrimitive = 3;
            bean.FloatPrimitive = 4;
            _epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]
            {
                1, 2L, 3d, 4d
            }
                                                   , subscriber.GetAndResetIndicate()[0]);

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionObjectArrayDelivery(EventRepresentationEnum eventRepresentationEnum)
        {
            var subscriber = new MySubscriberRowByRowObjectArr();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " select TheString, IntPrimitive from SupportBean.std:unique(TheString)");

            stmt.Subscriber = subscriber;
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(),
                            stmt.EventType.UnderlyingType);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertEqualsAnyOrder(
                subscriber.GetAndResetIndicate()[0], new Object[]
                {
                    "E1", 1
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            EPAssertionUtil.AssertEqualsAnyOrder(
                subscriber.GetAndResetIndicate()[0], new Object[]
                {
                    "E2", 10
                }
                );

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionRowMapDelivery(EventRepresentationEnum eventRepresentationEnum)
        {
            var subscriber = new MySubscriberRowByRowMap();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " select irstream TheString, IntPrimitive from SupportBean.std:unique(TheString)");

            stmt.Subscriber = subscriber;
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(),
                            stmt.EventType.UnderlyingType);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsMap(
                subscriber.GetAndResetIndicateIStream()[0], _fields,
                new Object[]
                {
                    "E1", 1
                }
                );
            Assert.AreEqual(0, subscriber.GetAndResetIndicateRStream().Count);

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            EPAssertionUtil.AssertPropsMap(
                subscriber.GetAndResetIndicateIStream()[0], _fields,
                new Object[]
                {
                    "E2", 10
                }
                );
            Assert.AreEqual(0, subscriber.GetAndResetIndicateRStream().Count);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertPropsMap(
                subscriber.GetAndResetIndicateIStream()[0], _fields,
                new Object[]
                {
                    "E1", 2
                }
                );
            EPAssertionUtil.AssertPropsMap(
                subscriber.GetAndResetIndicateRStream()[0], _fields,
                new Object[]
                {
                    "E1", 1
                }
                );

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private class LocalSubscriberNoParams
        {
            public LocalSubscriberNoParams()
            {
                IsCalled = false;
            }

            public bool IsCalled { get; set; }

            public void Update()
            {
                IsCalled = true;
            }
        }

        [Test]
        public void TestBindMap()
        {
            RunAssertBindMap(EventRepresentationEnum.OBJECTARRAY);
            RunAssertBindMap(EventRepresentationEnum.MAP);
            RunAssertBindMap(EventRepresentationEnum.DEFAULT);
        }

        [Test]
        public void TestBindObjectArr()
        {
            RunAssertionBindObjectArr(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionBindObjectArr(EventRepresentationEnum.MAP);
            RunAssertionBindObjectArr(EventRepresentationEnum.DEFAULT);
        }

        [Test]
        public void TestBindUpdateIRStream()
        {
            var subscriber = new MySubscriberRowByRowFull();
            String stmtText = "select irstream TheString, IntPrimitive from "
                              + typeof (SupportBean).FullName + ".win:length_batch(2)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Subscriber = subscriber;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(0, subscriber.IndicateStart.Count);
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual(1, subscriber.IndicateStart.Count);
            UniformPair<int?> pairLength = subscriber.GetAndResetIndicateStart()[0];

            Assert.AreEqual(2, (int) pairLength.First);
            Assert.AreEqual(0, (int) pairLength.Second);
            Assert.AreEqual(1, subscriber.GetAndResetIndicateEnd().Count);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[][]
            {
                new Object[]
                {
                    "E1", 1
                }
                ,
                new Object[]
                {
                    "E2", 2
                }
            }
                                                   , subscriber.GetAndResetIndicateIStream());
            EPAssertionUtil.AssertEqualsExactOrder(null,
                                                   subscriber.GetAndResetIndicateRStream());

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.AreEqual(0, subscriber.IndicateStart.Count);
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.AreEqual(1, subscriber.IndicateStart.Count);
            pairLength = subscriber.GetAndResetIndicateStart()[0];
            Assert.AreEqual(2, (int) pairLength.First);
            Assert.AreEqual(2, (int) pairLength.Second);
            Assert.AreEqual(1, subscriber.GetAndResetIndicateEnd().Count);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[][]
            {
                new Object[]
                {
                    "E3", 3
                }
                ,
                new Object[]
                {
                    "E4", 4
                }
            }
                                                   , subscriber.GetAndResetIndicateIStream());
            EPAssertionUtil.AssertEqualsExactOrder(new Object[][]
            {
                new Object[]
                {
                    "E1", 1
                }
                ,
                new Object[]
                {
                    "E2", 2
                }
            }
                                                   , subscriber.GetAndResetIndicateRStream());
        }

        [Test]
        public void TestBindWildcardIRStream()
        {
            var subscriber = new MySubscriberMultirowUnderlying();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select irstream * from SupportBean.win:length_batch(2)");

            stmt.Subscriber = subscriber;

            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportBean("E2", 200);

            _epService.EPRuntime.SendEvent(s0);
            _epService.EPRuntime.SendEvent(s1);
            Assert.AreEqual(1, subscriber.IndicateArr.Count);
            UniformPair<SupportBean[]> beans = subscriber.GetAndResetIndicateArr()[0];

            Assert.AreEqual(2, beans.First.Length);
            Assert.AreEqual(null, beans.Second);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]
            {
                s0, s1
            }
                                                   , beans.First);

            var s2 = new SupportBean("E3", 300);
            var s3 = new SupportBean("E4", 400);

            _epService.EPRuntime.SendEvent(s2);
            _epService.EPRuntime.SendEvent(s3);
            Assert.AreEqual(1, subscriber.IndicateArr.Count);
            beans = subscriber.GetAndResetIndicateArr()[0];
            Assert.AreEqual(2, beans.First.Length);
            Assert.AreEqual(2, beans.Second.Length);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]
            {
                s2, s3
            }
                                                   , beans.First);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]
            {
                s0, s1
            }
                                                   , beans.Second);
        }

        [Test]
        public void TestBindWildcardJoin()
        {
            var subscriber = new MySubscriberRowByRowSpecific();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from SupportBean.win:keepall() as s0, SupportMarketDataBean.win:keepall() as s1 where s0.TheString = s1.symbol");

            stmt.Subscriber = subscriber;

            // send event
            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportMarketDataBean("E1", 0, 0L, "");

            _epService.EPRuntime.SendEvent(s0);
            _epService.EPRuntime.SendEvent(s1);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[][]
            {
                new Object[]
                {
                    s0, s1
                }
            }
                                                   , subscriber.GetAndResetIndicate());
        }

        [Test]
        public void TestBindWildcardPlusProperties()
        {
            var subscriber = new MySubscriberRowByRowSpecific();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select *, IntPrimitive + 2, 'x'||TheString||'x' from "
                + typeof (SupportBean).FullName);

            stmt.Subscriber = subscriber;

            var s0 = new SupportBean("E1", 100);

            _epService.EPRuntime.SendEvent(s0);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]
            {
                s0, 102, "xE1x"
            }
                                                   , subscriber.GetAndResetIndicate()[0]);
        }

        [Test]
        public void TestEnum()
        {
            var subscriber = new MySubscriberRowByRowSpecific();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select TheString, supportEnum from SupportBeanWithEnum");

            stmt.Subscriber = subscriber;

            var theEvent = new SupportBeanWithEnum("abc",
                                                   SupportEnum.ENUM_VALUE_1);

            _epService.EPRuntime.SendEvent(theEvent);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]
            {
                theEvent.TheString, theEvent.SupportEnum
            }
                                                   , subscriber.GetAndResetIndicate()[0]);
        }

        [Test]
        public void TestNested()
        {
            var subscriber = new MySubscriberRowByRowSpecific();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select nested, nested.nestedNested from SupportBeanComplexProps");

            stmt.Subscriber = subscriber;

            SupportBeanComplexProps theEvent = SupportBeanComplexProps.MakeDefaultBean();

            _epService.EPRuntime.SendEvent(theEvent);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]
            {
                theEvent.Nested, theEvent.Nested.NestedNested
            }
                                                   , subscriber.GetAndResetIndicate()[0]);
        }

        [Test]
        public void TestNullType()
        {
            var subscriber = new MySubscriberRowByRowSpecific();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select null, LongBoxed from SupportBean");

            stmt.Subscriber = subscriber;

            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertEqualsExactOrder(
                new Object[]
                {
                    null, null
                }
                , subscriber.GetAndResetIndicate()[0]);
            stmt.Dispose();

            // test null-delivery for no-parameter subscriber
            var subscriberNoParams = new LocalSubscriberNoParams();

            stmt = _epService.EPAdministrator.CreateEPL(
                "select null from SupportBean");
            stmt.Subscriber = subscriberNoParams;

            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsTrue(subscriberNoParams.IsCalled);
        }

        [Test]
        public void TestObjectArrayDelivery()
        {
            RunAssertionObjectArrayDelivery(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionObjectArrayDelivery(EventRepresentationEnum.DEFAULT);
            RunAssertionObjectArrayDelivery(EventRepresentationEnum.MAP);
        }

        [Test]
        public void TestOutputLimitJoin()
        {
            var subscriber = new MySubscriberRowByRowSpecific();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select TheString, IntPrimitive from SupportBean.win:keepall(), SupportMarketDataBean.win:keepall() where symbol = TheString output every 2 events");

            stmt.Subscriber = subscriber;

            _epService.EPRuntime.SendEvent(
                new SupportMarketDataBean("E1", 0, 1L, ""));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(0, subscriber.GetAndResetIndicate().Count);
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertEqualsExactOrder(new Object[][]
            {
                new Object[]
                {
                    "E1", 1
                }
                ,
                new Object[]
                {
                    "E1", 2
                }
            }
                                                   , subscriber.GetAndResetIndicate());
        }

        [Test]
        public void TestOutputLimitNoJoin()
        {
            RunAssertionOutputLimitNoJoin(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionOutputLimitNoJoin(EventRepresentationEnum.MAP);
            RunAssertionOutputLimitNoJoin(EventRepresentationEnum.DEFAULT);
        }

        [Test]
        public void TestRStreamSelect()
        {
            var subscriber = new MySubscriberRowByRowSpecific();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select rstream s0 from SupportBean.std:unique(TheString) as s0");

            stmt.Subscriber = subscriber;

            // send event
            var s0 = new SupportBean("E1", 100);

            _epService.EPRuntime.SendEvent(s0);
            Assert.AreEqual(0, subscriber.GetAndResetIndicate().Count);

            var s1 = new SupportBean("E2", 200);

            _epService.EPRuntime.SendEvent(s1);
            Assert.AreEqual(0, subscriber.GetAndResetIndicate().Count);

            var s2 = new SupportBean("E1", 300);

            _epService.EPRuntime.SendEvent(s2);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[][]
            {
                new Object[]
                {
                    s0
                }
            }
                                                   , subscriber.GetAndResetIndicate());
        }

        [Test]
        public void TestRowMapDelivery()
        {
            RunAssertionRowMapDelivery(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionRowMapDelivery(EventRepresentationEnum.DEFAULT);
            RunAssertionRowMapDelivery(EventRepresentationEnum.MAP);
        }

        [Test]
        public void TestSimpleSelectStatic()
        {
            var subscriber = new MySubscriberRowByRowSpecificStatic();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select TheString, IntPrimitive from "
                + typeof (SupportBean).FullName);

            stmt.Subscriber = subscriber;

            // send event
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            EPAssertionUtil.AssertEqualsExactOrder(new Object[][]
            {
                new Object[]
                {
                    "E1", 100
                }
            }
                                                   , subscriber.GetAndResetIndicate());
        }

        [Test]
        public void TestStreamSelectJoin()
        {
            var subscriber = new MySubscriberRowByRowSpecific();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select null, s1, s0 from SupportBean.win:keepall() as s0, SupportMarketDataBean.win:keepall() as s1 where s0.TheString = s1.symbol");

            stmt.Subscriber = subscriber;

            // send event
            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportMarketDataBean("E1", 0, 0L, "");

            _epService.EPRuntime.SendEvent(s0);
            _epService.EPRuntime.SendEvent(s1);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[][]
            {
                new Object[]
                {
                    null, s1, s0
                }
            }
                                                   , subscriber.GetAndResetIndicate());
        }

        [Test]
        public void TestStreamWildcardJoin()
        {
            var subscriber = new MySubscriberRowByRowSpecific();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select TheString || '<', s1.* as s1, s0.* as s0 from SupportBean.win:keepall() as s0, SupportMarketDataBean.win:keepall() as s1 where s0.TheString = s1.symbol");

            stmt.Subscriber = subscriber;

            // send event
            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportMarketDataBean("E1", 0, 0L, "");

            _epService.EPRuntime.SendEvent(s0);
            _epService.EPRuntime.SendEvent(s1);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[][]
            {
                new Object[]
                {
                    "E1<", s1, s0
                }
            }
                                                   , subscriber.GetAndResetIndicate());
        }

        [Test]
        public void TestSubscriberandListener()
        {
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBean", typeof (SupportBean));
            _epService.EPAdministrator.CreateEPL(
                "insert into A1 select s.*, 1 as a from SupportBean as s");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select a1.* from A1 as a1");

            var listener = new SupportUpdateListener();
            var subscriber = new MySubscriberRowByRowObjectArr();

            stmt.Events += listener.Update;
            stmt.Subscriber = subscriber;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

            EventBean theEvent = listener.AssertOneGetNewAndReset();

            Assert.AreEqual("E1", theEvent.Get("TheString"));
            Assert.AreEqual(1, theEvent.Get("IntPrimitive"));
            Assert.IsTrue(theEvent.Underlying is Pair<object,IDictionary<string,object>>);

            foreach (String property in stmt.EventType.PropertyNames)
            {
                EventPropertyGetter getter = stmt.EventType.GetGetter(property);

                getter.Get(theEvent);
            }
        }

        [Test]
        public void TestWidening()
        {
            RunAssertionWidening(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionBindObjectArr(EventRepresentationEnum.MAP);
            RunAssertionBindObjectArr(EventRepresentationEnum.DEFAULT);
        }

        [Test]
        public void TestWildcard()
        {
            var subscriber = new MySubscriberRowByRowSpecific();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from SupportBean(TheString='E2')");

            stmt.Subscriber = subscriber;

            var theEvent = new SupportBean("E2", 1);

            _epService.EPRuntime.SendEvent(theEvent);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]
            {
                theEvent
            }
                                                   , subscriber.GetAndResetIndicate()[0]);
        }
    }
}
