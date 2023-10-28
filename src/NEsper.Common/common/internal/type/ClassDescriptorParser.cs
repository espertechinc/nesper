///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
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
            tokenizer.Add("([a-zA-Z_][a-zA-Z\\d_]*\\.)*[a-zA-Z_][a-zA-Z\\d_]*", ClassDescriptorTokenType.IDENTIFIER);
            tokenizer.Add("\\+", ClassDescriptorTokenType.PLUS);
            tokenizer.Add("\\[", ClassDescriptorTokenType.LEFT_BRACKET);
            tokenizer.Add("\\]", ClassDescriptorTokenType.RIGHT_BRACKET);
            tokenizer.Add("<", ClassDescriptorTokenType.LESSER_THAN);
            tokenizer.Add(",", ClassDescriptorTokenType.COMMA);
            tokenizer.Add(">", ClassDescriptorTokenType.GREATER_THAN);
        }

        internal static ClassDescriptor Parse(string classIdent)
        {
            classIdent = classIdent.UnmaskTypeName();
            
            try {
                var tokens = tokenizer.Tokenize(classIdent);
                var parser = new ClassDescriptorParserWalker(tokens);
                return parser.Walk(false);
            }
            catch (ClassDescriptorParseException ex) {
                // if we cannot tokenize it, then maybe we can just instantiate it and decompose it
                if (TryDecompose(classIdent, out var classDescriptor)) {
                    return classDescriptor;
                }
                
                throw new EPException($"Failed to parse class identifier '{classIdent}': {ex.Message}", ex);
            }
        }

        internal static bool TryDecompose(string classIdent, out ClassDescriptor classDescriptor)
        {
            var type = Type.GetType(classIdent, false, false);
            if (type != null) {
                classDescriptor = Decompose(type);
                return true;
            }

            classDescriptor = default;
            return false;
        }

        internal static ClassDescriptor Decompose(Type type)
        {
            if (type.IsArray) {
                var elementType = type.GetElementType();
                var elementDesc = Decompose(elementType);
                var arrayRank = type.GetArrayRank();
                return new ClassDescriptor(
                    elementDesc.ClassIdentifier,
                    elementDesc.TypeParameters,
                    arrayRank,
                    elementDesc.IsArrayOfPrimitive);
            }

            if (type.IsGenericType) {
                var genericArguments = type.GetGenericArguments();
                var genericDescriptors = genericArguments
                    .Select(Decompose)
                    .ToList();
                // get the basic generic type name
                return new ClassDescriptor(
                    type.Namespace + "." + type.Name,
                    genericDescriptors,
                    0,
                    false);
            }
            
            return new ClassDescriptor(
                type.Namespace + "." + type.Name,
                EmptyList<ClassDescriptor>.Instance,
                0,
                type.IsPrimitive);
        }
    }
} // end of namespace