///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Text;

using Antlr4.Runtime;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.generated;

namespace com.espertech.esper.epl.parse
{
    /// <summary>
    /// Converts recognition exceptions.
    /// </summary>
    public class ExceptionConvertor
    {
        protected const string END_OF_INPUT_TEXT = "end-of-input";
    
        /// <summary>
        /// Converts from a syntax error to a nice statement exception.
        /// </summary>
        /// <param name="e">is the syntax error</param>
        /// <param name="expression">is the expression text</param>
        /// <param name="parser">the parser that parsed the expression</param>
        /// <param name="addPleaseCheck">indicates to add "please check" paraphrases</param>
        /// <returns>syntax exception</returns>
        public static EPStatementSyntaxException ConvertStatement(RecognitionException e, string expression, bool addPleaseCheck, EsperEPL2GrammarParser parser)
        {
            var pair = Convert(e, expression, addPleaseCheck, parser);
            return new EPStatementSyntaxException(pair.First, pair.Second);
        }
    
        /// <summary>
        /// Converts from a syntax error to a nice property exception.
        /// </summary>
        /// <param name="e">is the syntax error</param>
        /// <param name="expression">is the expression text</param>
        /// <param name="parser">the parser that parsed the expression</param>
        /// <param name="addPleaseCheck">indicates to add "please check" paraphrases</param>
        /// <returns>syntax exception</returns>
        public static PropertyAccessException ConvertProperty(RecognitionException e, string expression, bool addPleaseCheck, EsperEPL2GrammarParser parser)
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
        public static UniformPair<string> Convert(RecognitionException e, string expression, bool addPleaseCheck, EsperEPL2GrammarParser parser)
        {
            string message;

            if (expression.Trim().Length == 0)
            {
                message = "Unexpected " + END_OF_INPUT_TEXT;
                return new UniformPair<string>(message, expression);
            }

            IToken t;
            IToken tBeforeBefore = null;
            IToken tBefore = null;
            IToken tAfter = null;

            ITokenStream tokenStream = parser.InputStream as ITokenStream;

            var tIndex = e.OffendingToken != null ? e.OffendingToken.TokenIndex : int.MaxValue;
            if (tIndex < tokenStream.Size)
            {
                t = tokenStream.Get(tIndex);
                if ((tIndex + 1) < tokenStream.Size)
                {
                    tAfter = tokenStream.Get(tIndex + 1);
                }
                if (tIndex - 1 >= 0) {
                    tBefore = tokenStream.Get(tIndex - 1);
                }
                if (tIndex - 2 >= 0) {
                    tBeforeBefore = tokenStream.Get(tIndex - 2);
                }
            }
            else
            {
                if (tokenStream.Size >= 1) {
                    tBeforeBefore = tokenStream.Get(tokenStream.Size - 1);
                }
                if (tokenStream.Size >= 2) {
                    tBefore = tokenStream.Get(tokenStream.Size - 2);
                }
                t = tokenStream.Get(tokenStream.Size - 1);
            }
    
            IToken tEnd = null;
            if (tokenStream.Size > 0) {
                tEnd = tokenStream.Get(tokenStream.Size - 1);
            }
    
            var positionInfo = GetPositionInfo(t);
            var token = t.Type == EsperEPL2GrammarParser.Eof ? "end-of-input" : "'" + t.Text + "'";
    
            var stack = parser.GetParaphrases();
            var check = "";
            var isSelect = stack.Count == 1 && stack.Peek().Equals("select clause");
            if ((stack.Count > 0) && addPleaseCheck)
            {
                var delimiter = "";
                var checkList = new StringBuilder();
                checkList.Append(", please check the ");
                while(stack.Count != 0)
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
            if (keywords.Contains(token.ToLower()))
            {
                token += " (a reserved keyword)";
                reservedKeyword = true;
            }
            else if (tAfter != null && keywords.Contains("'" + tAfter.Text.ToLower() + "'"))
            {
                token += " ('" + tAfter.Text + "' is a reserved keyword)";
                reservedKeyword = true;
            }
            else
            {
                if ((tBefore != null) &&
                    (tAfter != null) &&
                    (keywords.Contains("'" + tBefore.Text.ToLower() + "'")) &&
                    (keywords.Contains("'" + tAfter.Text.ToLower() + "'")))
                {
                    token += " ('" + tBefore.Text + "' and '" + tAfter.Text + "' are a reserved keyword)";
                    reservedKeyword = true;
                }
                else if ((tBefore != null) &&
                         (keywords.Contains("'" + tBefore.Text.ToLower() + "'")))
                {
                    token += " ('" + tBefore.Text + "' is a reserved keyword)";
                    reservedKeyword = true;
                }
                else if (tEnd != null && keywords.Contains("'" + tEnd.Text.ToLower() + "'")) {
                    token += " ('" + tEnd.Text + "' is a reserved keyword)";
                    reservedKeyword = true;
                }
            }
    
            // special handling for the select-clause "as" keyword, which is required
            if (isSelect && !reservedKeyword) {
                check += GetSelectClauseAsText(tBeforeBefore, t);
            }
    
            message = "Incorrect syntax near " + token + positionInfo + check;
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
                else
                {
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
                               (currentIndex < tokenStream.Size - 1) &&
                               (currentIndex < tIndex + 3))
                        {
                            IToken next = tokenStream.Get(currentIndex);
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
                        if (i > 5) {
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
                else {
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
            if (!(e is InputMismatchException)) {
                return false;
            }

            var expectedTokens = e.GetExpectedTokens();
            if (expectedTokens.Count > 1) {
                return false;
            }
            return
                expectedTokens.Count == 1 &&
                expectedTokens.ToList()[0] == -1;
        }

        private static string GetTokenText(EsperEPL2GrammarParser parser, int tokenIndex) {
            var expected = END_OF_INPUT_TEXT;
            if ((tokenIndex >= 0) && (tokenIndex < parser.TokenNames.Length)) {
                expected = parser.TokenNames[tokenIndex];
            }
            var lexerTokenParaphrases = EsperEPL2GrammarLexer.GetLexerTokenParaphrases();
            if (lexerTokenParaphrases.Get(tokenIndex) != null) {
                expected = lexerTokenParaphrases.Get(tokenIndex);
            }
            var parserTokenParaphrases = EsperEPL2GrammarParser.GetParserTokenParaphrases();
            if (parserTokenParaphrases.Get(tokenIndex) != null) {
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
    
        private static string GetSelectClauseAsText(IToken tBeforeBefore, IToken t) {
            if (tBeforeBefore != null &&
                tBeforeBefore.Type == EsperEPL2GrammarParser.IDENT &&
                t != null &&
                t.Type == EsperEPL2GrammarParser.IDENT) {
                return " (did you forget 'as'?)";
            }
            return "";
        }
    }
}
