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

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.grammar.@internal.generated;
using com.espertech.esper.grammar.@internal.util;

namespace com.espertech.esper.compiler.@internal.parse
{
    /// <summary>
    /// Helper class for parsing an expression and walking a parse tree.
    /// </summary>
    public class ParseHelper
    {
        /// <summary>
        /// Newline.
        /// </summary>
        public static readonly string NEWLINE = Environment.NewLine;

        /// <summary>
        /// Walk parse tree starting at the rule the walkRuleSelector supplies.
        /// </summary>
        /// <param name="ast">ast to walk</param>
        /// <param name="listener">walker instance</param>
        /// <param name="expression">the expression we are walking in string form</param>
        /// <param name="eplStatementForErrorMsg">statement text for error messages</param>
        public static void Walk(
            ITree ast,
            EPLTreeWalkerListener listener,
            string expression,
            string eplStatementForErrorMsg)
        {
            // Walk tree
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".walk Walking AST using walker " + listener.GetType().Name);
            }

            var walker = new ParseTreeWalker();
            walker.Walk(listener, (IParseTree) ast);
            listener.End();
        }

        /// <summary>
        /// Parse expression using the rule the ParseRuleSelector instance supplies.
        /// </summary>
        /// <param name="expression">text to parse</param>
        /// <param name="parseRuleSelector">parse rule to select</param>
        /// <param name="addPleaseCheck">true to include depth paraphrase</param>
        /// <param name="eplStatementErrorMsg">text for error</param>
        /// <param name="rewriteScript">whether to rewrite script expressions</param>
        /// <returns>AST - syntax tree</returns>
        /// <throws>EPException                         when the AST could not be parsed</throws>
        /// <throws>StatementSpecCompileSyntaxException syntax exceptions</throws>
        public static ParseResult Parse(
            string expression,
            string eplStatementErrorMsg,
            bool addPleaseCheck,
            ParseRuleSelector parseRuleSelector,
            bool rewriteScript)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".parse Parsing expr=" + expression);
            }

            var input = new CaseInsensitiveInputStream(expression);
            var lex = NewLexer(input);

            var tokens = new CommonTokenStream(lex);
            var parser = ParseHelper.NewParser(tokens);

            ITree tree;
            try
            {
                tree = parseRuleSelector.InvokeParseRule(parser);
            }
            catch (RecognitionException ex)
            {
                tokens.Fill();
                if (rewriteScript && IsContainsScriptExpression(tokens))
                {
                    return HandleScriptRewrite(tokens, eplStatementErrorMsg, addPleaseCheck, parseRuleSelector);
                }

                Log.Debug("Error parsing statement [" + expression + "]", ex);
                throw ExceptionConvertor.ConvertStatement(ex, eplStatementErrorMsg, addPleaseCheck, parser);
            }
            catch (Exception e)
            {
                try
                {
                    tokens.Fill();
                }
                catch (Exception)
                {
                    Log.Debug("Token-fill produced exception: " + e.Message, e);
                }

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Error parsing statement [" + eplStatementErrorMsg + "]", e);
                }

                if (e.InnerException is RecognitionException recognitionException)
                {
                    if (rewriteScript && IsContainsScriptExpression(tokens))
                    {
                        return HandleScriptRewrite(tokens, eplStatementErrorMsg, addPleaseCheck, parseRuleSelector);
                    }

                    throw ExceptionConvertor.ConvertStatement(recognitionException, eplStatementErrorMsg, addPleaseCheck, parser);
                }
                else
                {
                    throw e;
                }
            }

            // if we are re-writing scripts and contain a script, then rewrite
            if (rewriteScript && IsContainsScriptExpression(tokens))
            {
                return HandleScriptRewrite(tokens, eplStatementErrorMsg, addPleaseCheck, parseRuleSelector);
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug(".parse Dumping AST...");
                ASTUtil.DumpAST(tree);
            }

            var expressionWithoutAnnotation = expression;
            if (tree is EsperEPL2GrammarParser.StartEPLExpressionRuleContext)
            {
                var epl = (EsperEPL2GrammarParser.StartEPLExpressionRuleContext) tree;
                expressionWithoutAnnotation = GetNoAnnotation(expression, epl.annotationEnum(), tokens);
            }

            return new ParseResult(tree, expressionWithoutAnnotation, tokens, new EmptyList<string>());
        }

        private static ParseResult HandleScriptRewrite(
            CommonTokenStream tokens,
            string eplStatementErrorMsg,
            bool addPleaseCheck,
            ParseRuleSelector parseRuleSelector)
        {
            var rewriteExpression = RewriteTokensScript(tokens);
            var result = Parse(rewriteExpression.RewrittenEPL, eplStatementErrorMsg, addPleaseCheck, parseRuleSelector, false);
            return new ParseResult(result.Tree, result.ExpressionWithoutAnnotations, result.TokenStream, rewriteExpression.Scripts);
        }

        private static string GetNoAnnotation(
            string expression,
            IList<EsperEPL2GrammarParser.AnnotationEnumContext> annos,
            CommonTokenStream tokens)
        {
            if (annos == null || annos.IsEmpty())
            {
                return expression;
            }

            var lastAnnotationToken = annos[annos.Count - 1].Stop;

            if (lastAnnotationToken == null)
            {
                return null;
            }

            try
            {
                var line = lastAnnotationToken.Line;
                var charpos = lastAnnotationToken.Column;
                var fromChar = charpos + lastAnnotationToken.Text.Length;
                if (line == 1)
                {
                    return expression.Substring(fromChar).Trim();
                }

                var lines = expression.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var buf = new StringBuilder();
                buf.Append(lines[line - 1].Substring(fromChar));
                for (var i = line; i < lines.Length; i++)
                {
                    buf.Append(lines[i]);
                    if (i < lines.Length - 1)
                    {
                        buf.Append(NEWLINE);
                    }
                }

                return buf.ToString().Trim();
            }
            catch (Exception ex)
            {
                Log.Error("Error determining non-annotated expression sting: " + ex.Message, ex);
            }

            return null;
        }

        private static ScriptResult RewriteTokensScript(CommonTokenStream tokens)
        {
            IList<string> scripts = new List<string>();

            IList<UniformPair<int>> scriptTokenIndexRanges = new List<UniformPair<int>>();
            for (var i = 0; i < tokens.Size; i++)
            {
                if (tokens.Get(i).Type == EsperEPL2GrammarParser.EXPRESSIONDECL)
                {
                    var tokenBefore = GetTokenBefore(i, tokens);
                    var isCreateExpressionClause = tokenBefore != null && tokenBefore.Type == EsperEPL2GrammarParser.CREATE;
                    var nameAndNameStart = FindScriptName(i + 1, tokens);

                    var startIndex = FindStartTokenScript(nameAndNameStart.Second, tokens, EsperEPL2GrammarParser.LBRACK);
                    if (startIndex != -1)
                    {
                        var endIndex = FindEndTokenScript(
                            startIndex + 1, tokens, EsperEPL2GrammarParser.RBRACK, EsperEPL2GrammarParser.GetAfterScriptTokens(),
                            !isCreateExpressionClause);
                        if (endIndex != -1)
                        {
                            var writer = new StringWriter();
                            for (var j = startIndex + 1; j < endIndex; j++)
                            {
                                writer.Write(tokens.Get(j).Text);
                            }

                            scripts.Add(writer.ToString());
                            scriptTokenIndexRanges.Add(new UniformPair<int>(startIndex, endIndex));
                        }
                    }
                }
            }

            var rewrittenEPL = RewriteScripts(scriptTokenIndexRanges, tokens);
            return new ScriptResult(rewrittenEPL, scripts);
        }

        private static IToken GetTokenBefore(
            int i,
            ITokenStream tokens)
        {
            var position = i - 1;
            while (position >= 0)
            {
                var t = tokens.Get(position);
                if (t.Channel != 99 && t.Type != EsperEPL2GrammarLexer.WS)
                {
                    return t;
                }

                position--;
            }

            return null;
        }

        private static Pair<string, int> FindScriptName(
            int start,
            ITokenStream tokens)
        {
            string lastIdent = null;
            var lastIdentIndex = 0;
            for (var i = start; i < tokens.Size; i++)
            {
                if (tokens.Get(i).Type == EsperEPL2GrammarParser.IDENT)
                {
                    lastIdent = tokens.Get(i).Text;
                    lastIdentIndex = i;
                }

                if (tokens.Get(i).Type == EsperEPL2GrammarParser.LPAREN)
                {
                    break;
                }

                // find beginning of script, ignore brackets
                if (tokens.Get(i).Type == EsperEPL2GrammarParser.LBRACK && tokens.Get(i + 1).Type != EsperEPL2GrammarParser.RBRACK)
                {
                    break;
                }
            }

            if (lastIdent == null)
            {
                throw new IllegalStateException("Failed to parse expression name");
            }

            return new Pair<string, int>(lastIdent, lastIdentIndex);
        }

        private static string RewriteScripts(
            IList<UniformPair<int>> ranges,
            ITokenStream tokens)
        {
            if (ranges.IsEmpty())
            {
                return tokens.GetText();
            }

            var writer = new StringWriter();
            var rangeIndex = 0;
            var current = ranges[rangeIndex];
            for (var i = 0; i < tokens.Size; i++)
            {
                var t = tokens.Get(i);
                if (t.Type == EsperEPL2GrammarLexer.Eof)
                {
                    break;
                }

                if (i < current.First)
                {
                    writer.Write(t.Text);
                }
                else if (i == current.First)
                {
                    writer.Write(t.Text);
                    writer.Write("'");
                }
                else if (i == current.Second)
                {
                    writer.Write("'");
                    writer.Write(t.Text);
                    rangeIndex++;
                    if (ranges.Count > rangeIndex)
                    {
                        current = ranges[rangeIndex];
                    }
                    else
                    {
                        current = new UniformPair<int>(-1, -1);
                    }
                }
                else if (t.Type == EsperEPL2GrammarParser.SL_COMMENT || t.Type == EsperEPL2GrammarParser.ML_COMMENT)
                {
                    WriteCommentEscapeSingleQuote(writer, t);
                }
                else
                {
                    if (t.Type == EsperEPL2GrammarParser.QUOTED_STRING_LITERAL && i > current.First && i < current.Second)
                    {
                        writer.Write("\\'");
                        writer.Write(t.Text.Substring(1, t.Text.Length - 1));
                        writer.Write("\\'");
                    }
                    else
                    {
                        writer.Write(t.Text);
                    }
                }
            }

            return writer.ToString();
        }

        private static int FindEndTokenScript(
            int startIndex,
            ITokenStream tokens,
            int tokenTypeSearch,
            ISet<int> afterScriptTokens,
            bool requireAfterScriptToken)
        {
            // The next non-comment token must be among the afterScriptTokens, i.e. SELECT/INSERT/ON/DELETE/UPDATE
            // Find next non-comment token.
            if (requireAfterScriptToken)
            {
                var found = -1;
                for (var i = startIndex; i < tokens.Size; i++)
                {
                    if (tokens.Get(i).Type == tokenTypeSearch)
                    {
                        for (var j = i + 1; j < tokens.Size; j++)
                        {
                            var next = tokens.Get(j);
                            if (next.Channel == 0)
                            {
                                if (afterScriptTokens.Contains(next.Type))
                                {
                                    found = i;
                                }

                                break;
                            }
                        }
                    }

                    if (found != -1)
                    {
                        break;
                    }
                }

                return found;
            }

            // Find the last token
            var indexLast = -1;
            for (var i = startIndex; i < tokens.Size; i++)
            {
                if (tokens.Get(i).Type == tokenTypeSearch)
                {
                    indexLast = i;
                }
            }

            return indexLast;
        }

        private static bool IsContainsScriptExpression(ITokenStream tokens)
        {
            for (var i = 0; i < tokens.Size; i++)
            {
                if (tokens.Get(i).Type == EsperEPL2GrammarParser.EXPRESSIONDECL)
                {
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

        private static int FindStartTokenScript(
            int startIndex,
            ITokenStream tokens,
            int tokenTypeSearch)
        {
            var found = -1;
            for (var i = startIndex; i < tokens.Size; i++)
            {
                if (tokens.Get(i).Type == tokenTypeSearch)
                {
                    return i;
                }
            }

            return found;
        }

        public static EsperEPL2GrammarLexer NewLexer(ICharStream input)
        {
            var lex = new EsperEPL2GrammarLexer(input);
            lex.RemoveErrorListeners();
            lex.AddErrorListener(Antlr4ErrorListener<int>.INSTANCE);
            return lex;
        }

        public static EsperEPL2GrammarParser NewParser(ITokenStream tokens)
        {
            var g = new EsperEPL2GrammarParser(tokens);
            g.RemoveErrorListeners();
            g.AddErrorListener(Antlr4ErrorListener<IToken>.INSTANCE);
            g.ErrorHandler = new Antlr4ErrorStrategy();
            return g;
        }

        public static bool HasControlCharacters(string text)
        {
            var textWithoutControlCharacters = text.RegexReplaceAll("\\p{Cc}", "");
            return !textWithoutControlCharacters.Equals(text);
        }

        private static void WriteCommentEscapeSingleQuote(
            StringWriter writer,
            IToken t)
        {
            var text = t.Text;
            if (!text.Contains("'"))
            {
                return;
            }

            for (var i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\'')
                {
                    writer.Write('\\');
                    writer.Write(c);
                }
                else
                {
                    writer.Write(c);
                }
            }
        }

        public class ScriptResult
        {
            public ScriptResult(
                string rewrittenEPL,
                IList<string> scripts)
            {
                this.RewrittenEPL = rewrittenEPL;
                this.Scripts = scripts;
            }

            public string RewrittenEPL { get; }

            public IList<string> Scripts { get; }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace