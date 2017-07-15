///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

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

            if (columnType == null)
            {
                if (writeablePropertyType.IsPrimitive)
                {
                    String message = "Invalid assignment of column '" + columnName +
                                     "' of null type to event property '" + writeablePropertyName +
                                     "' typed as '" + writeablePropertyType.FullName +
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

                if (columnClassBoxed.IsArray && targetClassBoxed.IsArray)
                {
                    var columnClassElement = columnClassBoxed.GetElementType();
                    var targetClassElement = targetClassBoxed.GetElementType();
                    // By definition, columnClassElement and targetClassElement should be
                    // incompatible.  Question is, can we find a coercer between them?
                    var coercer = CoercerFactory.GetCoercer(columnClassElement, targetClassElement);
                    return source => WidenArray(source, targetClassElement, coercer);
                }

                if (!columnClassBoxed.IsAssignmentCompatible(targetClassBoxed))
                {
                    var writablePropName = writeablePropertyType.FullName;
                    if (writeablePropertyType.IsArray)
                    {
                        writablePropName = writeablePropertyType.GetElementType().FullName + "[]";
                    }

                    var columnTypeName = columnType.FullName;
                    if (columnType.IsArray)
                    {
                        columnTypeName = columnType.GetElementType().FullName + "[]";
                    }

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
