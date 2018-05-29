///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lrreport;
using com.espertech.esper.supportregression.bean.sales;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumNested : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddImport(typeof(LocationReportFactory));
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("PersonSales", typeof(PersonSales));
            configuration.AddEventType("LocationReport", typeof(LocationReport));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionEquivalentToMinByUncorrelated(epService);
            RunAssertionMinByWhere(epService);
            RunAssertionCorrelated(epService);
            RunAssertionAnyOf(epService);
        }
    
        private void RunAssertionEquivalentToMinByUncorrelated(EPServiceProvider epService) {
    
            var eplFragment = "select Contained.where(x => (x.p00 = Contained.min(y => y.p00))) as val from Bean";
            var stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
    
            var bean = SupportBean_ST0_Container.Make2Value("E1,2", "E2,1", "E3,2");
            epService.EPRuntime.SendEvent(bean);
            var result = listener.AssertOneGetNewAndReset().Get("val").Unwrap<SupportBean_ST0>();
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{bean.Contained[1]}, result.ToArray());
        }
    
        private void RunAssertionMinByWhere(EPServiceProvider epService) {
    
            var eplFragment = "select Sales.where(x => x.buyer = persons.minBy(y => age)) as val from PersonSales";
            var stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
    
            var bean = PersonSales.Make();
            epService.EPRuntime.SendEvent(bean);

            var sales = listener.AssertOneGetNewAndReset().Get("val").UnwrapIntoList<Sale>();
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{bean.Sales[0]}, sales.ToArray());
        }
    
        private void RunAssertionCorrelated(EPServiceProvider epService) {
    
            var eplFragment = "select Contained.where(x => x = (Contained.firstOf(y => y.p00 = x.p00 ))) as val from Bean";
            var stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
    
            var bean = SupportBean_ST0_Container.Make2Value("E1,2", "E2,1", "E3,3");
            epService.EPRuntime.SendEvent(bean);
            var result = listener.AssertOneGetNewAndReset().Get("val").Unwrap<SupportBean_ST0>();
            Assert.AreEqual(3, result.Count);  // this would be 1 if the cache is invalid
        }
    
        private void RunAssertionAnyOf(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(ContainerEvent));
            var listener = new SupportUpdateListener();
    
            // try "in" with "ISet<string> multivalues"
            epService.EPAdministrator.CreateEPL("select * from ContainerEvent(level1s.anyOf(x=>x.level2s.anyOf(y => 'A' in (y.multivalues))))").Events += listener.Update;
            TryAssertionAnyOf(epService, listener);
            epService.EPAdministrator.DestroyAllStatements();
    
            // try "in" with "string singlevalue"
            epService.EPAdministrator.CreateEPL("select * from ContainerEvent(level1s.anyOf(x=>x.level2s.anyOf(y => y.singlevalue = 'A')))").Events += listener.Update;
            TryAssertionAnyOf(epService, listener);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionAnyOf(EPServiceProvider epService, SupportUpdateListener listener) {
            epService.EPRuntime.SendEvent(MakeContainerEvent("A"));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(MakeContainerEvent("B"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
        }
    
        private ContainerEvent MakeContainerEvent(string value) {
            var level1s = new LinkedHashSet<Level1Event>();
            level1s.Add(new Level1Event(Collections.SingletonSet(new Level2Event(Collections.SingletonSet("X1"), "X1"))));
            level1s.Add(new Level1Event(Collections.SingletonSet(new Level2Event(Collections.SingletonSet(value), value))));
            level1s.Add(new Level1Event(Collections.SingletonSet(new Level2Event(Collections.SingletonSet("X2"), "X2"))));
            return new ContainerEvent(level1s);
        }
    
        public class ContainerEvent {
            private readonly ISet<Level1Event> _level1S;
    
            public ContainerEvent(ISet<Level1Event> level1s) {
                _level1S = level1s;
            }

            public ISet<Level1Event> Level1s => _level1S;
        }
    
        public class Level1Event {
            private readonly ISet<Level2Event> _level2S;
    
            public Level1Event(ISet<Level2Event> level2s) {
                _level2S = level2s;
            }

            public ISet<Level2Event> Level2s => _level2S;
        }
    
        public class Level2Event {
            private readonly ISet<string> _multivalues;
            private readonly string _singlevalue;
    
            public Level2Event(ISet<string> multivalues, string singlevalue) {
                _multivalues = multivalues;
                _singlevalue = singlevalue;
            }

            public ISet<string> Multivalues => _multivalues;

            public string Singlevalue => _singlevalue;
        }
    }
} // end of namespace
