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
    public class TestFilterParamIndexEquals 
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
        public void TestLong()
        {
            FilterParamIndexEquals index = MakeOne("ShortBoxed", _testEventType);
    
            index.Put(((short) 1), _testEvaluator);
            index.Put(((short) 20), _testEvaluator);
    
            VerifyShortBoxed(index, (short) 10, 0);
            VerifyShortBoxed(index, (short) 1, 1);
            VerifyShortBoxed(index, (short) 20, 1);
            VerifyShortBoxed(index, null, 0);
    
            Assert.AreEqual(_testEvaluator, index.Get((short) 1));
            Assert.IsTrue(index.ReadWriteLock != null);
            index.Remove((short) 1);
            index.Remove((short) 1);
            Assert.AreEqual(null, index.Get((short) 1));
        }
    
        [Test]
        public void TestBoolean()
        {
            FilterParamIndexEquals index = MakeOne("BoolPrimitive", _testEventType);
    
            index.Put(false, _testEvaluator);
    
            VerifyBooleanPrimitive(index, false, 1);
            VerifyBooleanPrimitive(index, true, 0);
        }
    
        [Test]
        public void TestString()
        {
            FilterParamIndexEquals index = MakeOne("TheString", _testEventType);
    
            index["hello"] = _testEvaluator;
            index["test"] = _testEvaluator;
    
            VerifyString(index, null, 0);
            VerifyString(index, "dudu", 0);
            VerifyString(index, "hello", 1);
            VerifyString(index, "test", 1);
        }
    
        [Test]
        public void TestFloatPrimitive()
        {
            FilterParamIndexEquals index = MakeOne("FloatPrimitive", _testEventType);
    
            index.Put(1.5f, _testEvaluator);
    
            VerifyFloatPrimitive(index, 1.5f, 1);
            VerifyFloatPrimitive(index, 2.2f, 0);
            VerifyFloatPrimitive(index, 0, 0);
        }
    
        private void VerifyShortBoxed(FilterParamIndexBase index, short? testValue, int numExpected)
        {
            _testBean.ShortBoxed = testValue;
            index.MatchEvent(_testEventBean, _matchesList);
            Assert.AreEqual(numExpected, _testEvaluator.GetAndResetCountInvoked());
        }
    
        private void VerifyBooleanPrimitive(FilterParamIndexBase index, bool testValue, int numExpected)
        {
            _testBean.BoolPrimitive = testValue;
            index.MatchEvent(_testEventBean, _matchesList);
            Assert.AreEqual(numExpected, _testEvaluator.GetAndResetCountInvoked());
        }
    
        private void VerifyString(FilterParamIndexBase index, String testValue, int numExpected)
        {
            _testBean.TheString = testValue;
            index.MatchEvent(_testEventBean, _matchesList);
            Assert.AreEqual(numExpected, _testEvaluator.GetAndResetCountInvoked());
        }
    
        private void VerifyFloatPrimitive(FilterParamIndexBase index, float testValue, int numExpected)
        {
            _testBean.FloatPrimitive = testValue;
            index.MatchEvent(_testEventBean, _matchesList);
            Assert.AreEqual(numExpected, _testEvaluator.GetAndResetCountInvoked());
        }
    
        private FilterParamIndexEquals MakeOne(String property, EventType testEventType) {
            return new FilterParamIndexEquals(MakeLookupable(property), _container.RWLockManager().CreateDefaultLock());
        }
    
        private FilterSpecLookupable MakeLookupable(String fieldName) {
            return new FilterSpecLookupable(fieldName, _testEventType.GetGetter(fieldName), _testEventType.GetPropertyType(fieldName), false);
        }
    }
}
