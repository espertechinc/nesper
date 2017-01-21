///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestInsertIntoPopulateCreateStream
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
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

        [Test]
        public void TestCreateStream()
        {
            RunAssertionCreateStream(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionCreateStream(EventRepresentationEnum.MAP);
            RunAssertionCreateStream(EventRepresentationEnum.DEFAULT);

            RunAssertPopulateFromNamedWindow(EventRepresentationEnum.OBJECTARRAY);
            RunAssertPopulateFromNamedWindow(EventRepresentationEnum.MAP);
            RunAssertPopulateFromNamedWindow(EventRepresentationEnum.DEFAULT);
            RunAssertionObjectArrPropertyReorder();
        }

        private void RunAssertPopulateFromNamedWindow(EventRepresentationEnum type)
        {
            _epService.EPAdministrator.CreateEPL("create " + type.GetOutputTypeCreateSchemaName() + " schema Node(nid string)");
            _epService.EPAdministrator.CreateEPL("create window NodeWindow.std:unique(nid) as Node");
            _epService.EPAdministrator.CreateEPL("insert into NodeWindow select * from Node");
            _epService.EPAdministrator.CreateEPL("create " + type.GetOutputTypeCreateSchemaName() + " schema NodePlus(npid string, node Node)");

            var stmt = _epService.EPAdministrator.CreateEPL("insert into NodePlus select 'E1' as npid, n1 as node from NodeWindow n1");
            stmt.Events += _listener.Update;

            if (type.GetOutputClass() == typeof(object[]))
            {
                _epService.EPRuntime.SendEvent(new Object[] { "n1" }, "Node");
            }
            else
            {
                _epService.EPRuntime.SendEvent(Collections.SingletonDataMap("nid", "n1"), "Node");
            }
            var @event = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E1", @event.Get("npid"));
            Assert.AreEqual("n1", @event.Get("node.nid"));
            var fragment = (EventBean)@event.GetFragment("node");
            Assert.AreEqual("Node", fragment.EventType.Name);

            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("Node", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("NodePlus", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("NodeWindow", true);
        }
        
        private void RunAssertionCreateStream(EventRepresentationEnum eventRepresentationEnum)
        {
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema MyEvent(myId int)");
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema CompositeEvent(c1 MyEvent, c2 MyEvent, rule string)");
            _epService.EPAdministrator.CreateEPL(
                "insert into MyStream select c, 'additionalValue' as value from MyEvent c");
            _epService.EPAdministrator.CreateEPL(
                "insert into CompositeEvent select e1.c as c1, e2.c as c2, '4' as rule "
                + "from pattern [e1=MyStream -> e2=MyStream]");
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " @Name('Target') select * from CompositeEvent");
            _epService.EPAdministrator.GetStatement("Target").Events += _listener.Update;

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(MakeEvent(10).Values.ToArray(), "MyEvent");
                _epService.EPRuntime.SendEvent(MakeEvent(11).Values.ToArray(), "MyEvent");
            }
            else
            {
                _epService.EPRuntime.SendEvent(MakeEvent(10), "MyEvent");
                _epService.EPRuntime.SendEvent(MakeEvent(11), "MyEvent");
            }
            var theEvent = _listener.AssertOneGetNewAndReset();

            Assert.AreEqual(10, theEvent.Get("c1.myId"));
            Assert.AreEqual(11, theEvent.Get("c2.myId"));
            Assert.AreEqual("4", theEvent.Get("rule"));

            _epService.Initialize();
        }
        
        [Test]
        public void TestCreateStreamTwo()
        {
            RunAssertionCreateStreamTwo(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionCreateStreamTwo(EventRepresentationEnum.MAP);
            RunAssertionCreateStreamTwo(EventRepresentationEnum.DEFAULT);
        }

        private void RunAssertionCreateStreamTwo(EventRepresentationEnum eventRepresentationEnum)
        {
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema MyEvent(myId int)");
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema AllMyEvent as (myEvent MyEvent, class String, reverse boolean)");
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema SuspectMyEvent as (myEvent MyEvent, class String)");

            var stmtOne = _epService.EPAdministrator.CreateEPL(
                "insert into AllMyEvent "
                + "select c as myEvent, 'test' as class, false as reverse "
                + "from MyEvent(myId=1) c");

            stmtOne.Events += _listener.Update;
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(),
                            stmtOne.EventType.UnderlyingType);

            var stmtTwo = _epService.EPAdministrator.CreateEPL(
                "insert into SuspectMyEvent "
                + "select c.myEvent as myEvent, class "
                + "from AllMyEvent(not reverse) c");
            var listenerTwo = new SupportUpdateListener();

            stmtTwo.Events += listenerTwo.Update;

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(MakeEvent(1).Values.ToArray(), "MyEvent");
            }
            else
            {
                _epService.EPRuntime.SendEvent(MakeEvent(1), "MyEvent");
            }

            var resultOne = _listener.AssertOneGetNewAndReset();

            Assert.IsTrue(resultOne.Get("myEvent") is EventBean);
            Assert.AreEqual(1, ((EventBean) resultOne.Get("myEvent")).Get("myId"));
            Assert.NotNull(stmtOne.EventType.GetFragmentType("myEvent"));

            var resultTwo = listenerTwo.AssertOneGetNewAndReset();

            Assert.IsTrue(resultTwo.Get("myEvent") is EventBean);
            Assert.AreEqual(1, ((EventBean) resultTwo.Get("myEvent")).Get("myId"));
            Assert.NotNull(stmtTwo.EventType.GetFragmentType("myEvent"));

            _epService.Initialize();
        }

        private static IDictionary<String, Object> MakeEvent(int myId)
        {
            return Collections.SingletonMap<string, object>("myId", myId);
        }


        private void RunAssertionObjectArrPropertyReorder()
        {
            _epService.EPAdministrator.CreateEPL("create objectarray schema MyInner (p_inner string)");
            _epService.EPAdministrator.CreateEPL("create objectarray schema MyOATarget (unfilled string, p0 string, p1 string, i0 MyInner)");
            _epService.EPAdministrator.CreateEPL("create objectarray schema MyOASource (p0 string, p1 string, i0 MyInner)");
            _epService.EPAdministrator.CreateEPL("insert into MyOATarget select p0, p1, i0, null as unfilled from MyOASource");
            _epService.EPAdministrator.CreateEPL("select * from MyOATarget").AddListener(_listener);

            _epService.EPRuntime.SendEvent(new Object[] { "p0value", "p1value", new Object[] { "i" } }, "MyOASource");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "p0,p1".Split(','), new Object[] { "p0value", "p1value" });
        }
    }
}
