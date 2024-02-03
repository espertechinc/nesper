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

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    [TestFixture]
    public class TestArrayMethodPropertyGetter : AbstractCommonTest
    {
        private ArrayMethodPropertyGetter getter;
        private ArrayMethodPropertyGetter getterOutOfBounds;
        private EventBean theEvent;
        private SupportBeanComplexProps bean;

        [SetUp]
        public void SetUp()
        {
            bean = SupportBeanComplexProps.MakeDefaultBean();
            theEvent = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, bean);
            getter = MakeGetter(0);
            getterOutOfBounds = MakeGetter(Int32.MaxValue);
        }

        [Test]
        public void TestCtor()
        {
            try
            {
                MakeGetter(-1);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }

        [Test]
        public void TestGet()
        {
            ClassicAssert.AreEqual(bean.ArrayProperty[0], getter.Get(theEvent));
            ClassicAssert.AreEqual(bean.ArrayProperty[0], getter.Get(theEvent, 0));

            ClassicAssert.IsNull(getterOutOfBounds.Get(theEvent));
        }

        private ArrayMethodPropertyGetter MakeGetter(int index)
        {
            var property = typeof(SupportBeanComplexProps).GetProperty("ArrayProperty");
            ClassicAssert.IsNotNull(property);
            ClassicAssert.IsNotNull(property.GetMethod);
            return new ArrayMethodPropertyGetter(property.GetMethod, index, null, null);
        }
    }
} // end of namespace
