///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    // Why do we need this? ... .NET types are fully qualified and include the dimensionality
    // and types of all the items we need...

    public class ClassDescriptor
    {
        public const string PRIMITIVE_KEYWORD = "primitive";

        public ClassDescriptor(string classIdentifier)
        {
            ClassIdentifier = classIdentifier;
            TypeParameters = EmptyList<ClassDescriptor>.Instance;
            ArrayDimensions = 0;
            IsArrayOfPrimitive = false;
        }

        public ClassDescriptor(
            string classIdentifier,
            IList<ClassDescriptor> typeParameters,
            int arrayDimensions,
            bool arrayOfPrimitive)
        {
            ClassIdentifier = classIdentifier;
            TypeParameters = typeParameters;
            ArrayDimensions = arrayDimensions;
            IsArrayOfPrimitive = arrayOfPrimitive;
        }

        public ClassDescriptor(Type type)
        {
            if (type.IsArray) {
                var elementType = type.GetElementType();
                ClassIdentifier = elementType.AssemblyQualifiedName;
                TypeParameters = new ClassDescriptor[0];
                ArrayDimensions = type.GetArrayRank();
                IsArrayOfPrimitive = elementType.IsPrimitive;
            }
            else {
                ClassIdentifier = type.AssemblyQualifiedName;
                TypeParameters = new ClassDescriptor[0];
                ArrayDimensions = 0;
                IsArrayOfPrimitive = false;
            }
        }

        public string ClassIdentifier { get; }

        public string ClassIdentifierClr {
            get {
                if (TypeParameters == null) {
                    return ClassIdentifier;
                }

                if (TypeParameters.Count == 0) {
                    return ClassIdentifier;
                }

                return ClassIdentifier + "`" + TypeParameters.Count;
            }
        }
        
        public int ArrayDimensions { get; set; }

        public bool IsArrayOfPrimitive { get; set; }
        
        public IList<ClassDescriptor> TypeParameters { get; set; }

        public static ClassDescriptor ParseTypeText(string typeName)
        {
            return ClassDescriptorParser.Parse(typeName);
        }

        public string ToEPL()
        {
            var writer = new StringWriter();
            ToEPL(writer);
            return writer.ToString();
        }

        public void ToEPL(TextWriter writer)
        {
            var classIdentifier = ClassIdentifier;
            
            writer.Write(classIdentifier);

            if (!TypeParameters.IsEmpty()) {
                writer.Write("<");
                string delimiter = "";
                foreach (var typeParameter in TypeParameters) {
                    writer.Write(delimiter);
                    typeParameter.ToEPL(writer);
                    delimiter = ",";
                }
                writer.Write(">");
            }
            
            if (ArrayDimensions > 0) {
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
    }
} // end of namespace