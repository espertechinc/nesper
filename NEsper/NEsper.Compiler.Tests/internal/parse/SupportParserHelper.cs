///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;
using com.espertech.esper.grammar.@internal.util;

namespace com.espertech.esper.compiler.@internal.parse
{
	public class SupportParserHelper
	{
		public static EPLTreeWalkerListener ParseAndWalkEPL(
			IContainer container,
			string expression)
		{
			Log.Debug(".parseAndWalk Trying text=" + expression);
			var ast = ParseEPL(expression);
			Log.Debug(".parseAndWalk success, tree walking...");
			SupportParserHelper.DisplayAST(ast.First);
			var listener = SupportEPLTreeWalkerFactory.MakeWalker(ast.Second, SupportClasspathImport.GetInstance(container));
			var walker = new ParseTreeWalker(); // create standard walker
			walker.Walk(listener, (IParseTree) ast.First); // initiate walk of tree with listener
			return listener;
		}

		public static void DisplayAST(ITree ast)
		{
			Log.Debug(".displayAST...");
			if (Log.IsDebugEnabled) {
				ASTUtil.DumpAST(ast);
			}
		}

		public static Pair<ITree, CommonTokenStream> ParseEPL(string text)
		{
			ParseRuleSelector startRuleSelector = new ProxyParseRuleSelector((parser) => parser.startEPLExpressionRule());
			return Parse(startRuleSelector, text);
		}

		public static Pair<ITree, CommonTokenStream> ParseEventProperty(string text)
		{
			ParseRuleSelector startRuleSelector = new ProxyParseRuleSelector((parser) => parser.startEventPropertyRule());
			return Parse(startRuleSelector, text);
		}

		public static Pair<ITree, CommonTokenStream> ParseJson(string text)
		{
			ParseRuleSelector startRuleSelector = new ProxyParseRuleSelector(parser => parser.startJsonValueRule());
			return Parse(startRuleSelector, text);
		}

		public static Pair<ITree, CommonTokenStream> Parse(
			ParseRuleSelector parseRuleSelector,
			string text)
		{
			var lex = ParseHelper.NewLexer(new CaseInsensitiveInputStream(text));

			var tokens = new CommonTokenStream(lex);
			var g = ParseHelper.NewParser(tokens);

			var ctx = parseRuleSelector.InvokeParseRule(g);
			return new Pair<ITree, CommonTokenStream>(ctx, tokens);
		}

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
