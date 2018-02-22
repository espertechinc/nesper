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
    public class TestFilterParamIndexNotEquals 
    {
        private SupportEventEvaluator _testEvaluator;
        private SupportBean _testBean;
        private EventBean _testEventBean;
        private EventType _testEventType;
        private List<FilterHandle> _matchesList;
        private FilterServiceGranularLockFactory _lockFactory;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _lockFactory = new FilterServiceGranularLockFactoryReentrant(_container.RWLockManager());
            _testEvaluator = new SupportEventEvaluator();
            _testBean = new SupportBean();
            _testEventBean = SupportEventBeanFactory.CreateObject(_testBean);
            _testEventType = _testEventBean.EventType;
            _matchesList = new List<FilterHandle>();
        }
    
        [Test]
        public void TestBoolean()
        {
            FilterParamIndexNotEquals index = new FilterParamIndexNotEquals(
                MakeLookupable("BoolPrimitive"), _lockFactory.ObtainNew());
            Assert.AreEqual(FilterOperator.NOT_EQUAL, index.FilterOperator);
            Assert.AreEqual("BoolPrimitive", index.Lookupable.Expression);
    
            index.Put(false, _testEvaluator);
    
            VerifyBooleanPrimitive(index, true, 1);
            VerifyBooleanPrimitive(index, false, 0);
        }
    
        [Test]
        public void TestString()
        {
            FilterParamIndexNotEquals index = new FilterParamIndexNotEquals(
                MakeLookupable("TheString"), _lockFactory.ObtainNew());
    
            index["hello"] = _testEvaluator;
            index["test"] = _testEvaluator;
    
            VerifyString(index, null, 0);
            VerifyString(index, "dudu", 2);
            VerifyString(index, "hello", 1);
            VerifyString(index, "test", 1);
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
    
        private FilterSpecLookupable MakeLookupable(String fieldName) {
            return new FilterSpecLookupable(fieldName, _testEventType.GetGetter(fieldName), _testEventType.GetPropertyType(fieldName), false);
        }
    }
}
