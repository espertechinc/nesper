///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.core.eval;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.epl.core
{
    [TestFixture]
    public class TestResultSetProcessorSimple 
    {
        private ResultSetProcessorSimple _outputProcessorAll;
        private SelectExprProcessor _selectExprProcessor;
        private OrderByProcessor _orderByProcessor;

        [SetUp]
        public void SetUp()
        {
            var container = SupportContainer.Instance;
            var selectExprEventTypeRegistry = new SelectExprEventTypeRegistry("abc", new StatementEventTypeRefImpl(
                container.Resolve<IReaderWriterLockManager>()));
            var statementContext = SupportStatementContextFactory.MakeContext(container);
    
            var factory = new SelectExprProcessorHelper(
                Collections.GetEmptyList<int>(), SupportSelectExprFactory.MakeNoAggregateSelectList(), 
                Collections.GetEmptyList<SelectExprStreamDesc>(), null, null, false,
                new SupportStreamTypeSvc1Stream(),
                container.Resolve<EventAdapterService>(), null,
                selectExprEventTypeRegistry, statementContext.EngineImportService, 1, "stmtname", null,
                new Configuration(container), null, new TableServiceImpl(container), null);
            _selectExprProcessor = factory.GetEvaluator();
            _orderByProcessor = null;
    
            var prototype = new ResultSetProcessorSimpleFactory(_selectExprProcessor, null, true, null, false, null, 1);
    		_outputProcessorAll = (ResultSetProcessorSimple) prototype.Instantiate(null, null, null);
        }
    
        [Test]
        public void TestUpdateAll()
        {
            Assert.IsNull(ResultSetProcessorUtil.GetSelectEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, (EventBean[])null, true, false, null));
    
            var testEvent1 = MakeEvent(10, 5, 6);
    	    var testEvent2 = MakeEvent(11, 6, 7);
            var newData = new EventBean[] {testEvent1, testEvent2};
    
            var testEvent3 = MakeEvent(20, 1, 2);
    	    var testEvent4 = MakeEvent(21, 3, 4);
    	    var oldData = new EventBean[] {testEvent3, testEvent4};
    
            var result = _outputProcessorAll.ProcessViewResult(newData, oldData, false);
            var newEvents = result.First;
            var oldEvents = result.Second;
    
            Assert.AreEqual(2, newEvents.Length);
            Assert.AreEqual(10d, newEvents[0].Get("resultOne"));
            Assert.AreEqual(30, newEvents[0].Get("resultTwo"));
    
    	    Assert.AreEqual(11d, newEvents[1].Get("resultOne"));
    	    Assert.AreEqual(42, newEvents[1].Get("resultTwo"));
    
            Assert.AreEqual(2, oldEvents.Length);
            Assert.AreEqual(20d, oldEvents[0].Get("resultOne"));
            Assert.AreEqual(2, oldEvents[0].Get("resultTwo"));
    
    	    Assert.AreEqual(21d, oldEvents[1].Get("resultOne"));
    	    Assert.AreEqual(12, oldEvents[1].Get("resultTwo"));
        }
    
        [Test]
        public void TestProcessAll()
        {
            Assert.IsNull(ResultSetProcessorUtil.GetSelectJoinEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, new HashSet<MultiKey<EventBean>>(), true, false, null));
    
            var testEvent1 = MakeEvent(10, 5, 6);
    	    var testEvent2 = MakeEvent(11, 6, 7);
            var newEventSet = MakeEventSet(testEvent1);
    	    newEventSet.Add(new MultiKey<EventBean>(new EventBean[] { testEvent2}));
    
            var testEvent3 = MakeEvent(20, 1, 2);
    	    var testEvent4 = MakeEvent(21, 3, 4);
            var oldEventSet = MakeEventSet(testEvent3);
    	    oldEventSet.Add(new MultiKey<EventBean>(new EventBean[] {testEvent4}));
    
            var result = _outputProcessorAll.ProcessJoinResult(newEventSet, oldEventSet, false);
            var newEvents = result.First;
            var oldEvents = result.Second;
    
            Assert.AreEqual(2, newEvents.Length);
            Assert.AreEqual(10d, newEvents[0].Get("resultOne"));
            Assert.AreEqual(30, newEvents[0].Get("resultTwo"));
    
    	    Assert.AreEqual(11d, newEvents[1].Get("resultOne"));
    	    Assert.AreEqual(42, newEvents[1].Get("resultTwo"));
    
            Assert.AreEqual(2, oldEvents.Length);
            Assert.AreEqual(20d, oldEvents[0].Get("resultOne"));
            Assert.AreEqual(2, oldEvents[0].Get("resultTwo"));
    
    	    Assert.AreEqual(21d, oldEvents[1].Get("resultOne"));
    	    Assert.AreEqual(12, oldEvents[1].Get("resultTwo"));
        }
    
        private ISet<MultiKey<EventBean>> MakeEventSet(EventBean theEvent)
        {
            ISet<MultiKey<EventBean>> result = new LinkedHashSet<MultiKey<EventBean>>();
            result.Add(new MultiKey<EventBean>(new EventBean[] { theEvent}));
            return result;
        }
    
        private EventBean MakeEvent(double doubleBoxed, int intBoxed, int intPrimitive)
        {
            var bean = new SupportBean();
            bean.DoubleBoxed = doubleBoxed;
            bean.IntBoxed = intBoxed;
            bean.IntPrimitive = intPrimitive;
            return SupportEventBeanFactory.CreateObject(bean);
        }
    }
}
