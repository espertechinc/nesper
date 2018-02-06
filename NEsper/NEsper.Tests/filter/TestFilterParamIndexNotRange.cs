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
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.filter;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestFilterParamIndexNotRange 
    {
        private SupportEventEvaluator[] _testEvaluators;
        private SupportBean _testBean;
        private EventBean _testEventBean;
        private EventType _testEventType;
        private List<FilterHandle> _matchesList;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _testEvaluators = new SupportEventEvaluator[4];
            for (int i = 0; i < _testEvaluators.Length; i++)
            {
                _testEvaluators[i] = new SupportEventEvaluator();
            }
    
            _testBean = new SupportBean();
            _testEventBean = SupportEventBeanFactory.CreateObject(_testBean);
            _testEventType = _testEventBean.EventType;
            _matchesList = new List<FilterHandle>();
        }
    
        [Test]
        public void TestClosedRange()
        {
            FilterParamIndexDoubleRangeInverted index = MakeOne("LongBoxed", FilterOperator.NOT_RANGE_CLOSED, _testEventType);
            Assert.AreEqual(FilterOperator.NOT_RANGE_CLOSED, index.FilterOperator);
    
            index.Put(new DoubleRange(2d, 4d), _testEvaluators[0]);
            index.Put(new DoubleRange(2d, 5d), _testEvaluators[1]);
            index.Put(new DoubleRange(1d, 3d), _testEvaluators[2]);
            index.Put(new DoubleRange(1d, 1d), _testEvaluators[3]);
    
            Verify(index, 0L, new bool[] {true, true, true, true});
            Verify(index, 1L, new bool[] {true, true, false, false});
            Verify(index, 2L, new bool[] {false, false, false, true});
            Verify(index, 3L, new bool[] {false, false, false, true});
            Verify(index, 4L, new bool[] {false, false, true, true});
            Verify(index, 5L, new bool[] {true, false, true, true});
            Verify(index, 6L, new bool[] {true, true, true, true});
        }
    
        [Test]
        public void TestOpenRange()
        {
            FilterParamIndexDoubleRangeInverted index = MakeOne("LongBoxed", FilterOperator.NOT_RANGE_OPEN, _testEventType);
    
            index.Put(new DoubleRange(2d, 4d), _testEvaluators[0]);
            index.Put(new DoubleRange(2d, 5d), _testEvaluators[1]);
            index.Put(new DoubleRange(1d, 3d), _testEvaluators[2]);
            index.Put(new DoubleRange(1d, 1d), _testEvaluators[3]);
    
            Verify(index, 0L, new bool[] {true, true, true, true});
            Verify(index, 1L, new bool[] {true, true, true, true});
            Verify(index, 2L, new bool[] {true, true, false, true});
            Verify(index, 3L, new bool[] {false, false, true, true});
            Verify(index, 4L, new bool[] {true, false, true, true});
            Verify(index, 5L, new bool[] {true, true, true, true});
            Verify(index, 6L, new bool[] {true, true, true, true});
        }
    
        [Test]
        public void TestHalfOpenRange()
        {
            FilterParamIndexDoubleRangeInverted index = MakeOne("LongBoxed", FilterOperator.NOT_RANGE_HALF_OPEN, _testEventType);
    
            index.Put(new DoubleRange(2d, 4d), _testEvaluators[0]);
            index.Put(new DoubleRange(2d, 5d), _testEvaluators[1]);
            index.Put(new DoubleRange(1d, 3d), _testEvaluators[2]);
            index.Put(new DoubleRange(1d, 1d), _testEvaluators[3]);
    
            Verify(index, 0L, new bool[] {true, true, true, true});
            Verify(index, 1L, new bool[] {true, true, false, true});
            Verify(index, 2L, new bool[] {false, false, false, true});
            Verify(index, 3L, new bool[] {false, false, true, true});
            Verify(index, 4L, new bool[] {true, false, true, true});
            Verify(index, 5L, new bool[] {true, true, true, true});
            Verify(index, 6L, new bool[] {true, true, true, true});
        }
    
        [Test]
        public void TestHalfClosedRange()
        {
            FilterParamIndexDoubleRangeInverted index = MakeOne("LongBoxed", FilterOperator.NOT_RANGE_HALF_CLOSED, _testEventType);
    
            index.Put(new DoubleRange(2d, 4d), _testEvaluators[0]);
            index.Put(new DoubleRange(2d, 5d), _testEvaluators[1]);
            index.Put(new DoubleRange(1d, 3d), _testEvaluators[2]);
            index.Put(new DoubleRange(1d, 1d), _testEvaluators[3]);
    
            Verify(index, 0L, new bool[] {true, true, true, true});
            Verify(index, 1L, new bool[] {true, true, true, true});
            Verify(index, 2L, new bool[] {true, true, false, true});
            Verify(index, 3L, new bool[] {false, false, false, true});
            Verify(index, 4L, new bool[] {false, false, true, true});
            Verify(index, 5L, new bool[] {true, false, true, true});
            Verify(index, 6L, new bool[] {true, true, true, true});
        }
    
        private FilterParamIndexDoubleRangeInverted MakeOne(
            String field,
            FilterOperator notRangeHalfClosed, 
            EventType testEventType)
        {
            return new FilterParamIndexDoubleRangeInverted(
                MakeLookupable(field),
                _container.RWLockManager().CreateDefaultLock(),
                notRangeHalfClosed);
        }
    
        private void Verify(FilterParamIndexBase index, long? testValue, bool[] expected)
        {
            _testBean.LongBoxed = testValue;
            index.MatchEvent(_testEventBean, _matchesList);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], _testEvaluators[i].GetAndResetCountInvoked() == 1,
                                "Unexpected result for eval " + i);
            }
        }
    
        private FilterSpecLookupable MakeLookupable(String fieldName) {
            return new FilterSpecLookupable(fieldName, _testEventType.GetGetter(fieldName), _testEventType.GetPropertyType(fieldName), false);
        }
    }
}
