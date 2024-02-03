///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestStringValue : AbstractCommonTest
    {
        [Test]
        public void TestParse()
        {
            ClassicAssert.AreEqual("a", StringValue.ParseString("\"a\""));
            ClassicAssert.AreEqual("", StringValue.ParseString("\"\""));
            ClassicAssert.AreEqual("", StringValue.ParseString("''"));
            ClassicAssert.AreEqual("b", StringValue.ParseString("'b'"));
        }

        [Test]
        public void TestInvalid()
        {
            TryInvalid("\"");
            TryInvalid("'");
        }

        [Test]
        public void TestRenderEPL()
        {
            ClassicAssert.AreEqual("null", TryConstant(null));
            ClassicAssert.AreEqual("\"\"", TryConstant(""));
            ClassicAssert.AreEqual("1", TryConstant(1));
            ClassicAssert.AreEqual("\"abc\"", TryConstant("abc"));
        }

        [Test]
        public void TestUnescapeIndexOf()
        {
            object[][] inout = new object[][]{
                new object[]{"a", -1},
                new object[]{"", -1},
                new object[]{" ", -1},
                new object[]{".", 0},
                new object[]{" . .", 1},
                new object[]{"a.", 1},
                new object[]{".a", 0},
                new object[]{"a.b", 1},
                new object[]{"a..b", 1},
                new object[]{"a\\.b", -1},
                new object[]{"a.\\..b", 1},
                new object[]{"a\\..b", 3},
                new object[]{"a.b.c", 1},
                new object[]{"abc.", 3}
            };

            for (int i = 0; i < inout.Length; i++)
            {
                string input = (string) inout[i][0];
                int expected = (int) inout[i][1];
                ClassicAssert.AreEqual(expected, StringValue.UnescapedIndexOfDot(input), "for input " + input);
            }
        }

        private string TryConstant(object value)
        {
            StringWriter writer = new StringWriter();
            StringValue.RenderConstantAsEPL(writer, value);
            return writer.ToString();
        }

        private void TryInvalid(string invalidString)
        {
            try
            {
                StringValue.ParseString(invalidString);
            }
            catch (ArgumentException)
            {
                // Expected exception
            }
        }
    }
} // end of namespace
