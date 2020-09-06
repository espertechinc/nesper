///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;
namespace com.espertech.esper.compiler.@internal.util
{
    [TestFixture]
	public class TestEPLModuleUtil  {
		[Test, RunInApplicationDomain]
		public void TestParse()
		{

			object[][] testdata = new object[][] {
				new object[] {
					"/* Comment One */ select * from A;\n" +
					"/* Comment Two */  select   *  from  B ;\n",
					new EPLModuleParseItem[] {
						new EPLModuleParseItem("/* Comment One */ select * from A", 1, 0, 33),
						new EPLModuleParseItem("/* Comment Two */  select   *  from  B", 2, 34, 73)
					},
				},
				new object[] {
					"select /* Comment One\n\r; */ *, ';', \";\" from A order by x;; ;\n\n \n;\n" +
					"/* Comment Two */  select   *  from  B ;\n",
					new EPLModuleParseItem[] {
						new EPLModuleParseItem("select /* Comment One\n\r; */ *, ';', \";\" from A order by x", 1, 0, 57),
						new EPLModuleParseItem("/* Comment Two */  select   *  from  B", 6, 63, 102)
					},
				}
			};

			for (int i = 0; i < testdata.Length; i++) {
				string input = (string) testdata[i][0];
				EPLModuleParseItem[] expected = (EPLModuleParseItem[]) testdata[i][1];
				IList<EPLModuleParseItem> result = EPLModuleUtil.Parse(input);

				Assert.AreEqual(expected.Length, result.Count);
				for (int j = 0; j < expected.Length; j++) {
					string message = "failed at item " + i + " and segment " + j;
					Assert.AreEqual(expected[j].Expression, result[j].Expression, message);
					Assert.AreEqual(expected[j].LineNum, result[j].LineNum, message);
					Assert.AreEqual(expected[j].StartChar, result[j].StartChar, message);
					Assert.AreEqual(expected[j].EndChar, result[j].EndChar, message);
				}
			}
		}
	}
} // end of namespace
