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
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestFilterParamIndexEquals : AbstractRuntimeTest
    {
        [SetUp]
        public void SetUp()
        {
            testEvaluator = new SupportEventEvaluator();
            testBean = new SupportBean();
            testEventBean = SupportEventBeanFactory
                .GetInstance(container)
                .CreateObject(testBean);
            testEventType = testEventBean.EventType;
            matchesList = new List<FilterHandle>();
        }

        private SupportEventEvaluator testEvaluator;
        private SupportBean testBean;
        private EventBean testEventBean;
        private EventType testEventType;
        private IList<FilterHandle> matchesList;

        private void VerifyShortBoxed(
            FilterParamIndexBase index,
            short? testValue,
            int numExpected)
        {
            testBean.ShortBoxed = testValue;
            index.MatchEvent(testEventBean, matchesList);
            Assert.AreEqual(numExpected, testEvaluator.GetAndResetCountInvoked());
        }

        private void VerifyBooleanPrimitive(
            FilterParamIndexBase index,
            bool testValue,
            int numExpected)
        {
            testBean.BoolPrimitive = testValue;
            index.MatchEvent(testEventBean, matchesList);
            Assert.AreEqual(numExpected, testEvaluator.GetAndResetCountInvoked());
        }

        private void VerifyString(
            FilterParamIndexBase index,
            string testValue,
            int numExpected)
        {
            testBean.TheString = testValue;
            index.MatchEvent(testEventBean, matchesList);
            Assert.AreEqual(numExpected, testEvaluator.GetAndResetCountInvoked());
        }

        private void VerifyFloatPrimitive(
            FilterParamIndexBase index,
            float testValue,
            int numExpected)
        {
            testBean.FloatPrimitive = testValue;
            index.MatchEvent(testEventBean, matchesList);
            Assert.AreEqual(numExpected, testEvaluator.GetAndResetCountInvoked());
        }

        private FilterParamIndexEquals MakeOne(
            string property,
            EventType testEventType)
        {
            return new FilterParamIndexEquals(MakeLookupable(property), new SlimReaderWriterLock());
        }

        private ExprFilterSpecLookupable MakeLookupable(string fieldName)
        {
            return new ExprFilterSpecLookupable(fieldName, testEventType.GetGetter(fieldName), testEventType.GetPropertyType(fieldName), false);
        }

        [Test, RunInApplicationDomain]
        public void TestBoolean()
        {
            var index = MakeOne("BoolPrimitive", testEventType);

            index.Put(false, testEvaluator);

            VerifyBooleanPrimitive(index, false, 1);
            VerifyBooleanPrimitive(index, true, 0);
        }

        [Test, RunInApplicationDomain]
        public void TestFloatPrimitive()
        {
            var index = MakeOne("FloatPrimitive", testEventType);

            index.Put(1.5f, testEvaluator);

            VerifyFloatPrimitive(index, 1.5f, 1);
            VerifyFloatPrimitive(index, 2.2f, 0);
            VerifyFloatPrimitive(index, 0, 0);
        }

        [Test, RunInApplicationDomain]
        public void TestLong()
        {
            var index = MakeOne("ShortBoxed", testEventType);

            index.Put((short) 1, testEvaluator);
            index.Put((short) 20, testEvaluator);

            VerifyShortBoxed(index, 10, 0);
            VerifyShortBoxed(index, 1, 1);
            VerifyShortBoxed(index, 20, 1);
            VerifyShortBoxed(index, null, 0);

            Assert.AreEqual(testEvaluator, index.Get((short) 1));
            Assert.IsTrue(index.ReadWriteLock != null);
            index.Remove((short) 1);
            index.Remove((short) 1);
            Assert.AreEqual(null, index.Get((short) 1));
        }

        [Test, RunInApplicationDomain]
        public void TestString()
        {
            var index = MakeOne("TheString", testEventType);

            index.Put("hello", testEvaluator);
            index.Put("test", testEvaluator);

            VerifyString(index, null, 0);
            VerifyString(index, "dudu", 0);
            VerifyString(index, "hello", 1);
            VerifyString(index, "test", 1);
        }
    }
} // end of namespace
