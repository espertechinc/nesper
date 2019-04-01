///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.agg
{
    [TestFixture]
    public class TestAggregationServiceGroupAllImpl 
    {
        private AggSvcGroupAllNoAccessImpl _service;
    
        [SetUp]
        public void SetUp()
        {
            var aggregators = new SupportAggregator[2];
            for (int i = 0; i < aggregators.Length; i++)
            {
                aggregators[i] = new SupportAggregator();
            }
    
            var evaluators = new[] { new SupportExprNode(5).ExprEvaluator, new SupportExprNode(2).ExprEvaluator };

            _service = new AggSvcGroupAllNoAccessImpl(evaluators, aggregators, new AggregationMethodFactory[] {
                new SupportAggregatorFactory(), new SupportAggregatorFactory()
            });
        }
    
        [Test]
        public void TestApplyEnter()
        {
            // apply two rows, all aggregators evaluated their sub-expressions(constants 5 and 2) twice
            _service.ApplyEnter(new EventBean[1], null, null);
            _service.ApplyEnter(new EventBean[1], null, null);
            Assert.AreEqual(10, _service.GetValue(0, -1, EvaluateParams.EmptyTrue));
            Assert.AreEqual(4, _service.GetValue(1, -1, EvaluateParams.EmptyTrue));
        }
    
        [Test]
        public void TestApplyLeave()
        {
            // apply 3 rows, all aggregators evaluated their sub-expressions(constants 5 and 2)
            _service.ApplyLeave(new EventBean[1], null, null);
            _service.ApplyLeave(new EventBean[1], null, null);
            _service.ApplyLeave(new EventBean[1], null, null);
            Assert.AreEqual(-15, _service.GetValue(0, -1, EvaluateParams.EmptyTrue));
            Assert.AreEqual(-6, _service.GetValue(1, -1, EvaluateParams.EmptyTrue));
        }
    
        private static EventBean[][] MakeEvents(int countRows)
        {
            EventBean[][] result = new EventBean[countRows][];
            for (int i = 0; i < countRows; i++)
            {
                result[i] = new EventBean[0];
            }
            return result;
        }
    }
}
