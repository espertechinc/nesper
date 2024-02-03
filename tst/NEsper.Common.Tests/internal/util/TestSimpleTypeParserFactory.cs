///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestSimpleTypeParserFactory : AbstractCommonTest
    {
        [Test]
        public void TestGetParser()
        {
            object[][] tests = new object[][]{
                    new object[] {typeof(bool?), "TrUe", true},
                    new object[]{typeof(bool?), "false", false},
                    new object[]{typeof(bool), "false", false},
                    new object[]{typeof(bool), "true", true},
                    new object[]{typeof(int), "73737474 ", 73737474},
                    new object[]{typeof(int?), " -1 ", -1},
                    new object[]{typeof(long), "123456789001222L", 123456789001222L},
                    new object[]{typeof(long?), " -2 ", -2L},
                    new object[]{typeof(long?), " -2L ", -2L},
                    new object[]{typeof(long?), " -2l ", -2L},
                    new object[]{typeof(short?), " -3 ", (short) -3},
                    new object[]{typeof(short), "111", (short) 111},
                    new object[]{typeof(double?), " -3d ", -3d},
                    new object[]{typeof(double), "111.38373", 111.38373d},
                    new object[]{typeof(double?), " -3.1D ", -3.1D},
                    new object[]{typeof(float?), " -3f ", -3f},
                    new object[]{typeof(float), "111.38373", 111.38373f},
                    new object[]{typeof(float?), " -3.1F ", -3.1f},
                    new object[]{typeof(sbyte), " -3 ", (sbyte) -3},
                    new object[]{typeof(byte), " 1 ", (byte) 1},
                    new object[]{typeof(char), "ABC", 'A'},
                    new object[]{typeof(char?), " AB", ' '},
                    new object[]{typeof(string), "AB", "AB"},
                    new object[]{typeof(string), " AB ", " AB "},
            };

            for (int i = 0; i < tests.Length; i++)
            {
                SimpleTypeParser parser = SimpleTypeParserFactory.GetParser((Type) tests[i][0]);
                ClassicAssert.AreEqual(tests[i][2], parser.Parse((string) tests[i][1]), "error in row:" + i);
            }
        }
    }
} // end of namespace
