///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

using NUnit.Framework;

namespace com.espertech.esper.compiler.@internal.parse
{
	[TestFixture]
	public class TestJsonParser
	{

		[Test]
		public void TestParse()
		{
			object result;

			Assert.AreEqual("abc", ParseLoadJson("\"abc\""));
			Assert.AreEqual("http://www.uri.com", ParseLoadJson("\"http://www.uri.com\""));
			Assert.AreEqual("new\nline", ParseLoadJson("\"new\\nline\""));
			Assert.AreEqual(" ~ ", ParseLoadJson("\" \\u007E \""));
			Assert.AreEqual("/", ParseLoadJson("\"\\/\""));
			Assert.AreEqual(true, ParseLoadJson("true"));
			Assert.AreEqual(false, ParseLoadJson("false"));
			Assert.AreEqual(null, ParseLoadJson("null"));
			Assert.AreEqual(10, ParseLoadJson("10"));
			Assert.AreEqual(-10, ParseLoadJson("-10"));
			Assert.AreEqual(20L, ParseLoadJson("20L"));
			Assert.AreEqual(5.5d, ParseLoadJson("5.5"));

			result = ParseLoadJson("{\"name\":\"myname\",\"value\":5}");
			EPAssertionUtil.AssertPropsMap(result.AsStringDictionary(), "name,value".SplitCsv(), "myname", 5);

			result = ParseLoadJson("{name:\"myname\",value:5}");
			EPAssertionUtil.AssertPropsMap(result.AsStringDictionary(), "name,value".SplitCsv(), "myname", 5);

			result = ParseLoadJson("[\"one\",2]");
			EPAssertionUtil.AssertEqualsExactOrder(new object[] {"one", 2}, (IList<object>) result);

			result = ParseLoadJson("{\"one\": { 'a' : 2 } }");
			var inner = result.AsStringDictionary().Get("one").AsStringDictionary();
			Assert.AreEqual(1, inner.Count);

			var json = "{\n" +
			           "    \"glossary\": {\n" +
			           "        \"title\": \"example glossary\",\n" +
			           "\t\t\"GlossDiv\": {\n" +
			           "            \"title\": \"S\",\n" +
			           "\t\t\t\"GlossList\": {\n" +
			           "                \"GlossEntry\": {\n" +
			           "                    \"ID\": \"SGML\",\n" +
			           "\t\t\t\t\t\"SortAs\": \"SGML\",\n" +
			           "\t\t\t\t\t\"GlossTerm\": \"Standard Generalized Markup Language\",\n" +
			           "\t\t\t\t\t\"Acronym\": \"SGML\",\n" +
			           "\t\t\t\t\t\"Abbrev\": \"ISO 8879:1986\",\n" +
			           "\t\t\t\t\t\"GlossDef\": {\n" +
			           "                        \"para\": \"A meta-markup language, used to create markup languages such as DocBook.\",\n" +
			           "\t\t\t\t\t\t\"GlossSeeAlso\": [\"GML\", \"XML\"]\n" +
			           "                    },\n" +
			           "\t\t\t\t\t\"GlossSee\": \"markup\"\n" +
			           "                }\n" +
			           "            }\n" +
			           "        }\n" +
			           "    }\n" +
			           "}";
			var tree = ParseJson(json).First;
			ASTUtil.DumpAST(tree);
			var loaded = ParseLoadJson(json);
			Assert.AreEqual(
				"{glossary={title=example glossary, GlossDiv={title=S, GlossList={GlossEntry={ID=SGML, SortAs=SGML, GlossTerm=Standard Generalized Markup Language, Acronym=SGML, Abbrev=ISO 8879:1986, GlossDef={para=A meta-markup language, used to create markup languages such as DocBook., GlossSeeAlso=[GML, XML]}, GlossSee=markup}}}}}",
				loaded.ToString());
		}

		private object ParseLoadJson(string expression)
		{
			var parsed = ParseJson(expression);
			var tree = (EsperEPL2GrammarParser.StartJsonValueRuleContext) parsed.First;
			Assert.AreEqual(EsperEPL2GrammarParser.RULE_startJsonValueRule, ASTUtil.GetRuleIndexIfProvided(tree));
			ITree root = tree.GetChild(0);
			ASTUtil.DumpAST(root);
			return ASTJsonHelper.Walk(parsed.Second, tree.jsonvalue());
		}

		private Pair<ITree, CommonTokenStream> ParseJson(string expression)
		{
			return SupportParserHelper.ParseJson(expression);
		}
	}
} // end of namespace
