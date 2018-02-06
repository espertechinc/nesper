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
using com.espertech.esper.collection;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.filter;

using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestFilterParamIndexIn 
    {
        private SupportEventEvaluator _testEvaluator;
        private SupportBean _testBean;
        private EventBean _testEventBean;
        private EventType _testEventType;
        private List<FilterHandle> _matchesList;
    
        [SetUp]
        public void SetUp()
        {
            _testEvaluator = new SupportEventEvaluator();
            _testBean = new SupportBean();
            _testEventBean = SupportEventBeanFactory.CreateObject(_testBean);
            _testEventType = _testEventBean.EventType;
            _matchesList = new List<FilterHandle>();
        }
    
        [Test]
        public void TestIndex()
        {
            FilterParamIndexIn index = new FilterParamIndexIn(MakeLookupable("LongBoxed"), ReaderWriterLockManager.CreateDefaultLock());
            Assert.AreEqual(FilterOperator.IN_LIST_OF_VALUES, index.FilterOperator);
    
            MultiKeyUntyped inList = new MultiKeyUntyped(new Object[] {2L, 5L});
            index.Put(inList, _testEvaluator);
            inList = new MultiKeyUntyped(new Object[] {10L, 5L});
            index.Put(inList, _testEvaluator);
    
            Verify(index, 1L, 0);
            Verify(index, 2L, 1);
            Verify(index, 5L, 2);
            Verify(index, 10L, 1);
            Verify(index, 999L, 0);
            Verify(index, null, 0);
    
            Assert.AreEqual(_testEvaluator, index.Get(inList));
            Assert.IsTrue(index.ReadWriteLock != null);
            index.Remove(inList);
            index.Remove(inList);
            Assert.AreEqual(null, index.Get(inList));
    
            try
            {
                index["a"] = _testEvaluator;
                Assert.IsTrue(false);
            }
            catch (Exception ex)
            {
                // Expected
            }
        }
    
        private void Verify(FilterParamIndexBase index, long? testValue, int numExpected)
        {
            _testBean.LongBoxed = testValue;
            index.MatchEvent(_testEventBean, _matchesList);
            Assert.AreEqual(numExpected, _testEvaluator.GetAndResetCountInvoked());
        }
    
        private FilterSpecLookupable MakeLookupable(String fieldName) {
            return new FilterSpecLookupable(fieldName, _testEventType.GetGetter(fieldName), _testEventType.GetPropertyType(fieldName), false);
        }
    }
}
