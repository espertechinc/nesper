///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.util
{
    /// <summary>Factory for type widening. </summary>
    public class TypeWidenerFactory
    {
        private static readonly TypeWidenerStringToCharCoercer StringToCharCoercer =
            new TypeWidenerStringToCharCoercer();

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

        /// <summary>Returns the widener. </summary>
        /// <param name="columnName">name of column</param>
        /// <param name="columnType">type of column</param>
        /// <param name="writeablePropertyType">property type</param>
        /// <param name="writeablePropertyName">propery name</param>
        /// <returns>type widender</returns>
        /// <throws>ExprValidationException if type validation fails</throws>
        public static TypeWidener GetCheckPropertyAssignType(String columnName,
                                                             Type columnType,
                                                             Type writeablePropertyType,
                                                             String writeablePropertyName)
        {
            Type columnClassBoxed = columnType.GetBoxedType();
            Type targetClassBoxed = writeablePropertyType.GetBoxedType();

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
    }
}