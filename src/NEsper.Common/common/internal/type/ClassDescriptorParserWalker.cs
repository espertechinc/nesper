///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
        private readonly ArrayDeque<ClassDescriptorToken> tokens;
        private ClassDescriptorToken lookahead;

        public ClassDescriptorParserWalker(ArrayDeque<ClassDescriptorToken> tokens)
        {
            if (tokens.IsEmpty()) {
                throw new ClassDescriptorParseException("Empty class identifier");
            }

            lookahead = tokens.First;
            this.tokens = tokens;
        }

        public ClassDescriptor Walk(bool typeParam)
        {
            if (lookahead.Token != ClassDescriptorTokenType.IDENTIFIER) {
                ExpectOrFail(ClassDescriptorTokenType.IDENTIFIER);
            }

            var name = lookahead.Sequence;
            var ident = new ClassDescriptor(name);

            NextToken();
            if (lookahead.Token == ClassDescriptorTokenType.LESSER_THAN) {
                NextToken();
                WalkTypeParams(ident);
            }

            if (lookahead.Token == ClassDescriptorTokenType.LEFT_BRACKET) {
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

        private void WalkArray(ClassDescriptor ident)
        {
            if (lookahead.Token == ClassDescriptorTokenType.IDENTIFIER) {
                var name = lookahead.Sequence;
                if (!name.ToLowerInvariant().Trim().Equals(ClassDescriptor.PRIMITIVE_KEYWORD)) {
                    throw new ClassDescriptorParseException(
                        "Invalid array keyword '" +
                        name +
                        "', expected ']' or '" +
                        ClassDescriptor.PRIMITIVE_KEYWORD +
                        "'");
                }

                ident.IsArrayOfPrimitive = true;
                NextToken();
            }

            while (true) {
                ExpectOrFail(ClassDescriptorTokenType.RIGHT_BRACKET);
                NextToken();
                ident.ArrayDimensions = ident.ArrayDimensions + 1;
                if (lookahead.Token != ClassDescriptorTokenType.LEFT_BRACKET) {
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
                if (lookahead.Token == ClassDescriptorTokenType.COMMA) {
                    NextToken();
                    ident = Walk(true);
                    parent.TypeParameters.Add(ident);
                    continue;
                }

                if (lookahead.Token == ClassDescriptorTokenType.GREATER_THAN) {
                    NextToken();
                    break;
                }

                ExpectOrFail(ClassDescriptorTokenType.GREATER_THAN);
            }
        }

        private void NextToken()
        {
            tokens.PopBack();
            if (tokens.IsEmpty()) {
                lookahead = new ClassDescriptorToken(ClassDescriptorTokenType.END, "");
            }
            else {
                lookahead = tokens.First;
            }
        }

        private void ExpectOrFail(ClassDescriptorTokenType expected)
        {
            if (lookahead.Token != expected) {
                throw new ClassDescriptorParseException(
                    "Unexpected token " +
                    lookahead.Token +
                    " value '" +
                    lookahead.Sequence +
                    "', expecting " +
                    expected);
            }
        }
    }
} // end of namespace