///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    [TestFixture]
    public class TestReflectionPropMethodGetter : AbstractCommonTest
    {
        private EventBean unitTestBean;

        [SetUp]
        public void SetUp()
        {
            SupportBean testEvent = new SupportBean();
            testEvent.IntPrimitive = 10;
            testEvent.TheString = "a";
            testEvent.DoubleBoxed = null;

            unitTestBean = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, testEvent);
        }

        [Test]
        public void TestGetter()
        {
            ReflectionPropMethodGetter getter = MakeGetter(typeof(SupportBean), "GetIntPrimitive");
            ClassicAssert.AreEqual(10, getter.Get(unitTestBean));

            getter = MakeGetter(typeof(SupportBean), "GetTheString");
            ClassicAssert.AreEqual("a", getter.Get(unitTestBean));

            getter = MakeGetter(typeof(SupportBean), "GetDoubleBoxed");
            ClassicAssert.AreEqual(null, getter.Get(unitTestBean));
        }

        [Test]
        public void TestPerformance()
        {
            ReflectionPropMethodGetter getter = MakeGetter(typeof(SupportBean), "GetIntPrimitive");

            Log.Info(".testPerformance Starting test");

            for (int i = 0; i < 10; i++)   // Change to 1E8 for performance testing
            {
                int value = getter.Get(unitTestBean).AsInt32();
                ClassicAssert.AreEqual(10, value);
            }

            Log.Info(".testPerformance Done test");
        }

        private ReflectionPropMethodGetter MakeGetter(Type clazz, string methodName)
        {
            var method = clazz.GetMethod(methodName, new Type[] { });

            ReflectionPropMethodGetter getter = new ReflectionPropMethodGetter(method, null, null);

            return getter;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
