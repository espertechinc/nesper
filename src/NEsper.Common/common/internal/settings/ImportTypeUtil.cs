///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

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
            var typeName = classIdent.ClassIdentifier;

            if (classIdent.IsArrayOfPrimitive) {
                var primitive = TypeHelper.GetPrimitiveTypeForName(typeName);
                if (primitive != null) {
                    return TypeHelper.GetArrayType(primitive, classIdent.ArrayDimensions);
                }

                throw new ExprValidationException("Type '" + typeName + "' is not a primitive type");
            }

            var plain = TypeHelper.GetTypeForSimpleName(typeName, importService.TypeResolver);
            if (plain != null) {
                return ParameterizeType(
                    plain,
                    classIdent.TypeParameters,
                    classIdent.ArrayDimensions,
                    importService,
                    extension);
            }

            if (allowObjectType && typeName.Equals("object", StringComparison.InvariantCultureIgnoreCase)) {
                return typeof(object);
            }

            // try imports first
            Type resolved = null;
            try {
                resolved = importService.ResolveType(typeName, false, extension);
            }
            catch (ImportException) {
                // expected
            }
            catch (TypeLoadException) {
                // expected
            }

            var lowercase = typeName;
            if (lowercase.Equals("biginteger", StringComparison.InvariantCultureIgnoreCase)) {
                return TypeHelper.GetArrayType(
                    typeof(BigInteger),
                    classIdent.ArrayDimensions);
            }

            if (lowercase.Equals("decimal", StringComparison.InvariantCultureIgnoreCase)) {
                return TypeHelper.GetArrayType(
                    typeof(decimal),
                    classIdent.ArrayDimensions);
            }

            // resolve from classpath when not found
            if (resolved == null) {
                try {
                    resolved = TypeHelper.GetTypeForName(typeName, importService.TypeResolver);
                }
                catch (TypeLoadException) {
                    // expected
                }
            }

            // Handle resolved classes here
            if (resolved != null) {
                return ParameterizeType(
                    resolved,
                    classIdent.TypeParameters,
                    classIdent.ArrayDimensions,
                    importService,
                    extension);
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

            var classArrayed = clazz;
            if (descriptor.ArrayDimensions > 0) {
                classArrayed = TypeHelper.GetArrayType(clazz, descriptor.ArrayDimensions);
            }

            if (descriptor.TypeParameters.IsEmpty()) {
                return classArrayed;
            }

            var variables = clazz.GetGenericArguments();
            if (variables.Length != descriptor.TypeParameters.Count) {
                throw new ExprValidationException(
                    "Number of type parameters mismatch, the class '" +
                    clazz.CleanName() +
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
                    inner = importService.ResolveType(desc.ClassIdentifier, false, extension);
                }
                catch (ImportException e) {
                    throw new ExprValidationException(
                        "Failed to resolve type parameter " + i + " of type '" + desc.ToEPL() + "': " + e.Message,
                        e);
                }

                var parameterized = ParameterizeType(
                    true,
                    inner,
                    desc,
                    importService,
                    extension);
                parameters[i] = parameterized;
            }

            return classArrayed.MakeGenericType(parameters);
        }

        public static Type ParameterizeType(
            Type plain,
            IList<ClassDescriptor> typeParameters,
            int arrayDimensions,
            ImportService importService,
            ExtensionClass extension)
        {
            if (typeParameters.Count == 0) {
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

            plain = TypeHelper.GetArrayType(plain, arrayDimensions);

            return plain.MakeGenericType(types.ToArray());
        }
    }
} // end of namespace