///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Reflection;
using com.espertech.esper.compat.logging;
using NUnit.Framework;

namespace com.espertech.esper.type
{
    [TestFixture]
    public class TestLongValue
    {
        private void TryValid(String strLong, long expected)
        {
            long result = LongValue.ParseString(strLong);
            Assert.IsTrue(result == expected);
        }

        private void TryInvalid(String strLong)
        {
            try {
                LongValue.ParseString(strLong);
                Assert.IsTrue(false);
            }
            catch (Exception ex) {
                Log.Debug("Expected exception caught, msg=" + ex.Message);
            }
        }

        [Test]
        public void TestLong()
        {
            var lvp = new LongValue();

            Assert.IsTrue(lvp.ValueObject == null);
            lvp.Parse("10");
            Assert.IsTrue(lvp.ValueObject.Equals(10L));
            Assert.IsTrue(lvp.GetLong() == 10L);
            lvp._Long = 200;
            Assert.IsTrue(lvp.GetLong() == 200L);
            Assert.IsTrue(lvp.ValueObject.Equals(200L));

            try {
                lvp._Boolean = false;
                Assert.IsTrue(false);
            }
            catch (Exception) {
                // Expected exception
            }

            try {
                lvp._Int = 20;
                Assert.IsTrue(false);
            }
            catch (Exception) {
                // Expected exception
            }

            try {
                lvp._String = "test";
                Assert.IsTrue(false);
            }
            catch (Exception) {
                // Expected exception
            }

            try {
                lvp = new LongValue();
                lvp.GetLong();
            }
            catch (Exception) {
                // Expected exception
            }
        }

        [Test]
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

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
