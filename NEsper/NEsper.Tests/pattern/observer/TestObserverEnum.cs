///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using NUnit.Framework;

namespace com.espertech.esper.pattern.observer
{
    [TestFixture]
    public class TestObserverEnum 
    {
        [Test]
        public void TestForName()
        {
            var enumValue = ObserverEnumExtensions.ForName(ObserverEnum.TIMER_INTERVAL.GetNamespace(), ObserverEnum.TIMER_INTERVAL.GetName());
            Assert.AreEqual(enumValue, ObserverEnum.TIMER_INTERVAL);

            enumValue = ObserverEnumExtensions.ForName(ObserverEnum.TIMER_INTERVAL.GetNamespace(), "dummy");
            Assert.IsNull(enumValue);

            enumValue = ObserverEnumExtensions.ForName("dummy", ObserverEnum.TIMER_INTERVAL.GetName());
            Assert.IsNull(enumValue);
        }
    }
}
