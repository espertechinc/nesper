///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.type
{
    [TestFixture]
    public class TestCronParameter : AbstractCommonTest
    {
        [Test]
        public void TestFormat()
        {
            CronParameter cronParameter = new CronParameter(CronOperatorEnum.LASTDAY, 1);
            ClassicAssert.AreEqual("LASTDAY(day 1 month null)", cronParameter.Formatted());
        }
    }
} // end of namespace
