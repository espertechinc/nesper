///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.collection;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.parse;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.util;

namespace com.espertech.esper.supportunit.epl.parse
{
    public class SupportParserHelper
    {
        public static EPLTreeWalkerListener ParseAndWalkEPL(String expression)
        {
            var container = SupportContainer.Instance;
            return ParseAndWalkEPL(expression, 
                SupportEngineImportServiceFactory.Make(container),
                new VariableServiceImpl(container, 0, null, container.Resolve<EventAdapterService>(), null));
        }

        public static EPLTreeWalkerListener ParseAndWalkEPL(String expression, EngineImportService engineImportService, VariableService variableService)
        {
            var container = SupportContainer.Instance;

            Log.Debug(".parseAndWalk Trying text=" + expression);
            Pair<ITree, CommonTokenStream> ast = SupportParserHelper.ParseEPL(expression);
            Log.Debug(".parseAndWalk success, tree walking...");
            SupportParserHelper.DisplayAST(ast.First);

            EventAdapterService eventAdapterService = container.Resolve<EventAdapterService>();
            eventAdapterService.AddBeanType("SupportBean_N", typeof(SupportBean_N), true, true, true);

            EPLTreeWalkerListener listener = SupportEPLTreeWalkerFactory.MakeWalker(ast.Second, engineImportService, variableService);
            ParseTreeWalker walker = new ParseTreeWalker(); // create standard walker
            walker.Walk(listener, (IParseTree) ast.First); // initiate walk of tree with listener
            return listener;
        }

        public static EPLTreeWalkerListener ParseAndWalkPattern(String expression)
        {
            Log.Debug(".parseAndWalk Trying text=" + expression);
            Pair<ITree, CommonTokenStream> ast = SupportParserHelper.ParsePattern(expression);
            Log.Debug(".parseAndWalk success, tree walking...");
            SupportParserHelper.DisplayAST(ast.First);

            EPLTreeWalkerListener listener = SupportEPLTreeWalkerFactory.MakeWalker(ast.Second);
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(listener, (IParseTree) ast.First);
            return listener;
        }

        public static void DisplayAST(ITree ast)
        {
            Log.Debug(".displayAST...");
            if (Log.IsDebugEnabled)
            {
                ASTUtil.DumpAST(ast);
            }
        }
    
        public static Pair<ITree, CommonTokenStream> ParsePattern(String text)
        {
            ParseRuleSelector startRuleSelector = 
                parser => (ITree) parser.startPatternExpressionRule();
            return Parse(startRuleSelector, text);
        }
    
        public static Pair<ITree, CommonTokenStream> ParseEPL(String text)
        {
            ParseRuleSelector startRuleSelector =
                parser => (ITree) parser.startEPLExpressionRule();
            return Parse(startRuleSelector, text);
        }
    
        public static Pair<ITree, CommonTokenStream> ParseEventProperty(String text)
        {
            ParseRuleSelector startRuleSelector = 
                parser => (ITree) parser.startEventPropertyRule();
            return Parse(startRuleSelector, text);
        }

        public static Pair<ITree, CommonTokenStream> ParseJson(String text)
        {
            ParseRuleSelector startRuleSelector = 
                parser => parser.startJsonValueRule();
            return Parse(startRuleSelector, text);
        }

        public static Pair<ITree, CommonTokenStream> Parse(ParseRuleSelector parseRuleSelector, String text)
        {
            NoCaseSensitiveStream input;
            try
            {
                input = new NoCaseSensitiveStream(text);
            }
            catch (IOException ex)
            {
                throw new IOException("IOException parsing text '" + text + '\'', ex);
            }
    
            var lex = ParseHelper.NewLexer(input);
            var tokens = new CommonTokenStream(lex);
            var g = ParseHelper.NewParser(tokens);

            var ctx = parseRuleSelector.Invoke(g);
            return new Pair<ITree, CommonTokenStream>(ctx, tokens);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
