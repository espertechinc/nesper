///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.common.@internal.supportunit.@event;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    [TestFixture]
    public class TestArrayFieldPropertyGetter : AbstractCommonTest
    {
        private ArrayFieldPropertyGetter getter;
        private ArrayFieldPropertyGetter getterOutOfBounds;
        private EventBean theEvent;
        private SupportLegacyBean bean;

        [SetUp]
        public void SetUp()
        {
            bean = new SupportLegacyBean(new string[] { "a", "b" });
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
            ClassicAssert.AreEqual(bean.fieldStringArray[0], getter.Get(theEvent));
            ClassicAssert.AreEqual(bean.fieldStringArray[0], getter.Get(theEvent, 0));

            ClassicAssert.IsNull(getterOutOfBounds.Get(theEvent));
        }

        private ArrayFieldPropertyGetter MakeGetter(int index)
        {
            var field = typeof(SupportLegacyBean).GetField("fieldStringArray");
            return new ArrayFieldPropertyGetter(field, index, null, null);
        }
    }
} // end of namespace
