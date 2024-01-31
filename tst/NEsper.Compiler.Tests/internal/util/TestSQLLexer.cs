///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.compiler.@internal.util
{
    [TestFixture]
	public class TestSQLLexer  {
        [Test]
	    public void TestLexSampleSQL()
	    {
		    string[][] testcases = new string[][] {
			    new string[] {
				    "select * from A where a=b and c=d",
				    "select * from A where 1=0 and a=b and c=d"
			    },
			    new string[] {
				    "select * from A where 1=0", 
				    "select * from A where 1=0 and 1=0"
			    },
			    new string[] {
				    "select * from A", 
				    "select * from A where 1=0"
			    },
			    new string[] {
				    "select * from A group by x",
				    "select * from A where 1=0 group by x"
			    },
			    new string[] {
				    "select * from A having a>b",
				    "select * from A where 1=0 having a>b"
			    },
			    new string[] {
"select * from A Order by d",
"select * from A where 1=0 Order by d"			    },
			    new string[] {
"select * from A group by a having b>c Order by d",
"select * from A where 1=0 group by a having b>c Order by d"			    },
			    new string[] {
"select * from A where (7<4) group by a having b>c Order by d",
"select * from A where 1=0 and (7<4) group by a having b>c Order by d"			    },
			    new string[] {
				    "select * from A union select * from B",
				    "select * from A  where 1=0 union  select * from B where 1=0"
			    },
			    new string[] {
				    "select * from A where a=2 union select * from B where 2=3",
				    "select * from A where 1=0 and a=2 union  select * from B where 1=0 and 2=3"
			    },
			    new string[] {
				    "select * from A union select * from B union select * from C",
				    "select * from A  where 1=0 union  select * from B  where 1=0 union  select * from C where 1=0"
			    },
		    };

	        for (int i = 0; i < testcases.Length; i++) {
	            string result = null;
	            try {
	                result = SQLLexer.LexSampleSQL(testcases[i][0]).Trim();
	            } catch (Exception e) {
		            while (e != null) {
			            Console.WriteLine($"Exception: {e.GetType().Name}");
			            Console.WriteLine(e.StackTrace);
			            Console.WriteLine("----------------------------------------");
			            e = e.InnerException;
		            }
		            
	                Assert.Fail("failed case with exception:" + testcases[i][0]);
	            }
	            string expected = testcases[i][1].Trim();
	            ClassicAssert.AreEqual(expected, result, "failed case " + i + " :" + testcases[i][0]);
	        }
	    }
	}
} // end of namespace
