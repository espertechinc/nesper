///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestFilterParamIndexCompare : AbstractRuntimeTest
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

        private FilterParamIndexCompare MakeOne(
            string field,
            FilterOperator op)
        {
            return new FilterParamIndexCompare(MakeLookupable(field), new SlimReaderWriterLock(), op);
        }

        private void VerifyDoublePrimitive(
            FilterParamIndexBase index,
            double testValue,
            int numExpected)
        {
            testBean.DoublePrimitive = testValue;
            index.MatchEvent(testEventBean, matchesList);
            Assert.AreEqual(numExpected, testEvaluator.GetAndResetCountInvoked());
        }

        private void VerifyDoubleBoxed(
            FilterParamIndexBase index,
            double testValue,
            int numExpected)
        {
            testBean.DoubleBoxed = testValue;
            index.MatchEvent(testEventBean, matchesList);
            Assert.AreEqual(numExpected, testEvaluator.GetAndResetCountInvoked());
        }

        private void VerifyLongBoxed(
            FilterParamIndexBase index,
            long? testValue,
            int numExpected)
        {
            testBean.LongBoxed = testValue;
            index.MatchEvent(testEventBean, matchesList);
            Assert.AreEqual(numExpected, testEvaluator.GetAndResetCountInvoked());
        }

        private void VerifyLongPrimitive(
            FilterParamIndexBase index,
            long testValue,
            int numExpected)
        {
            testBean.LongPrimitive = testValue;
            index.MatchEvent(testEventBean, matchesList);
            Assert.AreEqual(numExpected, testEvaluator.GetAndResetCountInvoked());
        }

        private ExprFilterSpecLookupable MakeLookupable(string fieldName)
        {
            return new ExprFilterSpecLookupable(fieldName, testEventType.GetGetter(fieldName), testEventType.GetPropertyType(fieldName), false);
        }

        [Test, RunInApplicationDomain]
        public void TestMatchDoubleAndGreater()
        {
            var index = MakeOne("DoublePrimitive", FilterOperator.GREATER);

            index.Put(1.5d, testEvaluator);
            index.Put(2.1d, testEvaluator);
            index.Put(2.2d, testEvaluator);

            VerifyDoublePrimitive(index, 1.5, 0);
            VerifyDoublePrimitive(index, 1.7, 1);
            VerifyDoublePrimitive(index, 2.2, 2);
            VerifyDoublePrimitive(index, 2.1999999, 2);
            VerifyDoublePrimitive(index, -1, 0);
            VerifyDoublePrimitive(index, 99, 3);

            Assert.AreEqual(testEvaluator, index.Get(1.5d));
            Assert.IsTrue(index.ReadWriteLock != null);
            index.Remove(1.5d);
            index.Remove(1.5d);
            Assert.AreEqual(null, index.Get(1.5d));

            Assert.That(
                () => index.Put("a", testEvaluator),
                Throws.InstanceOf<InvalidOperationException>());
        }

        [Test, RunInApplicationDomain]
        public void TestMatchDoubleAndLessOrEqualThan()
        {
            var index = MakeOne("DoubleBoxed", FilterOperator.LESS_OR_EQUAL);

            index.Put(7.4D, testEvaluator);
            index.Put(7.5D, testEvaluator);
            index.Put(7.6D, testEvaluator);

            VerifyDoubleBoxed(index, 7.39, 3);
            VerifyDoubleBoxed(index, 7.4, 3);
            VerifyDoubleBoxed(index, 7.41, 2);
            VerifyDoubleBoxed(index, 7.5, 2);
            VerifyDoubleBoxed(index, 7.51, 1);
            VerifyDoubleBoxed(index, 7.6, 1);
            VerifyDoubleBoxed(index, 7.61, 0);
        }

        [Test, RunInApplicationDomain]
        public void TestMatchLongAndGreaterEquals()
        {
            var index = MakeOne("LongBoxed", FilterOperator.GREATER_OR_EQUAL);

            index.Put(1L, testEvaluator);
            index.Put(2L, testEvaluator);
            index.Put(4L, testEvaluator);

            // Should not match with null
            VerifyLongBoxed(index, null, 0);

            VerifyLongBoxed(index, 0L, 0);
            VerifyLongBoxed(index, 1L, 1);
            VerifyLongBoxed(index, 2L, 2);
            VerifyLongBoxed(index, 3L, 2);
            VerifyLongBoxed(index, 4L, 3);
            VerifyLongBoxed(index, 10L, 3);

            // Put a long primitive in - should work
            index.Put(9L, testEvaluator);

            Assert.That(
                () => index.Put(10, testEvaluator),
                Throws.InstanceOf<InvalidOperationException>());
        }

        [Test, RunInApplicationDomain]
        public void TestMatchLongAndLessThan()
        {
            var index = MakeOne("LongPrimitive", FilterOperator.LESS);

            index.Put(1L, testEvaluator);
            index.Put(10L, testEvaluator);
            index.Put(100L, testEvaluator);

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
    }
} // end of namespace
