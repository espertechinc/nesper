///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using NUnit.Framework;


namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestPlaceholderParser 
    {
        [Test]
        public void TestParseValid()
        {
            Object[][] testdata = new Object[][] {
              new Object[] {"a  a $${lib}", new Object[] {TextF("a  a ${lib}") }},
              new Object[]{"a ${lib} b", new Object[] {TextF("a "), ParamF("lib"), TextF(" b")}},
              new Object[]{"${lib} b", new Object[] {ParamF("lib"), TextF(" b")}},
              new Object[]{"a${lib}", new Object[] {TextF("a"), ParamF("lib")}},
              new Object[]{"$${lib}", new Object[] {TextF("${lib}")}},
              new Object[]{"$${lib} c", new Object[] {TextF("${lib} c")}},
              new Object[]{"a$${lib}", new Object[] {TextF("a${lib}")}},
              new Object[]{"sometext ${a} text $${d} ${e} text",
                      new Object[] {TextF("sometext "), ParamF("a"), TextF(" text ${d} "), ParamF("e"), TextF(" text")}},
              new Object[]{"$${lib} c $${lib}", new Object[] {TextF("${lib} c ${lib}")}},
              new Object[]{"$${lib}$${lib}", new Object[] {TextF("${lib}${lib}")}},
              new Object[]{"${xxx}$${lib}", new Object[] {ParamF("xxx"), TextF("${lib}")}},
              new Object[]{"$${xxx}${lib}", new Object[] {TextF("${xxx}"), ParamF("lib")}},
              new Object[]{"${lib} ${lib}", new Object[] {ParamF("lib"), TextF(" "), ParamF("lib")}},
              new Object[]{"${lib}${lib}", new Object[] {ParamF("lib"), ParamF("lib")}},
              new Object[]{"$${lib", new Object[] {TextF("${lib")}},
              new Object[]{"lib}", new Object[] {TextF("lib}")}}
                };
    
            for (int i = 0; i < testdata.Length; i++)
            {
                TestParseValid(testdata[i]);
            }
        }
    
        public void TestParseValid(Object[] inputAndResults)
        {
            String parseString = (String) inputAndResults[0];
            Object[] expected = (Object[]) inputAndResults[1];
    
            IList<PlaceholderParser.Fragment> result = PlaceholderParser.ParsePlaceholder(parseString);

            Assert.AreEqual(expected.Length, result.Count, "Incorrect count for '" + parseString + "'");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i],result[i],"Incorrect value for '" + parseString + "' at " + i);
            }
        }
    
        [Test]
        public void TestParseInvalid()
        {
            TryParseInvalid("${lib");
            TryParseInvalid("${lib} ${aa");
        }
    
        private void TryParseInvalid(String parseString)
        {
            try
            {
                PlaceholderParser.ParsePlaceholder(parseString);
                Assert.Fail();
            }
            catch (PlaceholderParseException)
            {
                // expected
            }
        }
    
        private PlaceholderParser.TextFragment TextF(String text)
        {
            return new PlaceholderParser.TextFragment(text);
        }
    
        private PlaceholderParser.ParameterFragment ParamF(String text)
        {
            return new PlaceholderParser.ParameterFragment(text);
        }
    }
}
