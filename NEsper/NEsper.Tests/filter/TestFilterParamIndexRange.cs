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
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.filter;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestFilterParamIndexRange 
    {
        private SupportEventEvaluator _testEvaluator;
        private SupportBean _testBean;
        private EventBean _testEventBean;
        private EventType _testEventType;
        private List<FilterHandle> _matchesList;
        private DoubleRange _testRange;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _testEvaluator = new SupportEventEvaluator();
            _testBean = new SupportBean();
            _testEventBean = SupportEventBeanFactory.CreateObject(_testBean);
            _testEventType = _testEventBean.EventType;
            _matchesList = new List<FilterHandle>();
    
            _testRange = new DoubleRange(10d, 20d);
        }
    
        [Test]
        public void TestLongBothEndpointsIncluded()
        {
            FilterParamIndexDoubleRange index = GetLongDataset(FilterOperator.RANGE_CLOSED);
            VerifyLongPrimitive(index, -1, 0);
            VerifyLongPrimitive(index, 0, 2);
            VerifyLongPrimitive(index, 1, 5);
            VerifyLongPrimitive(index, 2, 5);
            VerifyLongPrimitive(index, 3, 7);
            VerifyLongPrimitive(index, 4, 6);
            VerifyLongPrimitive(index, 5, 6);
            VerifyLongPrimitive(index, 6, 6);
            VerifyLongPrimitive(index, 7, 6);
            VerifyLongPrimitive(index, 8, 6);
            VerifyLongPrimitive(index, 9, 5);
            VerifyLongPrimitive(index, 10, 3);
            VerifyLongPrimitive(index, 11, 1);
    
            index.Put(_testRange, _testEvaluator);
            Assert.AreEqual(_testEvaluator, index.Get(_testRange));
            Assert.IsTrue(index.ReadWriteLock != null);
            index.Remove(_testRange);
            index.Remove(_testRange);
            Assert.AreEqual(null, index.Get(_testRange));
    
            try
            {
                index["a"] = _testEvaluator;
                Assert.IsTrue(false);
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }
    
        [Test]
        public void TestLongLowEndpointIncluded()
        {
            FilterParamIndexDoubleRange index = GetLongDataset(FilterOperator.RANGE_HALF_OPEN);
            VerifyLongPrimitive(index, -1, 0);
            VerifyLongPrimitive(index, 0, 2);
            VerifyLongPrimitive(index, 1, 5);
            VerifyLongPrimitive(index, 2, 5);
            VerifyLongPrimitive(index, 3, 6);
            VerifyLongPrimitive(index, 4, 6);
            VerifyLongPrimitive(index, 5, 3);
            VerifyLongPrimitive(index, 6, 5);
            VerifyLongPrimitive(index, 7, 4);
            VerifyLongPrimitive(index, 8, 5);
            VerifyLongPrimitive(index, 9, 3);
            VerifyLongPrimitive(index, 10, 1);
            VerifyLongPrimitive(index, 11, 1);
        }
    
        [Test]
        public void TestLongHighEndpointIncluded()
        {
            FilterParamIndexDoubleRange index = GetLongDataset(FilterOperator.RANGE_HALF_CLOSED);
            VerifyLongPrimitive(index, -1, 0);
            VerifyLongPrimitive(index, 0, 0);
            VerifyLongPrimitive(index, 1, 2);
            VerifyLongPrimitive(index, 2, 5);
            VerifyLongPrimitive(index, 3, 5);
            VerifyLongPrimitive(index, 4, 6);
            VerifyLongPrimitive(index, 5, 6);
            VerifyLongPrimitive(index, 6, 3);
            VerifyLongPrimitive(index, 7, 5);
            VerifyLongPrimitive(index, 8, 4);
            VerifyLongPrimitive(index, 9, 5);
            VerifyLongPrimitive(index, 10, 3);
            VerifyLongPrimitive(index, 11, 1);
        }
    
        [Test]
        public void TestLongNeitherEndpointIncluded()
        {
            FilterParamIndexDoubleRange index = GetLongDataset(FilterOperator.RANGE_OPEN);
            VerifyLongPrimitive(index, -1, 0);
            VerifyLongPrimitive(index, 0, 0);
            VerifyLongPrimitive(index, 1, 2);
            VerifyLongPrimitive(index, 2, 5);
            VerifyLongPrimitive(index, 3, 4);
            VerifyLongPrimitive(index, 4, 6);
            VerifyLongPrimitive(index, 5, 3);
            VerifyLongPrimitive(index, 6, 2);
            VerifyLongPrimitive(index, 7, 3);
            VerifyLongPrimitive(index, 8, 3);
            VerifyLongPrimitive(index, 9, 3);
            VerifyLongPrimitive(index, 10, 1);
            VerifyLongPrimitive(index, 11, 1);
        }
    
        [Test]
        public void TestDoubleBothEndpointsIncluded()
        {
            FilterParamIndexDoubleRange index = GetDoubleDataset(FilterOperator.RANGE_CLOSED);
            VerifyDoublePrimitive(index, 1.49, 0);
            VerifyDoublePrimitive(index, 1.5, 1);
            VerifyDoublePrimitive(index, 2.5, 1);
            VerifyDoublePrimitive(index, 2.51, 0);
            VerifyDoublePrimitive(index, 3.5, 2);
            VerifyDoublePrimitive(index, 4.4, 2);
            VerifyDoublePrimitive(index, 4.5, 2);
            VerifyDoublePrimitive(index, 4.5001, 1);
            VerifyDoublePrimitive(index, 5.1, 1);
            VerifyDoublePrimitive(index, 5.8, 2);
            VerifyDoublePrimitive(index, 6.7, 2);
            VerifyDoublePrimitive(index, 6.8, 1);
            VerifyDoublePrimitive(index, 9.5, 1);
            VerifyDoublePrimitive(index, 10.1, 0);
        }
    
        [Test]
        public void TestDoubleFixedRangeSize()
        {
            FilterParamIndexDoubleRange index = MakeOne("DoublePrimitive", FilterOperator.RANGE_OPEN, _testEventType);
    
            for (int i = 0; i < 10000; i++)
            {
                var range = new DoubleRange(i, i + 1);
                index.Put(range, _testEvaluator);
            }
    
            VerifyDoublePrimitive(index, 5000, 0);
            VerifyDoublePrimitive(index, 5000.5, 1);
            VerifyDoublePrimitive(index, 5001, 0);
        }
    
        [Test]
        public void TestDoubleVariableRangeSize()
        {
            FilterParamIndexDoubleRange index = MakeOne("DoublePrimitive", FilterOperator.RANGE_CLOSED, _testEventType);
    
            for (int i = 0; i < 100; i++)
            {
                var range = new DoubleRange(i, 2*i);
                index.Put(range, _testEvaluator);
            }
    
            // 1 to 2
            // 2 to 4
            // 3 to 6
            // and so on
    
            VerifyDoublePrimitive(index, 1, 1);
            VerifyDoublePrimitive(index, 2, 2);
            VerifyDoublePrimitive(index, 2.001, 1);
            VerifyDoublePrimitive(index, 3, 2);
            VerifyDoublePrimitive(index, 4, 3);
            VerifyDoublePrimitive(index, 4.5, 2);
            VerifyDoublePrimitive(index, 50, 26);
        }
    
        private FilterParamIndexDoubleRange GetLongDataset(FilterOperator operatorType)
        {
            FilterParamIndexDoubleRange index = MakeOne("LongPrimitive", operatorType, _testEventType);
    
            AddToIndex(index,0,5);
            AddToIndex(index,0,6);
            AddToIndex(index,1,3);
            AddToIndex(index,1,5);
            AddToIndex(index,1,7);
            AddToIndex(index,3,5);
            AddToIndex(index,3,7);
            AddToIndex(index,6,9);
            AddToIndex(index,6,10);
            AddToIndex(index,6,int.MaxValue - 1);
            AddToIndex(index,7,8);
            AddToIndex(index,8,9);
            AddToIndex(index,8,10);
    
            return index;
        }
    
        private FilterParamIndexDoubleRange GetDoubleDataset(FilterOperator operatorType)
        {
            FilterParamIndexDoubleRange index = MakeOne("DoublePrimitive", operatorType, _testEventType);
    
            AddToIndex(index, 1.5, 2.5);
            AddToIndex(index, 3.5, 4.5 );
            AddToIndex(index, 3.5, 9.5);
            AddToIndex(index, 5.6, 6.7);
    
            return index;
        }
    
        private FilterParamIndexDoubleRange MakeOne(String fieldName, FilterOperator operatorType, EventType testEventType) {
            return new FilterParamIndexDoubleRange(MakeLookupable(fieldName), _container.RWLockManager().CreateDefaultLock(), operatorType);
        }
    
        private void VerifyDoublePrimitive(FilterParamIndexBase index, double testValue, int numExpected)
        {
            _testBean.DoublePrimitive = testValue;
            index.MatchEvent(_testEventBean, _matchesList);
            Assert.AreEqual(numExpected, _testEvaluator.GetAndResetCountInvoked());
        }
    
        private void VerifyLongPrimitive(FilterParamIndexBase index, long testValue, int numExpected)
        {
            _testBean.LongPrimitive = testValue;
            index.MatchEvent(_testEventBean, _matchesList);
            Assert.AreEqual(numExpected, _testEvaluator.GetAndResetCountInvoked());
        }
    
        private void AddToIndex(FilterParamIndexDoubleRange index, double min, double max)
        {
            var r = new DoubleRange(min,max);
            index.Put(r, _testEvaluator);
        }
    
        private FilterSpecLookupable MakeLookupable(String fieldName) {
            return new FilterSpecLookupable(fieldName, _testEventType.GetGetter(fieldName), _testEventType.GetPropertyType(fieldName), false);
        }
    }
}
