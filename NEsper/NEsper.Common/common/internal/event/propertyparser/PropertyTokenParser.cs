///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.propertyparser
{
    public class PropertyTokenParser
    {
        private readonly ArrayDeque<Token> tokens;
        private bool dynamic;
        private Token lookahead;

        public PropertyTokenParser(
            ArrayDeque<Token> tokens,
            bool rootedDynamic)
        {
            if (tokens.IsEmpty()) {
                throw new PropertyParseNodepException("Empty property name");
            }

            lookahead = tokens.First;
            this.tokens = tokens;
            dynamic = rootedDynamic;
        }

        public Property Property()
        {
            var first = EventPropertyAtomic();

            if (lookahead.token == TokenType.END) {
                return first;
            }

            IList<Property> props = new List<Property>(4);
            props.Add(first);

            while (lookahead.token == TokenType.DOT) {
                NextToken();

                var next = EventPropertyAtomic();
                props.Add(next);
            }

            return new NestedProperty(props);
        }

        private Property EventPropertyAtomic()
        {
            if (lookahead.token != TokenType.IDENT && lookahead.token != TokenType.IDENTESCAPED) {
                ExpectOrFail(TokenType.IDENT);
            }

            string ident;
            if (lookahead.token == TokenType.IDENT) {
                ident = ProcessIdent(lookahead.sequence);
            }
            else {
                ident = ProcessIdent(PropertyParser.UnescapeBacktickForProperty(lookahead.sequence));
            }

            NextToken();

            if (lookahead.token == TokenType.LBRACK) {
                NextToken();
                ExpectOrFail(TokenType.LBRACK, TokenType.NUMBER);

                var index = int.Parse(lookahead.sequence);
                NextToken();

                ExpectOrFail(TokenType.NUMBER, TokenType.RBRACK);
                NextToken();

                if (lookahead.token == TokenType.QUESTION) {
                    NextToken();
                    return new DynamicIndexedProperty(ident, index);
                }

                if (dynamic) {
                    return new DynamicIndexedProperty(ident, index);
                }

                return new IndexedProperty(ident, index);
            }

            if (lookahead.token == TokenType.LPAREN) {
                NextToken();

                if (lookahead.token == TokenType.DOUBLEQUOTEDLITERAL ||
                    lookahead.token == TokenType.SINGLEQUOTEDLITERAL) {
                    var type = lookahead.token;
                    var value = lookahead.sequence.Trim();
                    var key = value.Substring(1, value.Length - 1);
                    NextToken();

                    ExpectOrFail(type, TokenType.RPAREN);
                    NextToken();

                    if (lookahead.token == TokenType.QUESTION) {
                        NextToken();
                        return new DynamicMappedProperty(ident, key);
                    }

                    if (dynamic) {
                        return new DynamicMappedProperty(ident, key);
                    }

                    return new MappedProperty(ident, key);
                }

                ExpectOrFail(TokenType.LPAREN, TokenType.DOUBLEQUOTEDLITERAL);
            }

            if (lookahead.token == TokenType.QUESTION) {
                NextToken();
                dynamic = true;
                return new DynamicSimpleProperty(ident);
            }

            if (dynamic) {
                return new DynamicSimpleProperty(ident);
            }

            return new SimpleProperty(ident);
        }

        private string ProcessIdent(string ident)
        {
            if (!ident.Contains(".")) {
                return ident;
            }

            return ident.RegexReplaceAll("\\\\.", ".");
        }

        private void ExpectOrFail(
            TokenType before,
            TokenType expected)
        {
            if (lookahead.token != expected) {
                throw new PropertyParseNodepException(
                    "Unexpected token " +
                    lookahead.token +
                    " value '" +
                    lookahead.sequence +
                    "', expecting " +
                    expected +
                    " after " +
                    before);
            }
        }

        private void ExpectOrFail(TokenType expected)
        {
            if (lookahead.token != expected) {
                throw new PropertyParseNodepException(
                    "Unexpected token " +
                    lookahead.token +
                    " value '" +
                    lookahead.sequence +
                    "', expecting " +
                    expected);
            }
        }

        private void NextToken()
        {
            tokens.RemoveFirst(); // Pop();
            if (tokens.IsEmpty()) {
                lookahead = new Token(TokenType.END, "");
            }
            else {
                lookahead = tokens.First;
            }
        }
    }
} // end of namespace