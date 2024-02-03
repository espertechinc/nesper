///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    public class ClassDescriptorParserWalker
    {
        private readonly ArrayDeque<ClassDescriptorToken> _tokens;
        private ClassDescriptorToken _lookahead;

        public ClassDescriptorParserWalker(ArrayDeque<ClassDescriptorToken> tokens)
        {
            if (tokens.IsEmpty()) {
                throw new ClassDescriptorParseException("Empty class identifier");
            }

            _lookahead = tokens.First;
            _tokens = tokens;
        }

        public ClassDescriptor Walk(bool typeParam)
        {
            var name = WalkIdentifier();

            if (_lookahead.Token == ClassDescriptorTokenType.PLUS) {
                NextToken();
                name += "+";
                name += WalkIdentifier();
            }

            var ident = new ClassDescriptor(name);

            if (_lookahead.Token == ClassDescriptorTokenType.LESSER_THAN) {
                NextToken();
                WalkTypeParams(ident);
            }

            if (_lookahead.Token == ClassDescriptorTokenType.LEFT_BRACKET) {
                NextToken();
                WalkArray(ident);
            }

            if (!typeParam) {
                ExpectOrFail(ClassDescriptorTokenType.END);
                return ident;
            }
            else {
                return ident;
            }
        }

        private string WalkIdentifier()
        {
            if (_lookahead.Token != ClassDescriptorTokenType.IDENTIFIER) {
                ExpectOrFail(ClassDescriptorTokenType.IDENTIFIER);
            }

            var result = _lookahead.Sequence;
            NextToken();
            return result;
        }
        
        private void WalkArray(ClassDescriptor ident)
        {
            if (_lookahead.Token == ClassDescriptorTokenType.IDENTIFIER) {
                var name = _lookahead.Sequence;
                if (!name.ToLowerInvariant().Trim().Equals(ClassDescriptor.PRIMITIVE_KEYWORD)) {
                    throw new ClassDescriptorParseException(
                        $"Invalid array keyword '{name}', expected ']' or '{ClassDescriptor.PRIMITIVE_KEYWORD}'");
                }

                ident.IsArrayOfPrimitive = true;
                NextToken();
            }

            while (true) {
                ExpectOrFail(ClassDescriptorTokenType.RIGHT_BRACKET);
                NextToken();
                ident.ArrayDimensions = ident.ArrayDimensions + 1;
                if (_lookahead.Token != ClassDescriptorTokenType.LEFT_BRACKET) {
                    break;
                }
                else {
                    NextToken();
                }
            }
        }

        private void WalkTypeParams(ClassDescriptor parent)
        {
            var ident = Walk(true);
            if (parent.TypeParameters.IsEmpty()) {
                parent.TypeParameters = new List<ClassDescriptor>(2);
            }

            parent.TypeParameters.Add(ident);
            while (true) {
                if (_lookahead.Token == ClassDescriptorTokenType.COMMA) {
                    NextToken();
                    ident = Walk(true);
                    parent.TypeParameters.Add(ident);
                    continue;
                }

                if (_lookahead.Token == ClassDescriptorTokenType.GREATER_THAN) {
                    NextToken();
                    break;
                }

                ExpectOrFail(ClassDescriptorTokenType.GREATER_THAN);
            }
        }

        private void NextToken()
        {
            _tokens.PopFront();
            if (_tokens.IsEmpty()) {
                _lookahead = new ClassDescriptorToken(ClassDescriptorTokenType.END, "");
            }
            else {
                _lookahead = _tokens.First;
            }
        }

        private void ExpectOrFail(ClassDescriptorTokenType expected)
        {
            if (_lookahead.Token != expected) {
                throw new ClassDescriptorParseException(
                    $"Unexpected token {_lookahead.Token} value '{_lookahead.Sequence}', expecting {expected}");
            }
        }
    }
} // end of namespace