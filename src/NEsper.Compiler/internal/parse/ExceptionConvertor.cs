///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Text;

using Antlr4.Runtime;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    /// <summary>
    /// Converts recognition exceptions.
    /// </summary>
    public class ExceptionConvertor
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal const string END_OF_INPUT_TEXT = "end-of-input";

        /// <summary>
        /// Converts from a syntax error to a nice statement exception.
        /// </summary>
        /// <param name="e">is the syntax error</param>
        /// <param name="expression">is the expression text</param>
        /// <param name="parser">the parser that parsed the expression</param>
        /// <param name="addPleaseCheck">indicates to add "please check" paraphrases</param>
        /// <returns>syntax exception</returns>
        public static StatementSpecCompileSyntaxException ConvertStatement(
            RecognitionException e,
            string expression,
            bool addPleaseCheck,
            EsperEPL2GrammarParser parser)
        {
            var pair = Convert(e, expression, addPleaseCheck, parser);
            return new StatementSpecCompileSyntaxException(pair.First, e, pair.Second);
        }

        /// <summary>
        /// Converts from a syntax error to a nice property exception.
        /// </summary>
        /// <param name="e">is the syntax error</param>
        /// <param name="expression">is the expression text</param>
        /// <param name="parser">the parser that parsed the expression</param>
        /// <param name="addPleaseCheck">indicates to add "please check" paraphrases</param>
        /// <returns>syntax exception</returns>
        public static PropertyAccessException ConvertProperty(
            RecognitionException e,
            string expression,
            bool addPleaseCheck,
            EsperEPL2GrammarParser parser)
        {
            var pair = Convert(e, expression, addPleaseCheck, parser);
            return new PropertyAccessException(pair.First, pair.Second);
        }

        /// <summary>
        /// Converts from a syntax error to a nice exception.
        /// </summary>
        /// <param name="e">is the syntax error</param>
        /// <param name="expression">is the expression text</param>
        /// <param name="parser">the parser that parsed the expression</param>
        /// <param name="addPleaseCheck">indicates to add "please check" paraphrases</param>
        /// <returns>syntax exception</returns>
        public static UniformPair<string> Convert(
            RecognitionException e,
            string expression,
            bool addPleaseCheck,
            EsperEPL2GrammarParser parser)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                var errorMessage = "Unexpected " + END_OF_INPUT_TEXT;
                return new UniformPair<string>(errorMessage, expression);
            }

            IToken t;
            IToken tBeforeBefore = null;
            IToken tBefore = null;
            IToken tAfter = null;

            var tIndex = e.OffendingToken != null ? e.OffendingToken.TokenIndex : int.MaxValue;
            if (tIndex < parser.TokenStream.Size)
            {
                t = parser.TokenStream.Get(tIndex);
                if ((tIndex + 1) < parser.TokenStream.Size)
                {
                    tAfter = parser.TokenStream.Get(tIndex + 1);
                }

                if (tIndex - 1 >= 0)
                {
                    tBefore = parser.TokenStream.Get(tIndex - 1);
                }

                if (tIndex - 2 >= 0)
                {
                    tBeforeBefore = parser.TokenStream.Get(tIndex - 2);
                }
            }
            else
            {
                if (parser.TokenStream.Size >= 1)
                {
                    tBeforeBefore = parser.TokenStream.Get(parser.TokenStream.Size - 1);
                }

                if (parser.TokenStream.Size >= 2)
                {
                    tBefore = parser.TokenStream.Get(parser.TokenStream.Size - 2);
                }

                t = parser.TokenStream.Get(parser.TokenStream.Size - 1);
            }

            IToken tEnd = null;
            if (parser.TokenStream.Size > 0)
            {
                tEnd = parser.TokenStream.Get(parser.TokenStream.Size - 1);
            }

            var positionInfo = GetPositionInfo(t);
            var token = t.Type == EsperEPL2GrammarParser.Eof ? "end-of-input" : "'" + t.Text + "'";

            var stack = parser.GetParaphrases();
            var check = "";
            var isSelect = stack.Count == 1 && (stack.Peek() == "select clause");
            if ((stack.Count > 0) && addPleaseCheck)
            {
                var delimiter = "";
                var checkList = new StringBuilder();
                checkList.Append(", please check the ");
                while (stack.Count != 0)
                {
                    checkList.Append(delimiter);
                    checkList.Append(stack.Pop());
                    delimiter = " within the ";
                }

                check = checkList.ToString();
            }

            // check if token is a reserved keyword
            var keywords = parser.GetKeywords();
            var reservedKeyword = false;
            if (keywords.Contains(token.ToLowerInvariant()))
            {
                token += " (a reserved keyword)";
                reservedKeyword = true;
            }
            else if (tAfter != null && keywords.Contains("'" + tAfter.Text.ToLowerInvariant() + "'"))
            {
                token += " ('" + tAfter.Text + "' is a reserved keyword)";
                reservedKeyword = true;
            }
            else
            {
                if ((tBefore != null) &&
                    (tAfter != null) &&
                    (keywords.Contains("'" + tBefore.Text.ToLowerInvariant() + "'")) &&
                    (keywords.Contains("'" + tAfter.Text.ToLowerInvariant() + "'")))
                {
                    token += " ('" + tBefore.Text + "' and '" + tAfter.Text + "' are a reserved keyword)";
                    reservedKeyword = true;
                }
                else if ((tBefore != null) &&
                         (keywords.Contains("'" + tBefore.Text.ToLowerInvariant() + "'")))
                {
                    token += " ('" + tBefore.Text + "' is a reserved keyword)";
                    reservedKeyword = true;
                }
                else if (tEnd != null && keywords.Contains("'" + tEnd.Text.ToLowerInvariant() + "'"))
                {
                    token += " ('" + tEnd.Text + "' is a reserved keyword)";
                    reservedKeyword = true;
                }
            }

            // special handling for the select-clause "as" keyword, which is required
            if (isSelect && !reservedKeyword)
            {
                check += GetSelectClauseAsText(tBeforeBefore, t);
            }

            var message = "Incorrect syntax near " + token + positionInfo + check;
            if (e is NoViableAltException || e is LexerNoViableAltException || CheckForInputMismatchWithNoExpected(e))
            {
                var nvaeToken = e.OffendingToken;
                var nvaeTokenType = nvaeToken != null ? nvaeToken.Type : EsperEPL2GrammarLexer.Eof;

                if (nvaeTokenType == EsperEPL2GrammarLexer.Eof)
                {
                    if (token.Equals(END_OF_INPUT_TEXT))
                    {
                        message = "Unexpected " + END_OF_INPUT_TEXT + positionInfo + check;
                    }
                    else
                    {
                        if (ParseHelper.HasControlCharacters(expression))
                        {
                            message = "Unrecognized control characters found in text" + positionInfo;
                        }
                        else
                        {
                            message = "Unexpected " + END_OF_INPUT_TEXT + " near " + token + positionInfo + check;
                        }
                    }
                }
                else {
                    var parserTokenParaphrases = EsperEPL2GrammarParser.GetParserTokenParaphrases();
                    if (parserTokenParaphrases.Get(nvaeTokenType) != null)
                    {
                        message = "Incorrect syntax near " + token + positionInfo + check;
                    }
                    else
                    {
                        // find next keyword in the next 3 tokens
                        var currentIndex = tIndex + 1;
                        while ((currentIndex > 0) &&
                               (currentIndex < parser.TokenStream.Size - 1) &&
                               (currentIndex < tIndex + 3))
                        {
                            var next = parser.TokenStream.Get(currentIndex);
                            currentIndex++;

                            var quotedToken = "'" + next.Text + "'";
                            if (parser.GetKeywords().Contains(quotedToken))
                            {
                                check += " near reserved keyword '" + next.Text + "'";
                                break;
                            }
                        }

                        message = "Incorrect syntax near " + token + positionInfo + check;
                    }
                }
            }
            else if (e is InputMismatchException)
            {
                var mismatched = (InputMismatchException) e;

                string expected;
                var expectedTokens = mismatched.GetExpectedTokens().ToList();
                if (expectedTokens.Count > 1)
                {
                    var writer = new StringWriter();
                    writer.Write("any of the following tokens {");
                    var delimiter = "";
                    for (var i = 0; i < expectedTokens.Count; i++)
                    {
                        writer.Write(delimiter);
                        if (i > 5)
                        {
                            writer.Write("...");
                            writer.Write(expectedTokens.Count - 5);
                            writer.Write(" more");
                            break;
                        }

                        delimiter = ", ";
                        writer.Write(GetTokenText(parser, expectedTokens[i]));
                    }

                    writer.Write("}");
                    expected = writer.ToString();
                }
                else
                {
                    expected = GetTokenText(parser, expectedTokens[0]);
                }

                var offendingTokenType = mismatched.OffendingToken.Type;
                var unexpected = GetTokenText(parser, offendingTokenType);

                var expecting = " expecting " + expected.Trim() + " but found " + unexpected.Trim();
                message = "Incorrect syntax near " + token + expecting + positionInfo + check;
            }

            return new UniformPair<string>(message, expression);
        }

        private static bool CheckForInputMismatchWithNoExpected(RecognitionException e)
        {
            if (!(e is InputMismatchException))
            {
                return false;
            }

            var expectedTokens = e.GetExpectedTokens().ToList();
            if (expectedTokens.Count > 1)
            {
                return false;
            }

            return expectedTokens.Count == 1 && expectedTokens[0] == -1;
        }

        private static string GetTokenText(
            EsperEPL2GrammarParser parser,
            int tokenIndex)
        {
            var expected = END_OF_INPUT_TEXT;
            var vocabulary = parser.Vocabulary;
            var vocabularyTokenText = vocabulary.GetLiteralName(tokenIndex) ?? vocabulary.GetSymbolicName(tokenIndex);
            if (vocabularyTokenText != null) {
                expected = vocabularyTokenText;
            }

            var lexerTokenParaphrases = EsperEPL2GrammarParser.GetLexerTokenParaphrases();
            if (lexerTokenParaphrases.Get(tokenIndex) != null)
            {
                expected = lexerTokenParaphrases.Get(tokenIndex);
            }

            var parserTokenParaphrases = EsperEPL2GrammarParser.GetParserTokenParaphrases();
            if (parserTokenParaphrases.Get(tokenIndex) != null)
            {
                expected = parserTokenParaphrases.Get(tokenIndex);
            }

            return expected;
        }

        /// <summary>
        /// Returns the position information string for a parser exception.
        /// </summary>
        /// <param name="t">the token to return the information for</param>
        /// <returns>is a string with line and column information</returns>
        private static string GetPositionInfo(IToken t)
        {
            return t.Line > 0 && t.Column > 0
                ? " at line " + t.Line + " column " + t.Column
                : "";
        }

        private static string GetSelectClauseAsText(
            IToken tBeforeBefore,
            IToken t)
        {
            if (tBeforeBefore != null &&
                tBeforeBefore.Type == EsperEPL2GrammarParser.IDENT &&
                t != null &&
                t.Type == EsperEPL2GrammarParser.IDENT)
            {
                return " (did you forget 'as'?)";
            }

            return "";
        }
    }
} // end of namespace