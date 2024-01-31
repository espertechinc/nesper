///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportBeanAssertionUtil
    {
        public static void AssertPropsPerRow(
            object[] beans,
            string[] fields,
            object[][] expected)
        {
            ClassicAssert.AreEqual(beans.Length, expected.Length);
            for (var i = 0; i < beans.Length; i++) {
                AssertPropsBean((SupportBean) beans[i], fields, expected[i]);
            }
        }

        public static void AssertPropsBean(
            SupportBean bean,
            string[] fields,
            object[] expected)
        {
            var count = -1;
            foreach (var field in fields) {
                count++;
                if (field.Equals("TheString")) {
                    ClassicAssert.AreEqual(expected[count], bean.TheString);
                }
                else if (field.Equals("IntPrimitive")) {
                    ClassicAssert.AreEqual(expected[count], bean.IntPrimitive);
                }
                else if (field.Equals("LongPrimitive")) {
                    ClassicAssert.AreEqual(expected[count], bean.LongPrimitive);
                }
                else {
                    Assert.Fail("unrecognized field " + field);
                }
            }
        }
    }
} // end of namespace