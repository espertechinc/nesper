///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestFilterParamIndexEqualsTyped : AbstractRuntimeTest
    {
        private SupportEventEvaluator testEvaluator;
        private SupportBean testBean;
        private EventBean testEventBean;
        private EventType testEventType;
        private IList<FilterHandle> matchesList;

        [SetUp]
        public void SetUp()
        {
            testEvaluator = new SupportEventEvaluator();
            testBean = new SupportBean();
            testEventBean = SupportEventBeanFactory.GetInstance(Container).CreateObject(testBean);
            testEventType = testEventBean.EventType;
            matchesList = new List<FilterHandle>();
        }

        private ExprFilterSpecLookupable MakeLookupable(string fieldName)
        {
            var eval = new SupportExprEventEvaluator(testEventType.GetGetter(fieldName));
            return new ExprFilterSpecLookupable(fieldName, eval, null, testEventType.GetPropertyType(fieldName), false, null);
        }

        [Test, RunInApplicationDomain]
        public void TestIntPrimitive()
        {
            var index = new FilterParamIndexEqualsInt(MakeLookupable("IntPrimitive"), new SlimReaderWriterLock());
            index.Put(42, testEvaluator);

            testBean.IntPrimitive = 42;
            index.MatchEvent(testEventBean, matchesList, null);
            ClassicAssert.AreEqual(1, testEvaluator.GetAndResetCountInvoked());

            testBean.IntPrimitive = 99;
            index.MatchEvent(testEventBean, matchesList, null);
            ClassicAssert.AreEqual(0, testEvaluator.GetAndResetCountInvoked());
        }

        [Test, RunInApplicationDomain]
        public void TestLongPrimitive()
        {
            var index = new FilterParamIndexEqualsLong(MakeLookupable("LongPrimitive"), new SlimReaderWriterLock());
            index.Put(1000L, testEvaluator);

            testBean.LongPrimitive = 1000L;
            index.MatchEvent(testEventBean, matchesList, null);
            ClassicAssert.AreEqual(1, testEvaluator.GetAndResetCountInvoked());

            testBean.LongPrimitive = 999L;
            index.MatchEvent(testEventBean, matchesList, null);
            ClassicAssert.AreEqual(0, testEvaluator.GetAndResetCountInvoked());
        }

        [Test, RunInApplicationDomain]
        public void TestDoublePrimitive()
        {
            var index = new FilterParamIndexEqualsDouble(MakeLookupable("DoublePrimitive"), new SlimReaderWriterLock());
            index.Put(3.14, testEvaluator);

            testBean.DoublePrimitive = 3.14;
            index.MatchEvent(testEventBean, matchesList, null);
            ClassicAssert.AreEqual(1, testEvaluator.GetAndResetCountInvoked());

            testBean.DoublePrimitive = 2.71;
            index.MatchEvent(testEventBean, matchesList, null);
            ClassicAssert.AreEqual(0, testEvaluator.GetAndResetCountInvoked());
        }

        [Test, RunInApplicationDomain]
        public void TestNullValueNoMatch()
        {
            var index = new FilterParamIndexEqualsInt(MakeLookupable("IntBoxed"), new SlimReaderWriterLock());
            index.Put(5, testEvaluator);

            testBean.IntBoxed = null;
            index.MatchEvent(testEventBean, matchesList, null);
            ClassicAssert.AreEqual(0, testEvaluator.GetAndResetCountInvoked());
        }

        [Test, RunInApplicationDomain]
        public void TestPutGetRemove()
        {
            var index = new FilterParamIndexEqualsInt(MakeLookupable("IntPrimitive"), new SlimReaderWriterLock());
            index.Put(10, testEvaluator);

            ClassicAssert.AreEqual(testEvaluator, index.Get(10));
            ClassicAssert.IsNotNull(index.ReadWriteLock);
            ClassicAssert.AreEqual(1, index.CountExpensive);
            ClassicAssert.IsFalse(index.IsEmpty);

            index.Remove(10);
            ClassicAssert.IsNull(index.Get(10));
            ClassicAssert.IsTrue(index.IsEmpty);
        }
    }
}
