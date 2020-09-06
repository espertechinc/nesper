///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestLongValue : AbstractCommonTest
    {
        [Test, RunInApplicationDomain]
        public void TestParseLong()
        {
            TryValid("0", 0);
            TryValid("11", 11);
            TryValid("12l", 12);
            TryValid("+234", 234);
            TryValid("29349349L", 29349349);
            TryValid("+29349349L", 29349349);
            TryValid("-2993L", -2993);
            TryValid("-1l", -1);

            TryInvalid("-+0");
            TryInvalid("0s");
            TryInvalid("");
            TryInvalid("l");
            TryInvalid("L");
            TryInvalid(null);
        }

        private void TryValid(string strLong, long expected)
        {
            long result = LongValue.ParseString(strLong);
            Assert.IsTrue(result == expected);
        }

        private void TryInvalid(string strLong)
        {
            try
            {
                LongValue.ParseString(strLong);
                Assert.IsTrue(false);
            }
            catch (Exception ex)
            {
                log.Debug("Expected exception caught, msg=" + ex.Message);
            }
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
