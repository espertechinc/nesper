///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    /// Parser similar in structure to:
    /// http://cogitolearning.co.uk/docs/cogpar/files.html
    /// </summary>
    public class ClassDescriptorParser
    {
        private static readonly ClassDescriptorTokenizer tokenizer;

        static ClassDescriptorParser()
        {
            tokenizer = new ClassDescriptorTokenizer();
            tokenizer.Add(
                "([a-zA-Z_$][a-zA-Z\\d_$]*\\.)*[a-zA-Z_$][a-zA-Z\\d_$]*",
                ClassDescriptorTokenType.IDENTIFIER);
            tokenizer.Add("\\[", ClassDescriptorTokenType.LEFT_BRACKET);
            tokenizer.Add("\\]", ClassDescriptorTokenType.RIGHT_BRACKET);
            tokenizer.Add("<", ClassDescriptorTokenType.LESSER_THAN);
            tokenizer.Add(",", ClassDescriptorTokenType.COMMA);
            tokenizer.Add(">", ClassDescriptorTokenType.GREATER_THAN);
        }

        internal static ClassDescriptor Parse(string classIdent)
        {
            try {
                var tokens = tokenizer.Tokenize(classIdent);
                var parser = new ClassDescriptorParserWalker(tokens);
                return parser.Walk(false);
            }
            catch (ClassDescriptorParseException ex) {
                throw new EPException("Failed to parse class identifier '" + classIdent + "': " + ex.Message, ex);
            }
        }
    }
} // end of namespace