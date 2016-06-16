///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.generated;

namespace com.espertech.esper.epl.parse
{
    /// <summary>
    /// Helper class for parsing an expression and walking a parse tree.
    /// </summary>
    public class ParseHelper
    {
        /// <summary>
        /// Newline.
        /// </summary>
        public static readonly String NewLine = System.Environment.NewLine;
    
        /// <summary>
        /// Walk parse tree starting at the rule the walkRuleSelector supplies.
        /// </summary>
        /// <param name="ast">ast to walk</param>
        /// <param name="listener">walker instance</param>
        /// <param name="expression">the expression we are walking in string form</param>
        /// <param name="eplStatementForErrorMsg">statement text for error messages</param>
        public static void Walk(ITree ast, EPLTreeWalkerListener listener, String expression, String eplStatementForErrorMsg)
        {
            // Walk tree
            try
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".walk Walking AST using walker " + listener.GetType().FullName);
                }
                var walker = new ParseTreeWalker();
                walker.Walk(listener, (IParseTree) ast);
                listener.End();
            }
            catch (Exception e)
            {
                Log.Info("Error walking statement [" + expression + "]", e);
                throw;
            }
        }

        /// <summary>
        /// Parse expression using the rule the ParseRuleSelector instance supplies.
        /// </summary>
        /// <param name="expression">text to parse</param>
        /// <param name="eplStatementErrorMsg">text for error</param>
        /// <param name="addPleaseCheck">true to include depth paraphrase</param>
        /// <param name="parseRuleSelector">parse rule to select</param>
        /// <param name="rewriteScript">if set to <c>true</c> [rewrite script].</param>
        /// <returns>
        /// AST - syntax tree
        /// </returns>
        /// <exception cref="EPException">IOException parsing expression ' + expression + '\''</exception>
        /// <throws>EPException when the AST could not be parsed</throws>
        public static ParseResult Parse(String expression, String eplStatementErrorMsg, bool addPleaseCheck, ParseRuleSelector parseRuleSelector, bool rewriteScript)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug(".parse Parsing expr=" + expression);
            }
    
            ICharStream input;
            try {
                input = new NoCaseSensitiveStream(expression);
            } catch (IOException ex) {
                throw new EPException("IOException parsing expression '" + expression + '\'', ex);
            }
    
            var lex = NewLexer(input);
    
            var tokens = new CommonTokenStream(lex);
            var parser = ParseHelper.NewParser(tokens);
    
            ITree tree;
            try {
                tree = parseRuleSelector.Invoke(parser);
            }
            catch (RecognitionException ex) {
                tokens.Fill();
                if (rewriteScript && IsContainsScriptExpression(tokens)) {
                    return HandleScriptRewrite(tokens, eplStatementErrorMsg, addPleaseCheck, parseRuleSelector);
                }
                Log.Debug("Error parsing statement [" + expression + "]", ex);
                throw ExceptionConvertor.ConvertStatement(ex, eplStatementErrorMsg, addPleaseCheck, parser);
            }
            catch (Exception e) {
                try {
                    tokens.Fill();
                } catch (Exception ex) {
                    Log.Debug("Token-fill produced exception: " + ex.Message, ex);
                }

                if (Log.IsDebugEnabled) {
                    Log.Debug("Error parsing statement [" + eplStatementErrorMsg + "]", e);
                }
                if (e.InnerException is RecognitionException) {
                    if (rewriteScript && IsContainsScriptExpression(tokens)) {
                        return HandleScriptRewrite(tokens, eplStatementErrorMsg, addPleaseCheck, parseRuleSelector);
                    }
                    throw ExceptionConvertor.ConvertStatement((RecognitionException) e.InnerException, eplStatementErrorMsg, addPleaseCheck, parser);
                } else {
                    throw;
                }
            }
    
            // if we are re-writing scripts and contain a script, then rewrite
            if (rewriteScript && IsContainsScriptExpression(tokens)) {
                return HandleScriptRewrite(tokens, eplStatementErrorMsg, addPleaseCheck, parseRuleSelector);
            }
    
            if (Log.IsDebugEnabled) {
                Log.Debug(".parse Dumping AST...");
                ASTUtil.DumpAST(tree);
            }
    
            var expressionWithoutAnnotation = expression;
            if (tree is EsperEPL2GrammarParser.StartEPLExpressionRuleContext) {
                var epl = (EsperEPL2GrammarParser.StartEPLExpressionRuleContext) tree;
                expressionWithoutAnnotation = GetNoAnnotation(expression, epl.annotationEnum(), tokens);
            }
            else if (tree is EsperEPL2GrammarParser.StartPatternExpressionRuleContext) {
                var pattern = (EsperEPL2GrammarParser.StartPatternExpressionRuleContext) tree;
                expressionWithoutAnnotation = GetNoAnnotation(expression, pattern.annotationEnum(), tokens);
            }
    
            return new ParseResult(tree, expressionWithoutAnnotation, tokens, Collections.GetEmptyList<String>());
        }
    
        private static ParseResult HandleScriptRewrite(CommonTokenStream tokens, String eplStatementErrorMsg, bool addPleaseCheck, ParseRuleSelector parseRuleSelector) {
            var rewriteExpression = RewriteTokensScript(tokens);
            var result = Parse(rewriteExpression.RewrittenEPL, eplStatementErrorMsg, addPleaseCheck, parseRuleSelector, false);
            return new ParseResult(result.Tree, result.ExpressionWithoutAnnotations, result.TokenStream, rewriteExpression.Scripts);
        }
    
        private static String GetNoAnnotation(String expression, IList<EsperEPL2GrammarParser.AnnotationEnumContext> annos, CommonTokenStream tokens) {
            if (annos == null || annos.IsEmpty()) {
                return expression;
            }

            var lastAnnotationToken = annos[annos.Count - 1].Stop;
            if (lastAnnotationToken == null) {
                return null;
            }
    
            try {
                int line = lastAnnotationToken.Line;
                int charpos = lastAnnotationToken.Column;
                var fromChar = charpos + lastAnnotationToken.Text.Length;
                if (line == 1) {
                    return expression.Substring(fromChar).Trim();
                }
    
                var lines = expression.RegexSplit("\r\n|\r|\n");
                var buf = new StringBuilder();
                buf.Append(lines[line - 1].Substring(fromChar));
                for (var i = line; i < lines.Length; i++) {
                    buf.Append(lines[i]);
                    if (i < lines.Length - 1) {
                        buf.Append(NewLine);
                    }
                }
                return buf.ToString().Trim();
            } catch (Exception ex) {
                Log.Error("Error determining non-annotated expression sting: " + ex.Message, ex);
            }
            return null;
        }
    
        private static ScriptResult RewriteTokensScript(CommonTokenStream tokens) {
            IList<String> scripts = new List<String>();
    
            IList<UniformPair<int?>> scriptTokenIndexRanges = new List<UniformPair<int?>>();
            for (var i = 0; i < tokens.Size; i++) {
                if (tokens.Get(i).Type == EsperEPL2GrammarParser.EXPRESSIONDECL) {
                    var tokenBefore = GetTokenBefore(i, tokens);
                    var isCreateExpressionClause = tokenBefore != null && tokenBefore.Type == EsperEPL2GrammarParser.CREATE;
                    var nameAndNameStart = FindScriptName(i + 1, tokens);
    
                    var startIndex = FindStartTokenScript(nameAndNameStart.Second.Value, tokens, EsperEPL2GrammarParser.LBRACK);
                    if (startIndex != -1) {
                        var endIndex = FindEndTokenScript(startIndex + 1, tokens, EsperEPL2GrammarParser.RBRACK, EsperEPL2GrammarParser.GetAfterScriptTokens(), !isCreateExpressionClause);
                        if (endIndex != -1) {
    
                            var writer = new StringWriter();
                            for (var j = startIndex + 1; j < endIndex; j++) {
                                writer.Write(tokens.Get(j).Text);
                            }
                            scripts.Add(writer.ToString());
                            scriptTokenIndexRanges.Add(new UniformPair<int?>(startIndex, endIndex));
                        }
                    }
                }
            }
    
            var rewrittenEPL = RewriteScripts(scriptTokenIndexRanges, tokens);
            return new ScriptResult(rewrittenEPL, scripts);
        }
    
        private static IToken GetTokenBefore(int i, CommonTokenStream tokens) {
            var position = i-1;
            while (position >= 0) {
                var t = tokens.Get(position);
                if (t.Channel != 99 && t.Type != EsperEPL2GrammarLexer.WS) {
                    return t;
                }
                position--;
            }
            return null;
        }
    
        private static Pair<String, int?> FindScriptName(int start, CommonTokenStream tokens) {
            String lastIdent = null;
            var lastIdentIndex = 0;
            for (var i = start; i < tokens.Size; i++) {
                if (tokens.Get(i).Type == EsperEPL2GrammarParser.IDENT) {
                    lastIdent = tokens.Get(i).Text;
                    lastIdentIndex = i;
                }
                if (tokens.Get(i).Type == EsperEPL2GrammarParser.LPAREN) {
                    break;
                }
                // find beginning of script, ignore brackets
                if (tokens.Get(i).Type == EsperEPL2GrammarParser.LBRACK && tokens.Get(i+1).Type != EsperEPL2GrammarParser.RBRACK) {
                    break;
                }
            }
            if (lastIdent == null) {
                throw new IllegalStateException("Failed to parse expression name");
            }
            return new Pair<String, int?>(lastIdent, lastIdentIndex);
        }
    
        private static String RewriteScripts(IList<UniformPair<int?>> ranges, CommonTokenStream tokens)
        {
            if (ranges.IsEmpty()) {
                return tokens.GetText();
            }
            var writer = new StringWriter();
            var rangeIndex = 0;
            UniformPair<int?> current = ranges[rangeIndex];
            for (var i = 0; i < tokens.Size ; i++) {
                var t = tokens.Get(i);
                if (t.Type == EsperEPL2GrammarLexer.Eof) {
                    break;
                }
                if (i < current.First) {
                    writer.Write(t.Text);
                }
                else if (i == current.First) {
                    writer.Write(t.Text);
                    writer.Write("'");
                }
                else if (i == current.Second) {
                    writer.Write("'");
                    writer.Write(t.Text);
                    rangeIndex++;
                    if (ranges.Count > rangeIndex) {
                        current = ranges[rangeIndex];
                    }
                    else {
                        current = new UniformPair<int?>(-1, -1);
                    }
                }
                else {
                    if (t.Type == EsperEPL2GrammarParser.QUOTED_STRING_LITERAL && i > current.First && i < current.Second) {
                        writer.Write("\\'");
                        writer.Write(t.Text.Substring(1, t.Text.Length - 2));
                        writer.Write("\\'");
                    }
                    else {
                        writer.Write(t.Text);
                    }
                }
            }
            return writer.ToString();
        }
    
        private static int FindEndTokenScript(int startIndex, CommonTokenStream tokens, int tokenTypeSearch, ISet<int> afterScriptTokens, bool requireAfterScriptToken)
        {
            var found = -1;
            for (var i = startIndex; i < tokens.Size; i++) {
                if (tokens.Get(i).Type == tokenTypeSearch) {
                    if (!requireAfterScriptToken) {
                        return i;
                    }
                    // The next non-comment token must be among the afterScriptTokens, i.e. SELECT/INSERT/ON/DELETE/UPDATE
                    // Find next non-comment token.
                    for (var j = i + 1; j < tokens.Size; j++) {
                        var next = tokens.Get(j);
                        if (next.Channel == 0) {
                            if (afterScriptTokens.Contains(next.Type)) {
                                found = i;
                            }
                            break;
                        }
                    }
                }
                if (found != -1) {
                    break;
                }
            }
            return found;
        }
    
        private static bool IsContainsScriptExpression(CommonTokenStream tokens) {
            for (var i = 0; i < tokens.Size; i++) {
                if (tokens.Get(i).Type == EsperEPL2GrammarParser.EXPRESSIONDECL) {
                    var startTokenLcurly = FindStartTokenScript(i + 1, tokens, EsperEPL2GrammarParser.LCURLY);
                    var startTokenLbrack = FindStartTokenScript(i + 1, tokens, EsperEPL2GrammarParser.LBRACK);
                    // Handle:
                    // expression ABC { some[other] }
                    // expression boolean js:doit(...) [ {} ]
                    if (startTokenLbrack != -1 && (startTokenLcurly == -1 || startTokenLcurly > startTokenLbrack))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    
        private static int FindStartTokenScript(int startIndex, CommonTokenStream tokens, int tokenTypeSearch)
        {
            var found = -1;
            for (var i = startIndex; i < tokens.Size; i++) {
                if (tokens.Get(i).Type == tokenTypeSearch) {
                    return i;
                }
            }
            return found;
        }
    
        public static EsperEPL2GrammarLexer NewLexer(ICharStream input) {
            var lex = new EsperEPL2GrammarLexer(input);
            lex.RemoveErrorListeners();
            lex.AddErrorListener(Antlr4ErrorListener<int>.INSTANCE);
            return lex;
        }
    
        public static EsperEPL2GrammarParser NewParser(CommonTokenStream tokens) {
            var g = new EsperEPL2GrammarParser(tokens);
            g.RemoveErrorListeners();
            g.AddErrorListener(Antlr4ErrorListener<IToken>.INSTANCE);
            g.ErrorHandler = new Antlr4ErrorStrategy();
            return g;
        }

        public static bool HasControlCharacters(String text)
        {
            String textWithoutControlCharacters = text.RegexReplaceAll("\\p{Cc}", "");
            return !textWithoutControlCharacters.Equals(text);
        }

        internal class ScriptResult
        {
            internal ScriptResult(String rewrittenEPL, IList<String> scripts)
            {
                RewrittenEPL = rewrittenEPL;
                Scripts = scripts;
            }

            public string RewrittenEPL { get; private set; }

            public IList<string> Scripts { get; private set; }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
