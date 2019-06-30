///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestPlaceholderParser : CommonTest
    {
        private void TryParseInvalid(string parseString)
        {
            try
            {
                PlaceholderParser.ParsePlaceholder(parseString);
                Assert.Fail();
            }
            catch (PlaceholderParseException ex)
            {
                // expected
            }
        }

        private PlaceholderParser.TextFragment TextF(string text)
        {
            return new PlaceholderParser.TextFragment(text);
        }

        private PlaceholderParser.ParameterFragment ParamF(string text)
        {
            return new PlaceholderParser.ParameterFragment(text);
        }

        [Test]
        public void TestParseInvalid()
        {
            TryParseInvalid("${lib");
            TryParseInvalid("${lib} ${aa");
        }

        [Test]
        public void TestParseValid()
        {
            object[][] testdata = {
                new object[] {"a  a $${lib}", new object[] {TextF("a  a ${lib}")}},
                new object[] {"a ${lib} b", new object[] {TextF("a "), ParamF("lib"), TextF(" b")}},
                new object[] {"${lib} b", new object[] {ParamF("lib"), TextF(" b")}},
                new object[] {"a${lib}", new object[] {TextF("a"), ParamF("lib")}},
                new object[] {"$${lib}", new object[] {TextF("${lib}")}},
                new object[] {"$${lib} c", new object[] {TextF("${lib} c")}},
                new object[] {"a$${lib}", new object[] {TextF("a${lib}")}},
                new object[] {
                    "sometext ${a} text $${d} ${e} text",
                    new object[] {TextF("sometext "), ParamF("a"), TextF(" text ${d} "), ParamF("e"), TextF(" text")}
                },
                new object[] {"$${lib} c $${lib}", new object[] {TextF("${lib} c ${lib}")}},
                new object[] {"$${lib}$${lib}", new object[] {TextF("${lib}${lib}")}},
                new object[] {"${xxx}$${lib}", new object[] {ParamF("xxx"), TextF("${lib}")}},
                new object[] {"$${xxx}${lib}", new object[] {TextF("${xxx}"), ParamF("lib")}},
                new object[] {"${lib} ${lib}", new object[] {ParamF("lib"), TextF(" "), ParamF("lib")}},
                new object[] {"${lib}${lib}", new object[] {ParamF("lib"), ParamF("lib")}},
                new object[] {"$${lib", new object[] {TextF("${lib")}},
                new object[] {"lib}", new object[] {TextF("lib}")}}
            };

            for (var i = 0; i < testdata.Length; i++)
            {
                TestParseValid(testdata[i]);
            }
        }

        [Test]
        public void TestParseValid(object[] inputAndResults)
        {
            var parseString = (string) inputAndResults[0];
            var expected = (object[]) inputAndResults[1];

            var result = PlaceholderParser.ParsePlaceholder(parseString);

            Assert.AreEqual(expected.Length, result.Count, "Incorrect count for '" + parseString + "'");
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i], "Incorrect value for '" + parseString + "' at " + i);
            }
        }
    }
} // end of namespace