///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using NUnit.Framework;
namespace com.espertech.esper.compiler.@internal.util
{
	[TestFixture]
	public class TestEPLModuleUtil
	{
		[Test]
		public void TestParse()
		{
			string epl;

			epl = "/* Comment One */\n" +
			      "select * from A;";
			RunAssertion(epl, new EPLModuleParseItem(epl.Replace(";", ""), 1, 0, 33, 2, 2, 2));

			epl = "/* Comment One */ select * from A;\n" +
			      "/* Comment Two */  select   *  from  B ;\n";
			RunAssertion(
				epl,
				new EPLModuleParseItem("/* Comment One */ select * from A", 1, 0, 33, 1, 1, 1),
				new EPLModuleParseItem("/* Comment Two */  select   *  from  B", 2, 34, 73, 2, 2, 2));

			epl = "select /* Comment One\n\r; */ *, ';', \";\" from A order by x;; ;\n\n \n;\n" +
			      "/* Comment Two */  select   *  from  B ;\n";
			RunAssertion(
				epl,
				new EPLModuleParseItem(
					"select /* Comment One\n\r; */ *, ';', \";\" from A order by x",
					1,
					0,
					57,
					2,
					1,
					2),
				new EPLModuleParseItem("/* Comment Two */  select   *  from  B", 6, 63, 102, 6, 6, 6));
		}

		private void RunAssertion(
			string epl,
			params EPLModuleParseItem[] expecteds)
		{
			var result = EPLModuleUtil.Parse(epl);
			Assert.AreEqual(result.Count, expecteds.Length);
			for (int i = 0; i < expecteds.Length; i++) {
				string message = "failed at epl:\n-----\n" + epl + "-----\nfailed at module item #" + i;
				Assert.AreEqual(expecteds[i].Expression, result[i].Expression, message);
				Assert.AreEqual(expecteds[i].LineNum, result[i].LineNum, message);
				Assert.AreEqual(expecteds[i].StartChar, result[i].StartChar, message);
				Assert.AreEqual(expecteds[i].EndChar, result[i].EndChar, message);
				Assert.AreEqual(expecteds[i].LineNumEnd, result[i].LineNumEnd, message);
				Assert.AreEqual(expecteds[i].LineNumContent, result[i].LineNumContent, message);
				Assert.AreEqual(expecteds[i].LineNumContentEnd, result[i].LineNumContentEnd, message);
			}
		}
	}
} // end of namespace
