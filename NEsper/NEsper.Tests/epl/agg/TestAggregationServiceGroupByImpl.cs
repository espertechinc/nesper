///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.agg
{
    [TestFixture]
    public class TestAggregationServiceGroupByImpl 
    {
        private AggSvcGroupByNoAccessImpl _service;
        private MultiKeyUntyped _groupOneKey;
        private MultiKeyUntyped _groupTwoKey;
    
        [SetUp]
        public void SetUp()
        {
            var aggregators = new SupportAggregatorFactory[2];
            for (int i = 0; i < aggregators.Length; i++)
            {
                aggregators[i] = new SupportAggregatorFactory();
            }
            var evaluators = new[] { new SupportExprNode(5).ExprEvaluator, new SupportExprNode(2).ExprEvaluator };
    
            _service = new AggSvcGroupByNoAccessImpl(evaluators, aggregators);
    
            _groupOneKey = new MultiKeyUntyped(new Object[] {"x", "y1"});
            _groupTwoKey = new MultiKeyUntyped(new Object[] {"x", "y2"});
        }
    
        [Test]
        public void TestGetValue()
        {
            ExprEvaluatorContext exprEvaluatorContext = SupportStatementContextFactory.MakeEvaluatorContext();
            // apply 3 rows to group key 1, all aggregators evaluated their sub-expressions(constants 5 and 2)
            _service.ApplyEnter(new EventBean[1], _groupOneKey, exprEvaluatorContext);
            _service.ApplyEnter(new EventBean[1], _groupOneKey, exprEvaluatorContext);
            _service.ApplyEnter(new EventBean[1], _groupTwoKey, exprEvaluatorContext);
    
            _service.SetCurrentAccess(_groupOneKey, -1, null);
            Assert.AreEqual(10, _service.GetValue(0, -1, EvaluateParams.EmptyTrue));
            Assert.AreEqual(4, _service.GetValue(1, -1, EvaluateParams.EmptyTrue));
            _service.SetCurrentAccess(_groupTwoKey, -1, null);
            Assert.AreEqual(5, _service.GetValue(0, -1, EvaluateParams.EmptyTrue));
            Assert.AreEqual(2, _service.GetValue(1, -1, EvaluateParams.EmptyTrue));
    
            _service.ApplyLeave(new EventBean[1], _groupTwoKey, exprEvaluatorContext);
            _service.ApplyLeave(new EventBean[1], _groupTwoKey, exprEvaluatorContext);
            _service.ApplyLeave(new EventBean[1], _groupTwoKey, exprEvaluatorContext);
            _service.ApplyLeave(new EventBean[1], _groupOneKey, exprEvaluatorContext);

            _service.SetCurrentAccess(_groupOneKey, -1, null);
            Assert.AreEqual(10 - 5, _service.GetValue(0, -1, EvaluateParams.EmptyTrue));
            Assert.AreEqual(4 - 2, _service.GetValue(1, -1, EvaluateParams.EmptyTrue));
            _service.SetCurrentAccess(_groupTwoKey, -1, null);
            Assert.AreEqual(5 - 15, _service.GetValue(0, -1, EvaluateParams.EmptyTrue));
            Assert.AreEqual(2 - 6, _service.GetValue(1, -1, EvaluateParams.EmptyTrue));
        }
    }
}
