///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using NUnit.Framework;

namespace com.espertech.esper.view
{
    [TestFixture]
    public class TestViewEnum 
    {
        [Test]
        public void TestForName()
        {
            ViewEnum? enumValue = ViewEnumExtensions.ForName(
                ViewEnum.CORRELATION.GetNamespace(),
                ViewEnum.CORRELATION.GetName());
            Assert.AreEqual(enumValue, ViewEnum.CORRELATION);

            enumValue = ViewEnumExtensions.ForName(ViewEnum.CORRELATION.GetNamespace(), "dummy");
            Assert.IsNull(enumValue);

            enumValue = ViewEnumExtensions.ForName("dummy", ViewEnum.CORRELATION.GetName());
            Assert.IsNull(enumValue);
        }
    }
}
