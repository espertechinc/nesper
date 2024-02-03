///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.property;

namespace com.espertech.esper.common.@internal.@event.propertyparser
{
    /// <summary>
    ///     Parser similar in structure to:
    ///     http://cogitolearning.co.uk/docs/cogpar/files.html
    /// </summary>
    public class PropertyParserNoDep
    {
        private static readonly Tokenizer tokenizer;

        static PropertyParserNoDep()
        {
            tokenizer = new Tokenizer();
            tokenizer.Add("[a-zA-Z]([a-zA-Z0-9_]|\\\\.)*", TokenType.IDENT);
            tokenizer.Add("`[^`]*`", TokenType.IDENTESCAPED);
            tokenizer.Add("[0-9]+", TokenType.NUMBER);
            tokenizer.Add("\\[", TokenType.LBRACK);
            tokenizer.Add("\\]", TokenType.RBRACK);
            tokenizer.Add("\\(", TokenType.LPAREN);
            tokenizer.Add("\\)", TokenType.RPAREN);
            tokenizer.Add("\"([^\\\\\"]|\\\\\\\\|\\\\\")*\"", TokenType.DOUBLEQUOTEDLITERAL);
            tokenizer.Add("\'([^\\']|\\\\\\\\|\\')*\'", TokenType.SINGLEQUOTEDLITERAL);
            tokenizer.Add("\\.", TokenType.DOT);
            tokenizer.Add("\\?", TokenType.QUESTION);
        }

        public static Property ParseAndWalkLaxToSimple(
            string expression,
            bool rootedDynamic)
        {
            try {
                var tokens = tokenizer.Tokenize(expression);
                var parser = new PropertyTokenParser(tokens, rootedDynamic);
                return parser.Property();
            }
            catch (PropertyParseNodepException ex) {
                throw new PropertyAccessException("Failed to parse property '" + expression + "': " + ex.Message, ex);
            }
        }

        /// <summary>
        ///     Parse the mapped property into classname, method and string argument.
        ///     Mind this has been parsed already and is a valid mapped property.
        /// </summary>
        /// <param name="property">is the string property to be passed as a static method invocation</param>
        /// <returns>descriptor object</returns>
        public static MappedPropertyParseResult ParseMappedProperty(string property)
        {
            // split the class and method from the parentheses and argument
            var indexOpenParen = property.IndexOf('(');
            if (indexOpenParen == -1) {
                return null;
            }

            var classAndMethod = property.Substring(0, indexOpenParen);
            var parensAndArg = property.Substring(indexOpenParen);
            if (classAndMethod.Length == 0 || parensAndArg.Length == 0) {
                return null;
            }

            // find the first quote
            int startArg;
            var indexFirstDoubleQuote = parensAndArg.IndexOf('\"');
            var indexFirstSingleQuote = parensAndArg.IndexOf('\'');
            if (indexFirstSingleQuote != -1 && indexFirstDoubleQuote != -1) {
                startArg = Math.Min(indexFirstDoubleQuote, indexFirstSingleQuote);
            }
            else if (indexFirstSingleQuote != -1) {
                startArg = indexFirstSingleQuote;
            }
            else if (indexFirstDoubleQuote != -1) {
                startArg = indexFirstDoubleQuote;
            }
            else {
                return null;
            }

            // find the last quote
            int endArg;
            var indexLastDoubleQuote = parensAndArg.LastIndexOf('\"');
            var indexLastSingleQuote = parensAndArg.LastIndexOf('\'');
            if (indexLastSingleQuote != -1 && indexLastDoubleQuote != -1) {
                endArg = Math.Max(indexLastDoubleQuote, indexLastSingleQuote);
            }
            else if (indexLastSingleQuote != -1) {
                endArg = indexLastSingleQuote;
            }
            else if (indexLastDoubleQuote != -1) {
                endArg = indexLastDoubleQuote;
            }
            else {
                return null;
            }

            if (startArg == endArg) {
                return null;
            }

            var argument = parensAndArg.Substring(startArg + 1, endArg - startArg - 1);
            // split the class from the method
            var indexLastDot = classAndMethod.LastIndexOf('.');
            if (indexLastDot == -1) {
                // no class name
                return new MappedPropertyParseResult(null, classAndMethod, argument);
            }

            var method = classAndMethod.Substring(indexLastDot + 1);
            if (method.Length == 0) {
                return null;
            }

            var clazz = classAndMethod.Substring(0, indexLastDot);
            return new MappedPropertyParseResult(clazz, method, argument);
        }
    }
} // end of namespace