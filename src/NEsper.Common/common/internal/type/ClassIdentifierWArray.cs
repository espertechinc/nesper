///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.type
{
    // Why do we need this? ... .NET types are fully qualified and include the dimensionality
    // and types of all the items we need...

    public class ClassIdentifierWArray
    {
        public const string PRIMITIVE_KEYWORD = "primitive";

        public ClassIdentifierWArray(string classIdentifier)
        {
            ClassIdentifier = classIdentifier;
            ArrayDimensions = 0;
            IsArrayOfPrimitive = false;
        }

        public ClassIdentifierWArray(
            string classIdentifier,
            int arrayDimensions,
            bool arrayOfPrimitive)
        {
            ClassIdentifier = classIdentifier;
            ArrayDimensions = arrayDimensions;
            IsArrayOfPrimitive = arrayOfPrimitive;
        }

        public string ClassIdentifier { get; }

        public int ArrayDimensions { get; }

        public bool IsArrayOfPrimitive { get; }

        public static ClassIdentifierWArray ParseSODA(string typeName)
        {
            var indexStart = typeName.IndexOf('[');
            if (indexStart == -1) {
                return new ClassIdentifierWArray(typeName);
            }

            var name = typeName.Substring(0, indexStart);
            var arrayPart = typeName.Substring(indexStart).ToLowerInvariant().Trim();
            arrayPart = arrayPart.Replace(" ", "");
            var primitive = "[" + PRIMITIVE_KEYWORD + "]";
            if (!arrayPart.StartsWith("[]") && !arrayPart.StartsWith(primitive)) {
                var testType = Type.GetType(typeName, false);
                if (testType != null) {
                    if (testType.IsArray) {
                        return new ClassIdentifierWArray(
                            testType.GetElementType().FullName,
                            testType.GetArrayRank(),
                            testType.GetElementType().IsValueType);
                    }

                    return new ClassIdentifierWArray(typeName); // Generics like Nullable show up here
                }

                throw new EPException(
                    "Invalid array keyword '" + arrayPart + "', expected ']' or '" + PRIMITIVE_KEYWORD + "'");
            }

            var arrayOfPrimitive = arrayPart.StartsWith(primitive);
            if (arrayPart.Equals("[]") || arrayPart.Equals(primitive)) {
                return new ClassIdentifierWArray(name, 1, arrayOfPrimitive);
            }

            var dimensions = arrayPart.Split(']').Length - 1;
            return new ClassIdentifierWArray(name, dimensions, arrayOfPrimitive);
        }

        public string ToEPL()
        {
            var writer = new StringWriter();
            ToEPL(writer);
            return writer.ToString();
        }

        public void ToEPL(TextWriter writer)
        {
            writer.Write(ClassIdentifier);
            if (ArrayDimensions == 0) {
                return;
            }

            writer.Write("[");
            if (IsArrayOfPrimitive) {
                writer.Write("primitive");
            }

            writer.Write("]");
            for (var i = 1; i < ArrayDimensions; i++) {
                writer.Write("[]");
            }
        }
    }
} // end of namespace