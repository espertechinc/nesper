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
    public class TestFilterParamIndexCompare 
    {
        private SupportEventEvaluator _testEvaluator;
        private SupportBean _testBean;
        private EventBean _testEventBean;
        private EventType _testEventType;
        private List<FilterHandle> _matchesList;
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
        }
    
        [Test]
        public void TestMatchDoubleAndGreater()
        {
            FilterParamIndexCompare index = MakeOne("DoublePrimitive", FilterOperator.GREATER);
    
            index.Put(1.5, _testEvaluator);
            index.Put(2.1, _testEvaluator);
            index.Put(2.2, _testEvaluator);
    
            VerifyDoublePrimitive(index, 1.5, 0);
            VerifyDoublePrimitive(index, 1.7, 1);
            VerifyDoublePrimitive(index, 2.2, 2);
            VerifyDoublePrimitive(index, 2.1999999, 2);
            VerifyDoublePrimitive(index, -1, 0);
            VerifyDoublePrimitive(index, 99, 3);
    
            Assert.AreEqual(_testEvaluator, index.Get(1.5d));
            Assert.IsTrue(index.ReadWriteLock != null);
            index.Remove(1.5d);
            index.Remove(1.5d);
            Assert.AreEqual(null, index.Get(1.5d));
    
            try
            {
                index["a"] = _testEvaluator;
                Assert.IsTrue(false);
            }
            catch (InvalidOperationException ex)
            {
                Assert.That(ex.InnerException, Is.InstanceOf<ArgumentException>());
            }
        }
    
        [Test]
        public void TestMatchLongAndGreaterEquals()
        {
            FilterParamIndexCompare index = MakeOne("LongBoxed", FilterOperator.GREATER_OR_EQUAL);
    
            index.Put(1L, _testEvaluator);
            index.Put(2L, _testEvaluator);
            index.Put(4L, _testEvaluator);
    
            // Should not match with null
            VerifyLongBoxed(index, null, 0);
    
            VerifyLongBoxed(index, 0L, 0);
            VerifyLongBoxed(index, 1L, 1);
            VerifyLongBoxed(index, 2L, 2);
            VerifyLongBoxed(index, 3L, 2);
            VerifyLongBoxed(index, 4L, 3);
            VerifyLongBoxed(index, 10L, 3);
    
            // Put a long primitive in - should work
            index.Put(9L, _testEvaluator);
            try
            {
                index.Put(10, _testEvaluator);
                Assert.IsTrue(false);
            }
            catch (InvalidOperationException ex)
            {
                Assert.That(ex.InnerException, Is.InstanceOf<ArgumentException>());
            }
        }
    
        [Test]
        public void TestMatchLongAndLessThan()
        {
            FilterParamIndexCompare index = MakeOne("LongPrimitive", FilterOperator.LESS);
    
            index.Put(1L, _testEvaluator);
            index.Put(10L, _testEvaluator);
            index.Put(100L, _testEvaluator);
    
            VerifyLongPrimitive(index, 100, 0);
            VerifyLongPrimitive(index, 101, 0);
            VerifyLongPrimitive(index, 99, 1);
            VerifyLongPrimitive(index, 11, 1);
            VerifyLongPrimitive(index, 10, 1);
            VerifyLongPrimitive(index, 9, 2);
            VerifyLongPrimitive(index, 2, 2);
            VerifyLongPrimitive(index, 1, 2);
            VerifyLongPrimitive(index, 0, 3);
        }
    
        [Test]
        public void TestMatchDoubleAndLessOrEqualThan()
        {
            FilterParamIndexCompare index = MakeOne("DoubleBoxed", FilterOperator.LESS_OR_EQUAL);
    
            index.Put(7.4D, _testEvaluator);
            index.Put(7.5D, _testEvaluator);
            index.Put(7.6D, _testEvaluator);
    
            VerifyDoubleBoxed(index, 7.39, 3);
            VerifyDoubleBoxed(index, 7.4, 3);
            VerifyDoubleBoxed(index, 7.41, 2);
            VerifyDoubleBoxed(index, 7.5, 2);
            VerifyDoubleBoxed(index, 7.51, 1);
            VerifyDoubleBoxed(index, 7.6, 1);
            VerifyDoubleBoxed(index, 7.61, 0);
        }
    
        private FilterParamIndexCompare MakeOne(String field, FilterOperator op) {
            return new FilterParamIndexCompare(MakeLookupable(field), _container.RWLockManager().CreateDefaultLock(), op);
        }
    
        private void VerifyDoublePrimitive(FilterParamIndexBase index, double testValue, int numExpected)
        {
            _testBean.DoublePrimitive = testValue;
            index.MatchEvent(_testEventBean, _matchesList);
            Assert.AreEqual(numExpected, _testEvaluator.GetAndResetCountInvoked());
        }
    
        private void VerifyDoubleBoxed(FilterParamIndexBase index, double? testValue, int numExpected)
        {
            _testBean.DoubleBoxed = testValue;
            index.MatchEvent(_testEventBean, _matchesList);
            Assert.AreEqual(numExpected, _testEvaluator.GetAndResetCountInvoked());
        }
    
        private void VerifyLongBoxed(FilterParamIndexBase index, long? testValue, int numExpected)
        {
            _testBean.LongBoxed = testValue;
            index.MatchEvent(_testEventBean, _matchesList);
            Assert.AreEqual(numExpected, _testEvaluator.GetAndResetCountInvoked());
        }
    
        private void VerifyLongPrimitive(FilterParamIndexBase index, long testValue, int numExpected)
        {
            _testBean.LongPrimitive = testValue;
            index.MatchEvent(_testEventBean, _matchesList);
            Assert.AreEqual(numExpected, _testEvaluator.GetAndResetCountInvoked());
        }
    
        private FilterSpecLookupable MakeLookupable(String fieldName) {
            return new FilterSpecLookupable(fieldName, _testEventType.GetGetter(fieldName), _testEventType.GetPropertyType(fieldName), false);
        }
    }
}
