///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestFilterParamIndexNotEquals : AbstractTestBase
    {
        private SupportEventEvaluator testEvaluator;
        private SupportBean testBean;
        private EventBean testEventBean;
        private EventType testEventType;
        private IList<FilterHandle> matchesList;
        private FilterServiceGranularLockFactory lockFactory;

        [SetUp]
        public void SetUp()
        {
            lockFactory = new FilterServiceGranularLockFactoryReentrant(container.RWLockManager());

            testEvaluator = new SupportEventEvaluator();
            testBean = new SupportBean();
            testEventBean = SupportEventBeanFactory
                .GetInstance(container)
                .CreateObject(testBean);
            testEventType = testEventBean.EventType;
            matchesList = new List<FilterHandle>();
        }

        [Test]
        public void TestBoolean()
        {
            FilterParamIndexNotEquals index = new FilterParamIndexNotEquals(MakeLookupable("BoolPrimitive"), lockFactory.ObtainNew());
            Assert.AreEqual(FilterOperator.NOT_EQUAL, index.FilterOperator);
            Assert.AreEqual("BoolPrimitive", index.Lookupable.Expression);

            index.Put(false, testEvaluator);

            VerifyBooleanPrimitive(index, true, 1);
            VerifyBooleanPrimitive(index, false, 0);
        }

        [Test]
        public void TestString()
        {
            FilterParamIndexNotEquals index = new FilterParamIndexNotEquals(MakeLookupable("TheString"), lockFactory.ObtainNew());

            index.Put("hello", testEvaluator);
            index.Put("test", testEvaluator);

            VerifyString(index, null, 0);
            VerifyString(index, "dudu", 2);
            VerifyString(index, "hello", 1);
            VerifyString(index, "test", 1);
        }

        private void VerifyBooleanPrimitive(FilterParamIndexBase index, bool testValue, int numExpected)
        {
            testBean.BoolPrimitive = testValue;
            index.MatchEvent(testEventBean, matchesList);
            Assert.AreEqual(numExpected, testEvaluator.GetAndResetCountInvoked());
        }

        private void VerifyString(FilterParamIndexBase index, string testValue, int numExpected)
        {
            testBean.TheString = testValue;
            index.MatchEvent(testEventBean, matchesList);
            Assert.AreEqual(numExpected, testEvaluator.GetAndResetCountInvoked());
        }

        private ExprFilterSpecLookupable MakeLookupable(string fieldName)
        {
            return new ExprFilterSpecLookupable(fieldName, testEventType.GetGetter(fieldName), testEventType.GetPropertyType(fieldName), false);
        }
    }
} // end of namespace
