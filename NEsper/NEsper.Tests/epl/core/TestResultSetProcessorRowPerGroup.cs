///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.core.eval;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.events;
using com.espertech.esper.support.view;

using NUnit.Framework;

using SupportEventAdapterService = com.espertech.esper.support.events.SupportEventAdapterService;

namespace com.espertech.esper.epl.core
{
    [TestFixture]
    public class TestResultSetProcessorRowPerGroup 
    {
        private ResultSetProcessorRowPerGroup _processor;
        private SupportAggregationService _supportAggregationService;
        private AgentInstanceContext _agentInstanceContext;
    
        [SetUp]
        public void SetUp()
        {
            _agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext();
    
            var selectExprEventTypeRegistry = new SelectExprEventTypeRegistry("abc", new StatementEventTypeRefImpl());
            var factory = new SelectExprProcessorHelper(
                Collections.GetEmptyList<int>(), SupportSelectExprFactory.MakeSelectListFromIdent("TheString", "s0"),
                Collections.GetEmptyList<SelectExprStreamDesc>(), null, null, false, new SupportStreamTypeSvc1Stream(),
                SupportEventAdapterService.Service, null, selectExprEventTypeRegistry,
                _agentInstanceContext.StatementContext.MethodResolutionService,
                1, null, new Configuration(), null, new TableServiceImpl(), null);
            var selectProcessor = factory.Evaluator;
            _supportAggregationService = new SupportAggregationService();
    
            var groupKeyNodes = new ExprEvaluator[2];
            groupKeyNodes[0] = SupportExprNodeFactory.MakeIdentNode("IntPrimitive", "s0").ExprEvaluator;
            groupKeyNodes[1] = SupportExprNodeFactory.MakeIdentNode("IntBoxed", "s0").ExprEvaluator;

            var prototype = new ResultSetProcessorRowPerGroupFactory(selectProcessor, null, groupKeyNodes, null, true, false, null, false, false, false, false, null, false, 1, null);
            _processor = (ResultSetProcessorRowPerGroup) prototype.Instantiate(null, _supportAggregationService, _agentInstanceContext);
        }
    
        [Test]
        public void TestProcess()
        {
            var newData = new EventBean[] {MakeEvent(1, 2), MakeEvent(3, 4)};
            var oldData = new EventBean[] {MakeEvent(1, 2), MakeEvent(1, 10)};
    
            var result = _processor.ProcessViewResult(newData, oldData, false);
    
            Assert.AreEqual(2, _supportAggregationService.EnterList.Count);
            Assert.AreEqual(2, _supportAggregationService.LeaveList.Count);
    
            Assert.AreEqual(3, result.First.Length);
            Assert.AreEqual(3, result.Second.Length);
        }
    
        private EventBean MakeEvent(int intPrimitive, int intBoxed)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            return SupportEventBeanFactory.CreateObject(bean);
        }
    }
}
