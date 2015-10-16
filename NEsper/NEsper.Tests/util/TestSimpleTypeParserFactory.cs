///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestSimpleTypeParserFactory
    {
        [Test]
        public void TestGetParser()
        {
            var tests = new[]
                        {
                            new Object[] {typeof(bool?), "TrUe", true},
                            new Object[] {typeof(bool?), "false", false},
                            new Object[] {typeof(bool), "false", false},
                            new Object[] {typeof(bool), "true", true},
                            new Object[] {typeof(int), "73737474 ", 73737474},
                            new Object[] {typeof(int?), " -1 ", -1},
                            new Object[] {typeof(long), "123456789001222L", 123456789001222L},
                            new Object[] {typeof(long?), " -2 ", -2L},
                            new Object[] {typeof(long?), " -2L ", -2L},
                            new Object[] {typeof(long?), " -2l ", -2L},
                            new Object[] {typeof(short?), " -3 ", (short) -3},
                            new Object[] {typeof(short), "111", (short) 111},
                            new Object[] {typeof(double?), " -3d ", -3d},
                            new Object[] {typeof(double), "111.38373", 111.38373d},
                            new Object[] {typeof(double?), " -3.1D ", -3.1D},
                            new Object[] {typeof(float?), " -3f ", -3f},
                            new Object[] {typeof(float), "111.38373", 111.38373f},
                            new Object[] {typeof(float?), " -3.1F ", -3.1f},
                            new Object[] {typeof(sbyte?), " -3 ", (sbyte) -3},
                            new Object[] {typeof(sbyte), " 1 ", (sbyte) 1},
                            new Object[] {typeof(char), "ABC", 'A'},
                            new Object[] {typeof(char?), " AB", ' '},
                            new Object[] {typeof(string), "AB", "AB"},
                            new Object[] {typeof(string), " AB ", " AB "},
                        };

            for (int i = 0; i < tests.Length; i++) {
                SimpleTypeParser parser = SimpleTypeParserFactory.GetParser((Type) tests[i][0]);
                Assert.AreEqual(tests[i][2], parser.Invoke((String) tests[i][1]), "error in row:" + i);
            }
        }
    }
}
