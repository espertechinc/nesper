///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.compat.logging;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    [TestFixture]
    public class TestReflectionPropFieldGetter : AbstractCommonTest
    {
        private EventBean unitTestBean;

        [SetUp]
        public void SetUp()
        {
            SupportLegacyBean testEvent = new SupportLegacyBean("a");
            unitTestBean = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, testEvent);
        }

        [Test]
        public void TestGetter()
        {
            ReflectionPropFieldGetter getter = MakeGetter(typeof(SupportLegacyBean), "fieldLegacyVal");
            ClassicAssert.AreEqual("a", getter.Get(unitTestBean));
        }

        private ReflectionPropFieldGetter MakeGetter(Type clazz, string fieldName)
        {
            var field = clazz.GetField(fieldName);
            var getter = new ReflectionPropFieldGetter(field, null, null);
            return getter;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
