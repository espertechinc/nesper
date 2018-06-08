///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.util
{
    /// <summary>Factory for type widening.</summary>
    public class TypeWidenerFactory
    {
        public static readonly TypeWidener OBJECT_ARRAY_TO_COLLECTION_COERCER = TypeWidenerObjectArrayToCollectionCoercer.Widen;
        public static readonly TypeWidener STRING_TO_CHAR_COERCER = TypeWidenerStringToCharCoercer.Widen;
        public static readonly TypeWidener BYTE_ARRAY_TO_COLLECTION_COERCER = TypeWidenerByteArrayToCollectionCoercer;
        public static readonly TypeWidener SHORT_ARRAY_TO_COLLECTION_COERCER = TypeWidenerShortArrayToCollectionCoercer;
        public static readonly TypeWidener INT_ARRAY_TO_COLLECTION_COERCER = TypeWidenerIntArrayToCollectionCoercer;
        public static readonly TypeWidener LONG_ARRAY_TO_COLLECTION_COERCER = TypeWidenerLongArrayToCollectionCoercer;
        public static readonly TypeWidener FLOAT_ARRAY_TO_COLLECTION_COERCER = TypeWidenerFloatArrayToCollectionCoercer;
        public static readonly TypeWidener DOUBLE_ARRAY_TO_COLLECTION_COERCER = TypeWidenerDoubleArrayToCollectionCoercer;
        public static readonly TypeWidener BOOLEAN_ARRAY_TO_COLLECTION_COERCER = TypeWidenerBooleanArrayToCollectionCoercer;
        public static readonly TypeWidener CHAR_ARRAY_TO_COLLECTION_COERCER = TypeWidenerCharArrayToCollectionCoercer;

        /// <summary>Returns the widener. </summary>
        /// <param name="columnName">name of column</param>
        /// <param name="columnType">type of column</param>
        /// <param name="writeablePropertyType">property type</param>
        /// <param name="writeablePropertyName">propery name</param>
        /// <param name="allowObjectArrayToCollectionConversion">whether we widen object-array to collection</param>
        /// <param name="customizer">customization if any</param>
        /// <param name="engineURI">engine URI</param>
        /// <param name="statementName">statement name</param>
        /// <exception cref="ExprValidationException">if type validation fails</exception>
        /// <returns>type widender</returns>
        /// <throws>ExprValidationException if type validation fails</throws>
        public static TypeWidener GetCheckPropertyAssignType(
            String columnName,
            Type columnType,
            Type writeablePropertyType,
            String writeablePropertyName,
            bool allowObjectArrayToCollectionConversion,
            TypeWidenerCustomizer customizer,
            string statementName,
            string engineURI)
        {
            Type columnClassBoxed = TypeHelper.GetBoxedType(columnType);
            Type targetClassBoxed = TypeHelper.GetBoxedType(writeablePropertyType);

            if (customizer != null)
            {
                TypeWidener custom = customizer.WidenerFor(columnName, columnType, writeablePropertyType, writeablePropertyName, statementName, engineURI);
                if (custom != null)
                {
                    return custom;
                }
            }

            if (columnType == writeablePropertyType)
            {
                return null;
            }

            if (columnType == null)
            {
                if (writeablePropertyType.IsPrimitive)
                {
                    String message = "Invalid assignment of column '" + columnName +
                                     "' of null type to event property '" + writeablePropertyName +
                                     "' typed as '" + writeablePropertyType.GetCleanName() +
                                     "', nullable type mismatch";
                    throw new ExprValidationException(message);
                }
            }
            else if (columnClassBoxed != targetClassBoxed)
            {
                if (columnClassBoxed == typeof(string) && targetClassBoxed == typeof(char?))
                {
                    return TypeWidenerStringToCharCoercer.Widen;
                }

                if (allowObjectArrayToCollectionConversion
                    && columnClassBoxed.IsArray
                    && !columnClassBoxed.GetElementType().IsPrimitive
                    && targetClassBoxed.IsGenericCollection())
                {
                    return OBJECT_ARRAY_TO_COLLECTION_COERCER;
                }

                if (columnClassBoxed.IsGenericDictionary() && targetClassBoxed.IsGenericDictionary())
                {
                    var columnClassGenerics = columnClassBoxed.GetGenericArguments();
                    var targetClassGenerics = targetClassBoxed.GetGenericArguments();
                    var transformMethod = typeof(TransformDictionaryFactory)
                        .GetMethod("Create", new[] { typeof(object) })
                        .MakeGenericMethod(targetClassGenerics[0], targetClassGenerics[1], columnClassGenerics[0], columnClassGenerics[1]);

                    return source =>
                    {
                        var parameters = new object[] { source };
                        return transformMethod.Invoke(null, BindingFlags.Static | BindingFlags.Public, null, parameters, null);
                    };
                }

                if ((columnClassBoxed == typeof(string)) &&
                    (targetClassBoxed == typeof(char[])))
                {
                    return source =>
                    {
                        var sourceAsString = (string)source;
                        return sourceAsString != null ? sourceAsString.ToCharArray() : null;
                    };
                }

                if ((columnClassBoxed == typeof(char[])) &&
                    (targetClassBoxed == typeof(string)))
                {
                    return source =>
                    {
                        var sourceAsCharArray = (char[])source;
                        return sourceAsCharArray != null ? new string(sourceAsCharArray) : null;
                    };
                }

                if (columnClassBoxed.IsArray && targetClassBoxed.IsArray) {
                    var columnClassElement = columnClassBoxed.GetElementType();
                    var targetClassElement = targetClassBoxed.GetElementType();

                    if ((targetClassBoxed == typeof(object[])) && (columnClassElement.IsClass)) {
                        return null;
                    }

                    if (columnClassElement.GetBoxedType() == targetClassElement) {
                        return source => WidenArray(
                            source, targetClassElement,
                            CoercerFactory.GetCoercer(columnClassElement, targetClassElement));
                    } else if (columnClassElement == targetClassElement.GetBoxedType()) {
                        return source => WidenArray(
                            source, targetClassElement,
                            CoercerFactory.GetCoercer(columnClassElement, targetClassElement));
                    } else if (columnClassElement.IsAssignmentCompatible(targetClassElement)) {
                        // By definition, columnClassElement and targetClassElement should be
                        // incompatible.  Question is, can we find a coercer between them?
                        var coercer = CoercerFactory.GetCoercer(columnClassElement, targetClassElement);
                        if (coercer != null) {
                            return source => WidenArray(source, targetClassElement, coercer);
                        }
                    }
                }

                if (writeablePropertyType.IsNumeric() && columnType.IsAssignmentCompatible(writeablePropertyType))
                {
                    var instance = new TypeWidenerBoxedNumeric(
                        CoercerFactory.GetCoercer(columnClassBoxed, targetClassBoxed));
                    return instance.Widen;
                }

                if (!columnClassBoxed.IsAssignmentCompatible(targetClassBoxed))
                {
                    if (columnType.IsNullable() && writeablePropertyType.IsNullable()) {
                        // are the underlying nullable types compatible with one another?
                        var columnGenType = columnType.GetGenericArguments()[0];
                        var writeablePropertyGenType = writeablePropertyType.GetGenericArguments()[0];
                        if (writeablePropertyGenType.IsNumeric() &&
                            columnGenType.IsAssignmentCompatible(writeablePropertyGenType)) {
                            var instance = new TypeWidenerBoxedNumeric(
                                CoercerFactory.GetCoercer(columnClassBoxed, targetClassBoxed));
                            return instance.Widen;
                        }
                    } else if (!columnType.IsNullable() && writeablePropertyType.IsNullable()) {
                        // is the column type compatible with the nullable type?
                        var writeablePropertyGenType = writeablePropertyType.GetGenericArguments()[0];
                        if (writeablePropertyGenType.IsNumeric() &&
                            columnType.IsAssignmentCompatible(writeablePropertyGenType)) {
                            var instance = new TypeWidenerBoxedNumeric(
                                CoercerFactory.GetCoercer(columnClassBoxed, targetClassBoxed));
                            return instance.Widen;
                        }
                    } else if (columnType.IsNullable() && !writeablePropertyType.IsNullable()) {
                        // the target is not nullable, we can live with an exception if the
                        // value is null after widening.
                        var columnGenType = columnType.GetGenericArguments()[0];
                        if (writeablePropertyType.IsNumeric() &&
                            columnGenType.IsAssignmentCompatible(writeablePropertyType)) {
                            var instance = new TypeWidenerBoxedNumeric(
                                CoercerFactory.GetCoercer(columnClassBoxed, targetClassBoxed));
                            return instance.Widen;
                        }
                    }

                    var writablePropName = writeablePropertyType.GetCleanName();
                    var columnTypeName = columnType.GetCleanName();

                    String message = "Invalid assignment of column '" + columnName +
                                     "' of type '" + columnTypeName +
                                     "' to event property '" + writeablePropertyName +
                                     "' typed as '" + writablePropName +
                                     "', column and parameter types mismatch";
                    throw new ExprValidationException(message);
                }

                if (writeablePropertyType.IsNumeric())
                {
                    var instance = new TypeWidenerBoxedNumeric(
                        CoercerFactory.GetCoercer(columnClassBoxed, targetClassBoxed));
                    return instance.Widen;
                }
            }

            return null;
        }

        public static Object WidenArray(Object source, Type targetElementType, Coercer coercer)
        {
            var sourceArray = (Array)source;
            var length = sourceArray.Length;
            var targetArray = Array.CreateInstance(targetElementType, length);

            for (int ii = 0; ii < length; ii++)
            {
                targetArray.SetValue(coercer.Invoke(sourceArray.GetValue(ii)), ii);
            }

            return targetArray;
        }
    
        public static TypeWidener GetArrayToCollectionCoercer(Type componentType) {
            if (!componentType.IsPrimitive) {
                return OBJECT_ARRAY_TO_COLLECTION_COERCER;
            } else if (componentType == typeof(byte)) {
                return BYTE_ARRAY_TO_COLLECTION_COERCER;
            } else if (componentType == typeof(short)) {
                return SHORT_ARRAY_TO_COLLECTION_COERCER;
            } else if (componentType == typeof(int)) {
                return INT_ARRAY_TO_COLLECTION_COERCER;
            } else if (componentType == typeof(long)) {
                return LONG_ARRAY_TO_COLLECTION_COERCER;
            } else if (componentType == typeof(float)) {
                return FLOAT_ARRAY_TO_COLLECTION_COERCER;
            } else if (componentType == typeof(double)) {
                return DOUBLE_ARRAY_TO_COLLECTION_COERCER;
            } else if (componentType == typeof(bool)) {
                return BOOLEAN_ARRAY_TO_COLLECTION_COERCER;
            } else if (componentType == typeof(char)) {
                return CHAR_ARRAY_TO_COLLECTION_COERCER;
            }
            throw new IllegalStateException("Unrecognized class " + componentType);
        }

        internal static object TypeWidenerByteArrayToCollectionCoercer(object input)
        {
            return input.Unwrap<byte>();
        }

        internal static object TypeWidenerShortArrayToCollectionCoercer(object input) {
            return input.Unwrap<short>();
        }

        internal static object TypeWidenerIntArrayToCollectionCoercer(object input) {
            return input.Unwrap<int>();
        }

        internal static object TypeWidenerLongArrayToCollectionCoercer(object input) {
            return input.Unwrap<long>();
        }

        internal static object TypeWidenerFloatArrayToCollectionCoercer(object input) {
            return input.Unwrap<float>();
        }

        internal static object TypeWidenerDoubleArrayToCollectionCoercer(object input) {
            return input.Unwrap<double>();
        }

        internal static object TypeWidenerBooleanArrayToCollectionCoercer(object input) {
            return input.Unwrap<bool>();
        }

        internal static object TypeWidenerCharArrayToCollectionCoercer(object input) {
            return input.Unwrap<byte>();
        }
    }
} // end of namespace
