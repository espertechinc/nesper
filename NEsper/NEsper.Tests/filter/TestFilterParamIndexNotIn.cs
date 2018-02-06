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
    public class TestFilterParamIndexNotIn 
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
        public void TestIndex()
        {
            FilterParamIndexNotIn index = new FilterParamIndexNotIn(MakeLookupable("LongBoxed"), _container.RWLockManager().CreateDefaultLock());
            Assert.AreEqual(FilterOperator.NOT_IN_LIST_OF_VALUES, index.FilterOperator);
    
            index.Put(new MultiKeyUntyped(new Object[] {2L, 5L}), _testEvaluators[0]);
            index.Put(new MultiKeyUntyped(new Object[] {3L, 4L, 5L}), _testEvaluators[1]);
            index.Put(new MultiKeyUntyped(new Object[] {1L, 4L, 5L}), _testEvaluators[2]);
            index.Put(new MultiKeyUntyped(new Object[] {2L, 5L}), _testEvaluators[3]);
    
            Verify(index, 0L, new bool[] {true, true, true, true});
            Verify(index, 1L, new bool[] {true, true, false, true});
            Verify(index, 2L, new bool[] {false, true, true, false});
            Verify(index, 3L, new bool[] {true, false, true, true});
            Verify(index, 4L, new bool[] {true, false, false, true});
            Verify(index, 5L, new bool[] {false, false, false, false});
            Verify(index, 6L, new bool[] {true, true, true, true});
    
            MultiKeyUntyped inList = new MultiKeyUntyped(new Object[] {3L, 4L, 5L});
            Assert.AreEqual(_testEvaluators[1], index.Get(inList));
            Assert.IsTrue(index.ReadWriteLock != null);
            index.Remove(inList);
            index.Remove(inList);
            Assert.AreEqual(null, index.Get(inList));
    
            // now that {3,4,5} is removed, verify results again
            Verify(index, 0L, new bool[] {true, false, true, true});
            Verify(index, 1L, new bool[] {true, false, false, true});
            Verify(index, 2L, new bool[] {false, false, true, false});
            Verify(index, 3L, new bool[] {true, false, true, true});
            Verify(index, 4L, new bool[] {true, false, false, true});
            Verify(index, 5L, new bool[] {false, false, false, false});
            Verify(index, 6L, new bool[] {true, false, true, true});
            
            try
            {
                index["a"] = _testEvaluators[0];
                Assert.IsTrue(false);
            }
            catch (Exception)
            {
                // Expected
            }
        }
    
        private void Verify(FilterParamIndexBase index, long? testValue, bool[] expected)
        {
            _testBean.LongBoxed = testValue;
            index.MatchEvent(_testEventBean, _matchesList);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], _testEvaluators[i].GetAndResetCountInvoked() == 1);
            }
        }
    
        private FilterSpecLookupable MakeLookupable(String fieldName) {
            return new FilterSpecLookupable(fieldName, _testEventType.GetGetter(fieldName), _testEventType.GetPropertyType(fieldName), false);
        }
    }
}
