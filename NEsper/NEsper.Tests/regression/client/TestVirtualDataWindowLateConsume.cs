///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.virtualdw;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestVirtualDataWindowLateConsume
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();

            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddPlugInVirtualDataWindow("test", "vdw", typeof(SupportVirtualDWFactory).FullName, SupportVirtualDW.ITERATE);    // configure with iteration
            configuration.AddEventType("SupportBean", typeof(SupportBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestInsertConsume()
        {
            _epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw() as SupportBean");
            var window = (SupportVirtualDW)GetFromContext("/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            window.Data = supportBean.AsSingleton();
            _epService.EPAdministrator.CreateEPL("insert into MyVDW select * from SupportBean");

            // test aggregated consumer - wherein the virtual data window does not return an iterator that prefills the aggregation state
            var fields = "val0".Split(',');
            var stmtAggregate = _epService.EPAdministrator.CreateEPL("@Name('ABC') select sum(IntPrimitive) as val0 from MyVDW");
            stmtAggregate.Events += _listener.Update;
            EPAssertionUtil.AssertProps(stmtAggregate.First(), fields, new Object[] { 100 });

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 110 });

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 130 });

            // assert events received for add-consumer and remove-consumer
            stmtAggregate.Dispose();
            var addConsumerEvent = (VirtualDataWindowEventConsumerAdd) window.Events[0];
            var removeConsumerEvent = (VirtualDataWindowEventConsumerRemove) window.Events[1];

            foreach (var @base in new VirtualDataWindowEventConsumerBase[] {addConsumerEvent, removeConsumerEvent}) {
                Assert.AreEqual(-1, @base.AgentInstanceId);
                Assert.AreEqual("MyVDW", @base.NamedWindowName);
                Assert.AreEqual("ABC", @base.StatementName);
            }
            Assert.AreSame(removeConsumerEvent.ConsumerObject, addConsumerEvent.ConsumerObject);
            window.Events.Clear();

            // test filter criteria passed to event
            var stmtAggregateWFilter = _epService.EPAdministrator.CreateEPL("@Name('ABC') select sum(IntPrimitive) as val0 from MyVDW(TheString = 'A')");
            var eventWithFilter = (VirtualDataWindowEventConsumerAdd) window.Events[0];
            Assert.AreEqual(1, eventWithFilter.FilterExpressions.Length);
            Assert.IsNotNull(eventWithFilter.ExprEvaluatorContext);
            stmtAggregateWFilter.Dispose();
        }

        private VirtualDataWindow GetFromContext(String name)
        {
            return (VirtualDataWindow)_epService.Directory.Lookup(name);
        }
    }
}
