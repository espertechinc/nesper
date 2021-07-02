using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.settings
{
    public class ImportTypeUtil
    {
        public static Type ResolveClassIdentifierToType(
            ClassDescriptor classIdent,
            bool allowObjectType,
            ImportService importService,
            ExtensionClass extension)
        {
            var typeName = classIdent.ClassIdentifierClr;

            if (classIdent.IsArrayOfPrimitive) {
                var primitive = TypeHelper.GetPrimitiveTypeForName(typeName);
                if (primitive != null) {
                    return TypeHelper.GetArrayType(primitive, classIdent.ArrayDimensions);
                }

                throw new ExprValidationException("Type '" + typeName + "' is not a primitive type");
            }

            var plain = TypeHelper.GetTypeForSimpleName(typeName, importService.ClassForNameProvider, true);
            if (plain != null) {
                return ParameterizeType(plain, classIdent.TypeParameters, classIdent.ArrayDimensions, importService, extension);
            }

            if (allowObjectType && String.Equals(typeName, "object", StringComparison.OrdinalIgnoreCase)) {
                return typeof(object);
            }

            // try imports first
            Type resolved = null;
            try {
                resolved = importService.ResolveClass(typeName, false, extension);
            }
            catch (ImportException) {
                // expected
            }

            if (string.Equals(typeName, "biginteger", StringComparison.OrdinalIgnoreCase)) {
                return TypeHelper.GetArrayType(typeof(BigInteger), classIdent.ArrayDimensions);
            }

            if (string.Equals(typeName, "decimal", StringComparison.OrdinalIgnoreCase)) {
                return TypeHelper.GetArrayType(typeof(decimal), classIdent.ArrayDimensions);
            }

            // resolve from  when not found
            if (resolved == null) {
                try {
                    resolved = TypeHelper.GetClassForName(typeName, importService.ClassForNameProvider);
                }
                catch (TypeLoadException) {
                    // expected
                }
            }

            // Handle resolved classes here
            if (resolved != null) {
                return ParameterizeType(resolved, classIdent.TypeParameters, classIdent.ArrayDimensions, importService, extension);
            }

            return null;
        }

        public static Type ParameterizeType(
            bool allowArrayDimensions,
            Type clazz,
            ClassDescriptor descriptor,
            ImportService importService,
            ClassProvidedExtension extension)
        {
            if (descriptor.ArrayDimensions != 0 && !allowArrayDimensions) {
                throw new ExprValidationException("Array dimensions are not allowed");
            }

            var elementType = clazz;
            if (!descriptor.TypeParameters.IsEmpty()) {
                var variables = clazz.GetGenericArguments();
                if (variables.Length != descriptor.TypeParameters.Count) {
                    throw new ExprValidationException(
                        "Number of type parameters mismatch, the class '" +
                        clazz.TypeSafeName() +
                        "' has " +
                        variables.Length +
                        " type parameters but specified are " +
                        descriptor.TypeParameters.Count +
                        " type parameters");
                }

                var parameters = new Type[variables.Length];
                for (var i = 0; i < descriptor.TypeParameters.Count; i++) {
                    var desc = descriptor.TypeParameters[i];
                
                    Type inner;
                    try {
                        inner = importService.ResolveClass(desc.ClassIdentifier, false, extension);
                    }
                    catch (ImportException e) {
                        throw new ExprValidationException("Failed to resolve type parameter " + i + " of type '" + desc.ToEPL() + "': " + e.Message, e);
                    }

                    var variable = variables[i];
                    if (!variable.IsGenericParameter) {
                        if (!TypeHelper.IsSubclassOrImplementsInterface(inner, variable)) {
                            throw new ExprValidationException(
                                "Bound type parameters " +
                                i +
                                " expects '" +
                                variable.TypeSafeName() +
                                "' but receives '" +
                                inner.TypeSafeName() +
                                "'");
                        }
                    }

                    var parameterized = ParameterizeType(true, inner, desc, importService, extension);
                    parameters[i] = parameterized;
                }

                elementType = clazz.MakeGenericType(parameters);
            }

            if (descriptor.ArrayDimensions > 0) {
                return TypeHelper.GetArrayType(elementType, descriptor.ArrayDimensions);
            }

            return elementType;
        }

        public static Type ParameterizeType(
            Type plain,
            IList<ClassDescriptor> typeParameters,
            int arrayDimensions,
            ImportService importService,
            ExtensionClass extension)
        {
            if (typeParameters.IsEmpty()) {
                return TypeHelper.GetArrayType(plain, arrayDimensions);
            }

            var types = new List<Type>(typeParameters.Count);
            for (var i = 0; i < typeParameters.Count; i++) {
                var typeParam = typeParameters[i];
                var type = ResolveClassIdentifierToType(typeParam, false, importService, extension);
                if (type == null) {
                    throw new ExprValidationException("Failed to resolve type parameter '" + typeParam.ToEPL() + "'");
                }

                types.Add(type);
            }

            var generic = plain.MakeGenericType(types.ToArray());
            if (arrayDimensions == 0) {
                return generic;
            }

            var genericArray = TypeHelper.GetArrayType(generic);
            return genericArray;

            // plain = TypeHelper.GetArrayType(plain, arrayDimensions);
            // return new EPTypeClassParameterized(plain, types.toArray(new EPTypeClass[0]));
        }
    }
}