///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    public class ClassDescriptor
    {
        public const string PRIMITIVE_KEYWORD = "primitive";

        public ClassDescriptor(string classIdentifier)
        {
            ClassIdentifier = classIdentifier;
            TypeParameters = Collections.GetEmptyList<ClassDescriptor>();
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

        public string ClassIdentifier { get; }

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
            writer.Write(ClassIdentifier);
            if (ArrayDimensions == 0 && TypeParameters.IsEmpty()) {
                return;
            }

            if (!TypeParameters.IsEmpty()) {
                writer.Write("<");
                var delimiter = "";
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

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (ClassDescriptor)o;

            if (ArrayDimensions != that.ArrayDimensions) {
                return false;
            }

            if (IsArrayOfPrimitive != that.IsArrayOfPrimitive) {
                return false;
            }

            if (!ClassIdentifier.Equals(that.ClassIdentifier)) {
                return false;
            }

            return CompatExtensions.DeepEquals(TypeParameters, that.TypeParameters);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(
                ClassIdentifier,
                TypeParameters,
                ArrayDimensions,
                IsArrayOfPrimitive);
        }

        public override string ToString()
        {
            return
                $"ClassIdentifierWArray{{classIdentifier='{ClassIdentifier}', typeParameters={TypeParameters}, arrayDimensions={ArrayDimensions}, arrayOfPrimitive={IsArrayOfPrimitive}}}";
        }
    }
} // end of namespace