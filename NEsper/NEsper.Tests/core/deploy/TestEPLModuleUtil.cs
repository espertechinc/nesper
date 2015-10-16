///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using NUnit.Framework;

namespace com.espertech.esper.core.deploy
{
    [TestFixture]
    public class TestEPLModuleUtil
    {
        [Test]
        public void TestParse()
        {
            var testdata = new[] {
                    new Object[]
                    {
                        "/* Comment One */ select * from A;\n" +
                        "/* Comment Two */  select   *  from  B ;\n",
                        new [] {
                            new EPLModuleParseItem("/* Comment One */ select * from A", 1, 0, 33),
                            new EPLModuleParseItem("/* Comment Two */  select   *  from  B", 2, 34, 73)
                        },
                    },
                    
                    new Object[] 
                    {
                        "select /* Comment One\n\r; */ *, ';', \";\" from A order by x;; ;\n\n \n;\n" +
                        "/* Comment Two */  select   *  from  B ;\n",
                        new [] {
                            new EPLModuleParseItem("select /* Comment One\n\r; */ *, ';', \";\" from A order by x", 1, 0, 57),
                            new EPLModuleParseItem("/* Comment Two */  select   *  from  B", 6, 63, 102)
                        },
                    }
            };

            for (int i = 0; i < testdata.Length; i++)
            {
                var input = (String)testdata[i][0];
                var expected = (EPLModuleParseItem[])testdata[i][1];
                var result = EPLModuleUtil.Parse(input);

                Assert.AreEqual(expected.Length, result.Count);
                for (int j = 0; j < expected.Length; j++)
                {
                    String message = "failed at item " + i + " and segment " + j;
                    Assert.That(result[j].Expression, Is.EqualTo(expected[j].Expression), message);
                    Assert.That(result[j].LineNum, Is.EqualTo(expected[j].LineNum), message);
                    Assert.That(result[j].StartChar, Is.EqualTo(expected[j].StartChar), message);
                    Assert.That(result[j].EndChar, Is.EqualTo(expected[j].EndChar), message);
                }
            }
        }
    }
}
