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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lrreport;
using com.espertech.esper.support.bean.sales;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    [TestFixture]
    public class TestEnumNested
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddImport(typeof(LocationReportFactory));
            config.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            config.AddEventType("PersonSales", typeof(PersonSales));
            config.AddEventType("LocationReport", typeof(LocationReport));
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

        [Test]
        public void TestEquivalentToMinByUncorrelated()
        {
            String eplFragment = "select contained.Where(x => (x.P00 = contained.min(y => y.P00))) as val from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;

            SupportBean_ST0_Container bean = SupportBean_ST0_Container.Make2Value("E1,2", "E2,1", "E3,2");
            _epService.EPRuntime.SendEvent(bean);
            var result = (ICollection<object>)_listener.AssertOneGetNewAndReset().Get("val");
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { bean.Contained[1] }, result.ToArray());
        }

        [Test]
        public void TestMinByWhere()
        {
            String eplFragment = "select sales.Where(x => x.buyer = persons.minBy(y => age)) as val from PersonSales";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;

            PersonSales bean = PersonSales.Make();
            _epService.EPRuntime.SendEvent(bean);

            var sales = (ICollection<object>)_listener.AssertOneGetNewAndReset().Get("val");
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { bean.Sales[0] }, sales.ToArray());
        }

        [Test]
        public void TestCorrelated()
        {
            String eplFragment = "select contained.where(x => x = (contained.firstOf(y => y.P00 = x.P00 ))) as val from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;

            SupportBean_ST0_Container bean = SupportBean_ST0_Container.Make2Value("E1,2", "E2,1", "E3,3");
            _epService.EPRuntime.SendEvent(bean);
            var result = (ICollection<object>)_listener.AssertOneGetNewAndReset().Get("val");
            Assert.AreEqual(3, result.Count);  // this would be 1 if the cache is invalid

        }

        [Test]
        public void TestAnyOf()
        {
            _epService.EPAdministrator.Configuration.AddEventType<ContainerEvent>();

            // try "in" with "Insert<String> multivalues"
            _epService.EPAdministrator.CreateEPL("select * from ContainerEvent(level1s.anyOf(x=>x.level2s.anyOf(y => 'A' in (y.multivalues))))").Events += _listener.Update;
            RunAssertionContainer();

            // try "in" with "String singlevalue"
            _epService.EPAdministrator.CreateEPL("select * from ContainerEvent(level1s.anyOf(x=>x.level2s.anyOf(y => y.singlevalue = 'A')))").Events += _listener.Update;
            RunAssertionContainer();
        }

        private void RunAssertionContainer()
        {
            _epService.EPRuntime.SendEvent(MakeContainerEvent("A"));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(MakeContainerEvent("B"));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
        }

        private ContainerEvent MakeContainerEvent(String value)
        {
            ISet<Level1Event> level1s = new LinkedHashSet<Level1Event>();
            level1s.Add(new Level1Event(new Level2Event("X1".AsSet(), "X1").AsSet()));
            level1s.Add(new Level1Event(new Level2Event(value.AsSet(), value).AsSet()));
            level1s.Add(new Level1Event(new Level2Event("X2".AsSet(), "X2").AsSet()));
            return new ContainerEvent(level1s);
        }

        public class ContainerEvent
        {
            public ContainerEvent(ISet<Level1Event> level1s) {
                this.Level1S = level1s;
            }

            public ISet<Level1Event> Level1S { get; private set; }
        }

        public class Level1Event
        {
            public Level1Event(ISet<Level2Event> level2s)
            {
                Level2s = level2s;
            }

            public ISet<Level2Event> Level2s { get; private set; }
        }

        public class Level2Event
        {
            public Level2Event(ISet<String> multivalues, String singlevalue)
            {
                Multivalues = multivalues;
                Singlevalue = singlevalue;
            }

            public ISet<string> Multivalues { get; private set; }
            public string Singlevalue { get; private set; }
        }
    }
}
