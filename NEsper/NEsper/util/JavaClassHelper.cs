///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.util;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.event.avro;
using com.espertech.esper.events;
using com.espertech.esper.type;

using java.math;
using java.time;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Helper for questions about Java classes such as
    /// <para>
    ///  what is the boxed type for a primitive type
    /// </para>
    /// <para>
    ///  is this a numeric type.
    /// </para>
    /// </summary>
    public class JavaClassHelper {
        public static bool IsArrayTypeCompatible(Type target, Type provided) {
            if (target == provided || target == Typeof(Object)) {
                return true;
            }
            Type targetBoxed = GetBoxedType(target);
            Type providedBoxed = GetBoxedType(provided);
            return targetBoxed == providedBoxed || IsSubclassOrImplementsInterface(providedBoxed, targetBoxed);
        }
    
        public static bool IsCollectionMapOrArray(Type returnType) {
            return returnType != null && (IsImplementsInterface(returnType, Typeof(Collection)) || IsImplementsInterface(returnType, Typeof(Map)) || returnType.IsArray);
        }
    
        /// <summary>
        /// Returns the boxed class for the given class, or the class itself if already boxed or not a primitive type.
        /// For primitive unboxed types returns the boxed types, e.g. returns java.lang.int? for passing Typeof(int).
        /// For any other class, returns the class passed.
        /// </summary>
        /// <param name="clazz">is the class to return the boxed class for</param>
        /// <returns>boxed variant of the same class</returns>
        public static Type GetBoxedType(Type clazz) {
            if (clazz == null) {
                return null;
            }
            if (!clazz.IsPrimitive) {
                return clazz;
            }
            if (clazz == Typeof(bool)) {
                return Typeof(bool?);
            }
            if (clazz == Typeof(char)) {
                return Typeof(Character);
            }
            if (clazz == Typeof(double)) {
                return Typeof(double?);
            }
            if (clazz == Typeof(float)) {
                return Typeof(Float);
            }
            if (clazz == Typeof(byte)) {
                return Typeof(Byte);
            }
            if (clazz == Typeof(short)) {
                return Typeof(Short);
            }
            if (clazz == Typeof(int)) {
                return Typeof(int?);
            }
            if (clazz == Typeof(long)) {
                return Typeof(long);
            }
            return clazz;
        }
    
        /// <summary>
        /// Returns a comma-separated parameter type list in readable form,
        /// considering arrays and null-type parameters.
        /// </summary>
        /// <param name="parameters">is the parameter types to render</param>
        /// <returns>rendered list of parameters</returns>
        public static string GetParameterAsString(Type[] parameters) {
            var builder = new StringBuilder();
            string delimiterComma = ", ";
            string delimiter = "";
            foreach (Type param in parameters) {
                builder.Append(delimiter);
                builder.Append(GetParameterAsString(param));
                delimiter = delimiterComma;
            }
            return Builder.ToString();
        }
    
        /// <summary>
        /// Returns a parameter as a string text, allowing null values to represent a null
        /// select expression type.
        /// </summary>
        /// <param name="param">is the parameter type</param>
        /// <returns>string representation of parameter</returns>
        public static string GetParameterAsString(Type param) {
            if (param == null) {
                return "null (any type)";
            }
            return Param.SimpleName;
        }
    
        /// <summary>
        /// Returns the un-boxed class for the given class, or the class itself if already un-boxed or not a primitive type.
        /// For primitive boxed types returns the unboxed primitive type, e.g. returns Typeof(int) for passing Typeof(int?).
        /// For any other class, returns the class passed.
        /// </summary>
        /// <param name="clazz">is the class to return the unboxed (or primitive) class for</param>
        /// <returns>primitive variant of the same class</returns>
        public static Type GetPrimitiveType(Type clazz) {
            if (clazz == Typeof(bool?)) {
                return Typeof(bool);
            }
            if (clazz == Typeof(Character)) {
                return Typeof(char);
            }
            if (clazz == Typeof(double?)) {
                return Typeof(double);
            }
            if (clazz == Typeof(Float)) {
                return Typeof(float);
            }
            if (clazz == Typeof(Byte)) {
                return Typeof(byte);
            }
            if (clazz == Typeof(Short)) {
                return Typeof(short);
            }
            if (clazz == Typeof(int?)) {
                return Typeof(int);
            }
            if (clazz == Typeof(long)) {
                return Typeof(long);
            }
            return clazz;
        }
    
        /// <summary>
        /// Determines if the class passed in is one of the numeric classes.
        /// </summary>
        /// <param name="clazz">to check</param>
        /// <returns>true if numeric, false if not</returns>
        public static bool IsNumeric(Type clazz) {
            if ((clazz == Typeof(double?)) ||
                    (clazz == Typeof(double)) ||
                    (clazz == Typeof(BigDecimal)) ||
                    (clazz == Typeof(BigInteger)) ||
                    (clazz == Typeof(Float)) ||
                    (clazz == Typeof(float)) ||
                    (clazz == Typeof(Short)) ||
                    (clazz == Typeof(short)) ||
                    (clazz == Typeof(int?)) ||
                    (clazz == Typeof(int)) ||
                    (clazz == Typeof(long)) ||
                    (clazz == Typeof(long)) ||
                    (clazz == Typeof(Byte)) ||
                    (clazz == Typeof(byte))) {
                return true;
            }
    
            return false;
        }
    
        /// <summary>
        /// Determines if the class passed in is one of the numeric classes and not a floating point.
        /// </summary>
        /// <param name="clazz">to check</param>
        /// <returns>true if numeric and not a floating point, false if not</returns>
        public static bool IsNumericNonFP(Type clazz) {
            if ((clazz == Typeof(Short)) ||
                    (clazz == Typeof(short)) ||
                    (clazz == Typeof(int?)) ||
                    (clazz == Typeof(int)) ||
                    (clazz == Typeof(long)) ||
                    (clazz == Typeof(long)) ||
                    (clazz == Typeof(Byte)) ||
                    (clazz == Typeof(byte))) {
                return true;
            }
    
            return false;
        }
    
        /// <summary>
        /// Returns true if 2 classes are assignment compatible.
        /// </summary>
        /// <param name="invocationType">type to assign from</param>
        /// <param name="declarationType">type to assign to</param>
        /// <returns>true if assignment compatible, false if not</returns>
        public static bool IsAssignmentCompatible(Type invocationType, Type declarationType) {
            if (invocationType == null) {
                return true;
            }
            if (declarationType.IsAssignableFrom(invocationType)) {
                return true;
            }
    
            if (declarationType.IsPrimitive) {
                Type parameterWrapperClazz = GetBoxedType(declarationType);
                if (parameterWrapperClazz != null) {
                    if (parameterWrapperClazz.Equals(invocationType)) {
                        return true;
                    }
                }
            }
    
            if (GetBoxedType(invocationType) == declarationType) {
                return true;
            }
    
            Set<Type> widenings = MethodResolver.WideningConversions.Get(declarationType);
            if (widenings != null) {
                return Widenings.Contains(invocationType);
            }
    
            if (declarationType.IsInterface) {
                if (IsImplementsInterface(invocationType, declarationType)) {
                    return true;
                }
            }
    
            return RecursiveIsSuperClass(invocationType, declarationType);
        }
    
        /// <summary>
        /// Determines if the class passed in is a bool boxed or unboxed type.
        /// </summary>
        /// <param name="clazz">to check</param>
        /// <returns>true if bool, false if not</returns>
        public static bool IsBoolean(Type clazz) {
            if ((clazz == Typeof(bool?)) ||
                    (clazz == Typeof(bool))) {
                return true;
            }
            return false;
        }
    
        /// <summary>
        /// Returns the coercion type for the 2 numeric types for use in arithmatic.
        /// Note: byte and short types always result in integer.
        /// </summary>
        /// <param name="typeOne">is the first type</param>
        /// <param name="typeTwo">is the second type</param>
        /// <exception cref="CoercionException">if types don't allow coercion</exception>
        /// <returns>coerced type</returns>
        public static Type GetArithmaticCoercionType(Type typeOne, Type typeTwo)
                {
            Type boxedOne = GetBoxedType(typeOne);
            Type boxedTwo = GetBoxedType(typeTwo);
    
            if (!IsNumeric(boxedOne) || !IsNumeric(boxedTwo)) {
                throw new CoercionException("Cannot coerce types " + typeOne.Name + " and " + typeTwo.Name);
            }
            if (boxedOne == boxedTwo) {
                return boxedOne;
            }
            if ((boxedOne == Typeof(BigDecimal)) || (boxedTwo == Typeof(BigDecimal))) {
                return Typeof(BigDecimal);
            }
            if (((boxedOne == Typeof(BigInteger)) && JavaClassHelper.IsFloatingPointClass(boxedTwo)) ||
                    ((boxedTwo == Typeof(BigInteger)) && JavaClassHelper.IsFloatingPointClass(boxedOne))) {
                return Typeof(BigDecimal);
            }
            if ((boxedOne == Typeof(BigInteger)) || (boxedTwo == Typeof(BigInteger))) {
                return Typeof(BigInteger);
            }
            if ((boxedOne == Typeof(double?)) || (boxedTwo == Typeof(double?))) {
                return Typeof(double?);
            }
            if ((boxedOne == Typeof(Float)) && (!IsFloatingPointClass(typeTwo))) {
                return Typeof(double?);
            }
            if ((boxedTwo == Typeof(Float)) && (!IsFloatingPointClass(typeOne))) {
                return Typeof(double?);
            }
            if ((boxedOne == Typeof(long)) || (boxedTwo == Typeof(long))) {
                return Typeof(long);
            }
            return Typeof(int?);
        }
    
        /// <summary>
        /// Coerce the given number to the given type, assuming the type is a Boxed type. Allows coerce to lower resultion number.
        /// Does't coerce to primitive types.
        /// <para>
        /// Meant for statement compile-time use, not for runtime use.
        /// </para>
        /// </summary>
        /// <param name="numToCoerce">is the number to coerce to the given type</param>
        /// <param name="resultBoxedType">is the boxed result type to return</param>
        /// <returns>the numToCoerce as a value in the given result type</returns>
        public static Number CoerceBoxed(Number numToCoerce, Type resultBoxedType) {
            if (numToCoerce.Class == resultBoxedType) {
                return numToCoerce;
            }
            if (resultBoxedType == Typeof(double?)) {
                return NumToCoerce.DoubleValue();
            }
            if (resultBoxedType == Typeof(long)) {
                return NumToCoerce.LongValue();
            }
            if (resultBoxedType == Typeof(BigInteger)) {
                return BigInteger.ValueOf(numToCoerce.LongValue());
            }
            if (resultBoxedType == Typeof(BigDecimal)) {
                if (JavaClassHelper.IsFloatingPointNumber(numToCoerce)) {
                    return new BigDecimal(numToCoerce.DoubleValue());
                }
                return new BigDecimal(numToCoerce.LongValue());
            }
            if (resultBoxedType == Typeof(Float)) {
                return NumToCoerce.FloatValue();
            }
            if (resultBoxedType == Typeof(int?)) {
                return NumToCoerce.IntValue();
            }
            if (resultBoxedType == Typeof(Short)) {
                return NumToCoerce.ShortValue();
            }
            if (resultBoxedType == Typeof(Byte)) {
                return NumToCoerce.ByteValue();
            }
            throw new IllegalArgumentException("Cannot coerce to number subtype " + resultBoxedType.Name);
        }
    
        /// <summary>
        /// Returns true if the Number instance is a floating point number.
        /// </summary>
        /// <param name="number">to check</param>
        /// <returns>true if number is Float or double? type</returns>
        public static bool IsFloatingPointNumber(Number number) {
            if ((number is Float) ||
                    (number is double?)) {
                return true;
            }
            return false;
        }
    
        /// <summary>
        /// Returns true if the supplied type is a floating point number.
        /// </summary>
        /// <param name="clazz">to check</param>
        /// <returns>true if primitive or boxed float or double</returns>
        public static bool IsFloatingPointClass(Type clazz) {
            if ((clazz == Typeof(Float)) ||
                    (clazz == Typeof(double?)) ||
                    (clazz == Typeof(float)) ||
                    (clazz == Typeof(double))) {
                return true;
            }
            return false;
        }
    
        /// <summary>
        /// Returns for 2 classes to be compared via relational operator the Type type of
        /// common comparison. The output is always Typeof(long), Typeof(double?), Typeof(string) or Typeof(bool?)
        /// depending on whether the passed types are numeric and floating-point.
        /// Accepts primitive as well as boxed types.
        /// </summary>
        /// <param name="typeOne">is the first type</param>
        /// <param name="typeTwo">is the second type</param>
        /// <exception cref="CoercionException">if the types cannot be compared</exception>
        /// <returns>One of Typeof(long), Typeof(double?) or Typeof(string)</returns>
        public static Type GetCompareToCoercionType(Type typeOne, Type typeTwo) {
            if ((typeOne == Typeof(string)) && (typeTwo == Typeof(string))) {
                return Typeof(string);
            }
            if (((typeOne == Typeof(bool)) || ((typeOne == Typeof(bool?)))) &&
                    ((typeTwo == Typeof(bool)) || ((typeTwo == Typeof(bool?))))) {
                return Typeof(bool?);
            }
            if (!IsJavaBuiltinDataType(typeOne) && (!IsJavaBuiltinDataType(typeTwo))) {
                if (typeOne != typeTwo) {
                    return Typeof(Object);
                }
                return typeOne;
            }
            if (typeOne == null) {
                return typeTwo;
            }
            if (typeTwo == null) {
                return typeOne;
            }
            if (!IsNumeric(typeOne) || !IsNumeric(typeTwo)) {
                string typeOneName = typeOne.Name;
                string typeTwoName = typeTwo.Name;
                throw new CoercionException("Types cannot be compared: " + typeOneName + " and " + typeTwoName);
            }
            return GetArithmaticCoercionType(typeOne, typeTwo);
        }
    
        /// <summary>
        /// Returns true if the type is one of the big number types, i.e. BigDecimal or BigInteger
        /// </summary>
        /// <param name="clazz">to check</param>
        /// <returns>true for big number</returns>
        public static bool IsBigNumberType(Type clazz) {
            if ((clazz == Typeof(BigInteger)) || (clazz == Typeof(BigDecimal))) {
                return true;
            }
            return false;
        }
    
        /// <summary>
        /// Determines if a number can be coerced upwards to another number class without loss.
        /// <para>
        /// Clients must pass in two classes that are numeric types.
        /// </para>
        /// <para>
        /// Any number class can be coerced to double, while only double cannot be coerced to float.
        /// Any non-floating point number can be coerced to long.
        /// int? can be coerced to Byte and Short even though loss is possible, for convenience.
        /// </para>
        /// </summary>
        /// <param name="numberClassToBeCoerced">the number class to be coerced</param>
        /// <param name="numberClassToCoerceTo">the number class to coerce to</param>
        /// <returns>true if numbers can be coerced without loss, false if not</returns>
        public static bool CanCoerce(Type numberClassToBeCoerced, Type numberClassToCoerceTo) {
            Type boxedFrom = GetBoxedType(numberClassToBeCoerced);
            Type boxedTo = GetBoxedType(numberClassToCoerceTo);
    
            if (!IsNumeric(numberClassToBeCoerced)) {
                throw new IllegalArgumentException("Type '" + numberClassToBeCoerced + "' is not a numeric type'");
            }
    
            if (boxedTo == Typeof(Float)) {
                return (boxedFrom == Typeof(Byte)) ||
                        (boxedFrom == Typeof(Short)) ||
                        (boxedFrom == Typeof(int?)) ||
                        (boxedFrom == Typeof(long)) ||
                        (boxedFrom == Typeof(Float));
            } else if (boxedTo == Typeof(double?)) {
                return (boxedFrom == Typeof(Byte)) ||
                        (boxedFrom == Typeof(Short)) ||
                        (boxedFrom == Typeof(int?)) ||
                        (boxedFrom == Typeof(long)) ||
                        (boxedFrom == Typeof(Float)) ||
                        (boxedFrom == Typeof(double?));
            } else if (boxedTo == Typeof(BigDecimal)) {
                return (boxedFrom == Typeof(Byte)) ||
                        (boxedFrom == Typeof(Short)) ||
                        (boxedFrom == Typeof(int?)) ||
                        (boxedFrom == Typeof(long)) ||
                        (boxedFrom == Typeof(Float)) ||
                        (boxedFrom == Typeof(double?)) ||
                        (boxedFrom == Typeof(BigInteger)) ||
                        (boxedFrom == Typeof(BigDecimal));
            } else if (boxedTo == Typeof(BigInteger)) {
                return (boxedFrom == Typeof(Byte)) ||
                        (boxedFrom == Typeof(Short)) ||
                        (boxedFrom == Typeof(int?)) ||
                        (boxedFrom == Typeof(long)) ||
                        (boxedFrom == Typeof(BigInteger));
            } else if (boxedTo == Typeof(long)) {
                return (boxedFrom == Typeof(Byte)) ||
                        (boxedFrom == Typeof(Short)) ||
                        (boxedFrom == Typeof(int?)) ||
                        (boxedFrom == Typeof(long));
            } else if ((boxedTo == Typeof(int?)) ||
                    (boxedTo == Typeof(Short)) ||
                    (boxedTo == Typeof(Byte))) {
                return (boxedFrom == Typeof(Byte)) ||
                        (boxedFrom == Typeof(Short)) ||
                        (boxedFrom == Typeof(int?));
            } else {
                throw new IllegalArgumentException("Type '" + numberClassToCoerceTo + "' is not a numeric type'");
            }
        }
    
        /// <summary>
        /// Returns for the class name given the class name of the boxed (wrapped) type if
        /// the class name is one of the Java primitive types.
        /// </summary>
        /// <param name="className">is a class name, a Java primitive type or other class</param>
        /// <returns>
        /// boxed class name if Java primitive type, or just same class name passed in if not a primitive type
        /// </returns>
        public static string GetBoxedClassName(string className) {
            if (className.Equals(Typeof(char).Name)) {
                return Typeof(Character).Name;
            }
            if (className.Equals(Typeof(byte).Name)) {
                return Typeof(Byte).Name;
            }
            if (className.Equals(Typeof(short).Name)) {
                return Typeof(Short).Name;
            }
            if (className.Equals(Typeof(int).Name)) {
                return Typeof(int?).Name;
            }
            if (className.Equals(Typeof(long).Name)) {
                return Typeof(long).Name;
            }
            if (className.Equals(Typeof(float).Name)) {
                return Typeof(Float).Name;
            }
            if (className.Equals(Typeof(double).Name)) {
                return Typeof(double?).Name;
            }
            if (className.Equals(Typeof(bool).Name)) {
                return Typeof(bool?).Name;
            }
            return className;
        }
    
        /// <summary>
        /// Returns true if the class passed in is a Java built-in data type (primitive or wrapper) including string and 'null'.
        /// </summary>
        /// <param name="clazz">to check</param>
        /// <returns>true if built-in data type, or false if not</returns>
        public static bool IsJavaBuiltinDataType(Type clazz) {
            if (clazz == null) {
                return true;
            }
            Type clazzBoxed = GetBoxedType(clazz);
            if (IsNumeric(clazzBoxed)) {
                return true;
            }
            if (IsBoolean(clazzBoxed)) {
                return true;
            }
            if (clazzBoxed.Equals(Typeof(string))) {
                return true;
            }
            if (clazzBoxed.Equals(Typeof(CharSequence))) {
                return true;
            }
            if ((clazzBoxed.Equals(Typeof(char))) ||
                    (clazzBoxed.Equals(Typeof(Character)))) {
                return true;
            }
            if (clazzBoxed.Equals(Typeof(void))) {
                return true;
            }
            return false;
        }
    
        // null values are allowed and represent and unknown type
    
        /// <summary>
        /// Determines a common denominator type to which one or more types can be casted or coerced.
        /// For use in determining the result type in certain expressions (coalesce, case).
        /// <para>
        /// Null values are allowed as part of the input and indicate a 'null' constant value
        /// in an expression tree. Such as value doesn't have any type and can be ignored in
        /// determining a result type.
        /// </para>
        /// <para>
        /// For numeric types, determines a coercion type that all types can be converted to
        /// via the method getArithmaticCoercionType.
        /// </para>
        /// <para>
        /// Indicates that there is no common denominator type by throwing <seealso cref="CoercionException" />.
        /// </para>
        /// </summary>
        /// <param name="types">
        /// is an array of one or more types, which can be Java built-in (primitive or wrapper)
        /// or user types
        /// </param>
        /// <exception cref="CoercionException">when no coercion type could be determined</exception>
        /// <returns>
        /// common denominator type if any can be found, for use in comparison
        /// </returns>
        public static Type GetCommonCoercionType(Type[] types)
                {
            if (types.Length < 1) {
                throw new IllegalArgumentException("Unexpected zero length array");
            }
            if (types.Length == 1) {
                return GetBoxedType(types[0]);
            }
    
            // Reduce to non-null types
            var nonNullTypes = new List<Type>();
            for (int i = 0; i < types.Length; i++) {
                if (types[i] != null) {
                    nonNullTypes.Add(types[i]);
                }
            }
            types = nonNullTypes.ToArray(new Type[nonNullTypes.Count]);
    
            if (types.Length == 0) {
                return null;    // only null types, result is null
            }
            if (types.Length == 1) {
                return GetBoxedType(types[0]);
            }
    
            // Check if all String
            if (types[0] == Typeof(string)) {
                for (int i = 0; i < types.Length; i++) {
                    if (types[i] != Typeof(string)) {
                        throw new CoercionException("Cannot coerce to string type " + types[i].Name);
                    }
                }
                return Typeof(string);
            }
    
            // Convert to boxed types
            for (int i = 0; i < types.Length; i++) {
                types[i] = GetBoxedType(types[i]);
            }
    
            // Check if all boolean
            if (types[0] == Typeof(bool?)) {
                for (int i = 0; i < types.Length; i++) {
                    if (types[i] != Typeof(bool?)) {
                        throw new CoercionException("Cannot coerce to bool? type " + types[i].Name);
                    }
                }
                return Typeof(bool?);
            }
    
            // Check if all char
            if (types[0] == Typeof(Character)) {
                foreach (Type type in types) {
                    if (type != Typeof(Character)) {
                        throw new CoercionException("Cannot coerce to bool? type " + type.Name);
                    }
                }
                return Typeof(Character);
            }
    
            // Check if all the same non-Java builtin type, i.e. Java beans etc.
            bool isAllBuiltinTypes = true;
            bool isAllNumeric = true;
            foreach (Type type in types) {
                if (!IsNumeric(type) && (!IsJavaBuiltinDataType(type))) {
                    isAllBuiltinTypes = false;
                }
            }
    
            // handle all built-in types
            if (!isAllBuiltinTypes) {
                foreach (Type type in types) {
                    if (types[0] == type) {
                        continue;
                    }
                    if (IsJavaBuiltinDataType(type)) {
                        throw new CoercionException("Cannot coerce to " + types[0].Name + " type " + type.Name);
                    }
                    if (type != types[0]) {
                        return Typeof(Object);
                    }
                }
                return types[0];
            }
    
            // test for numeric
            if (!isAllNumeric) {
                throw new CoercionException("Cannot coerce to numeric type " + types[0].Name);
            }
    
            // Use arithmatic coercion type as the final authority, considering all types
            Type result = GetArithmaticCoercionType(types[0], types[1]);
            int count = 2;
            while (count < types.Length) {
                result = GetArithmaticCoercionType(result, types[count]);
                count++;
            }
            return result;
        }
    
        /// <summary>
        /// Returns the class given a fully-qualified class name.
        /// </summary>
        /// <param name="className">is the fully-qualified class name, java primitive types included.</param>
        /// <param name="classForNameProvider">lookup of class for class name</param>
        /// <exception cref="ClassNotFoundException">if the class cannot be found</exception>
        /// <returns>class for name</returns>
        public static Type GetClassForName(string className, ClassForNameProvider classForNameProvider) {
            if (className.Equals(Typeof(bool).Name)) {
                return Typeof(bool);
            }
            if (className.Equals(Typeof(char).Name)) {
                return Typeof(char);
            }
            if (className.Equals(Typeof(double).Name)) {
                return Typeof(double);
            }
            if (className.Equals(Typeof(float).Name)) {
                return Typeof(float);
            }
            if (className.Equals(Typeof(byte).Name)) {
                return Typeof(byte);
            }
            if (className.Equals(Typeof(short).Name)) {
                return Typeof(short);
            }
            if (className.Equals(Typeof(int).Name)) {
                return Typeof(int);
            }
            if (className.Equals(Typeof(long).Name)) {
                return Typeof(long);
            }
            return Typeof(classForNameProvider)ForName(className);
        }
    
        /// <summary>
        /// Returns the boxed class for the given classname, recognizing all primitive and abbreviations,
        /// uppercase and lowercase.
        /// <para>
        /// Recognizes "int" as Typeof(int?) and "strIng" as Typeof(string), and "int?" as Typeof(int?), and so on.
        /// </para>
        /// </summary>
        /// <param name="className">is the name to recognize</param>
        /// <param name="classForNameProvider">lookup of class for class name</param>
        /// <exception cref="EventAdapterException">is throw if the class cannot be identified</exception>
        /// <returns>class</returns>
        public static Type GetClassForSimpleName(string className, ClassForNameProvider classForNameProvider)
                {
            if (("string".Equals(className.ToLowerCase(Locale.ENGLISH).Trim())) ||
                    ("varchar".Equals(className.ToLowerCase(Locale.ENGLISH).Trim())) ||
                    ("varchar2".Equals(className.ToLowerCase(Locale.ENGLISH).Trim()))) {
                return Typeof(string);
            }
    
            if (("integer".Equals(className.ToLowerCase(Locale.ENGLISH).Trim())) ||
                    ("int".Equals(className.ToLowerCase(Locale.ENGLISH).Trim()))) {
                return Typeof(int?);
            }
    
            if ("bool".Equals(className.ToLowerCase(Locale.ENGLISH).Trim())) {
                return Typeof(bool?);
            }
    
            if ("character".Equals(className.ToLowerCase(Locale.ENGLISH).Trim())) {
                return Typeof(Character);
            }
    
            // use the boxed type for primitives
            string boxedClassName = JavaClassHelper.GetBoxedClassName(className.Trim());
    
            try {
                return Typeof(classForNameProvider)ForName(boxedClassName);
            } catch (ClassNotFoundException ex) {
                // expected
            }
    
            boxedClassName = JavaClassHelper.GetBoxedClassName(className.ToLowerCase(Locale.ENGLISH).Trim());
            try {
                return Typeof(classForNameProvider)ForName(boxedClassName);
            } catch (ClassNotFoundException ex) {
                return null;
            }
        }
    
        public static string GetSimpleNameForClass(Type clazz) {
            if (clazz == null) {
                return "(null)";
            }
            if (JavaClassHelper.IsImplementsInterface(clazz, Typeof(CharSequence))) {
                return "string";
            }
            Type boxed = JavaClassHelper.GetBoxedType(clazz);
            if (boxed == Typeof(int?)) {
                return "int";
            }
            if (boxed == Typeof(bool?)) {
                return "bool";
            }
            if (boxed == Typeof(Character)) {
                return "character";
            }
            if (boxed == Typeof(double?)) {
                return "double";
            }
            if (boxed == Typeof(Float)) {
                return "float";
            }
            if (boxed == Typeof(Byte)) {
                return "byte";
            }
            if (boxed == Typeof(Short)) {
                return "short";
            }
            if (boxed == Typeof(long)) {
                return "long";
            }
            return Clazz.SimpleName;
        }
    
        /// <summary>
        /// Returns the class for a Java primitive type name, ignoring case, and considering string as a primitive.
        /// </summary>
        /// <param name="typeName">is a potential primitive Java type, or some other type name</param>
        /// <returns>
        /// class for primitive type name, or null if not a primitive type.
        /// </returns>
        public static Type GetPrimitiveClassForName(string typeName) {
            typeName = typeName.ToLowerCase(Locale.ENGLISH);
            if (typeName.Equals("bool")) {
                return Typeof(bool);
            }
            if (typeName.Equals("char")) {
                return Typeof(char);
            }
            if (typeName.Equals("double")) {
                return Typeof(double);
            }
            if (typeName.Equals("float")) {
                return Typeof(float);
            }
            if (typeName.Equals("byte")) {
                return Typeof(byte);
            }
            if (typeName.Equals("short")) {
                return Typeof(short);
            }
            if (typeName.Equals("int")) {
                return Typeof(int);
            }
            if (typeName.Equals("long")) {
                return Typeof(long);
            }
            if (typeName.Equals("string")) {
                return Typeof(string);
            }
            return null;
        }
    
        /// <summary>
        /// Parse the string using the given Java built-in class for parsing.
        /// </summary>
        /// <param name="clazz">is the class to parse the value to</param>
        /// <param name="text">is the text to parse</param>
        /// <returns>value matching the type passed in</returns>
        public static Object Parse(Type clazz, string text) {
            Type classBoxed = JavaClassHelper.GetBoxedType(clazz);
    
            if (classBoxed == Typeof(string)) {
                return text;
            }
            if (classBoxed == Typeof(Character)) {
                return Text.CharAt(0);
            }
            if (classBoxed == Typeof(bool?)) {
                return BoolValue.ParseString(text.ToLowerCase(Locale.ENGLISH).Trim());
            }
            if (classBoxed == Typeof(Byte)) {
                return ByteValue.ParseString(text.Trim());
            }
            if (classBoxed == Typeof(Short)) {
                return ShortValue.ParseString(text.Trim());
            }
            if (classBoxed == Typeof(long)) {
                return LongValue.ParseString(text.Trim());
            }
            if (classBoxed == Typeof(Float)) {
                return FloatValue.ParseString(text.Trim());
            }
            if (classBoxed == Typeof(double?)) {
                return DoubleValue.ParseString(text.Trim());
            }
            if (classBoxed == Typeof(int?)) {
                return IntValue.ParseString(text.Trim());
            }
            return null;
        }
    
        /// <summary>
        /// Method to check if a given class, and its superclasses and interfaces (deep), implement a given interface.
        /// </summary>
        /// <param name="clazz">to check, including all its superclasses and their interfaces and extends</param>
        /// <param name="interfaceClass">is the interface class to look for</param>
        /// <returns>
        /// true if such interface is implemented by any of the clazz or its superclasses or
        /// extends by any interface and superclasses (deep check)
        /// </returns>
        public static bool IsImplementsInterface(Type clazz, Type interfaceClass) {
            if (!(interfaceClass.IsInterface)) {
                throw new IllegalArgumentException("Interface class passed in is not an interface");
            }
            bool resultThisClass = RecursiveIsImplementsInterface(clazz, interfaceClass);
            if (resultThisClass) {
                return true;
            }
            return RecursiveSuperclassImplementsInterface(clazz, interfaceClass);
        }
    
        /// <summary>
        /// Method to check if a given class, and its superclasses and interfaces (deep), implement a given interface or extend a given class.
        /// </summary>
        /// <param name="extendorOrImplementor">is the class to inspects its : and, clauses</param>
        /// <param name="extendedOrImplemented">is the potential interface, or superclass, to check</param>
        /// <returns>
        /// true if such interface is implemented by any of the clazz or its superclasses or
        /// extends by any interface and superclasses (deep check)
        /// </returns>
        public static bool IsSubclassOrImplementsInterface(Type extendorOrImplementor, Type extendedOrImplemented) {
            if (extendorOrImplementor.Equals(extendedOrImplemented)) {
                return true;
            }
            if (extendedOrImplemented.IsInterface) {
                return RecursiveIsImplementsInterface(extendorOrImplementor, extendedOrImplemented) ||
                        RecursiveSuperclassImplementsInterface(extendorOrImplementor, extendedOrImplemented);
            }
            return RecursiveIsSuperClass(extendorOrImplementor, extendedOrImplemented);
        }
    
        private static bool RecursiveIsSuperClass(Type clazz, Type superClass) {
            if (clazz == null) {
                return false;
            }
            if (clazz.IsPrimitive) {
                return false;
            }
            Type mySuperClass = clazz.Superclass;
            if (mySuperClass == superClass) {
                return true;
            }
            if (mySuperClass == Typeof(Object)) {
                return false;
            }
            return RecursiveIsSuperClass(mySuperClass, superClass);
        }
    
        private static bool RecursiveSuperclassImplementsInterface(Type clazz, Type interfaceClass) {
            Type superClass = clazz.Superclass;
            if ((superClass == null) || (superClass == Typeof(Object))) {
                return false;
            }
            bool result = RecursiveIsImplementsInterface(superClass, interfaceClass);
            if (result) {
                return result;
            }
            return RecursiveSuperclassImplementsInterface(superClass, interfaceClass);
        }
    
        private static bool RecursiveIsImplementsInterface(Type clazz, Type interfaceClass) {
            if (clazz == interfaceClass) {
                return true;
            }
            Type[] interfaces = clazz.Interfaces;
            if (interfaces == null) {
                return false;
            }
            foreach (Type implementedInterface in interfaces) {
                if (implementedInterface == interfaceClass) {
                    return true;
                }
                bool result = RecursiveIsImplementsInterface(implementedInterface, interfaceClass);
                if (result) {
                    return result;
                }
            }
            return false;
        }
    
        /// <summary>
        /// Looks up the given class and checks that it : or : therequired interface,
        /// and instantiates an object.
        /// </summary>
        /// <param name="implementedOrExtendedClass">is the class that the looked-up class should extend or implement</param>
        /// <param name="className">of the class to load, check type and instantiate</param>
        /// <param name="classForNameProvider">lookup of class for class name</param>
        /// <exception cref="ClassInstantiationException">if the type does not match or the class cannot be loaded or an object instantiated</exception>
        /// <returns>instance of given class, via newInstance</returns>
        public static Object Instantiate(Type implementedOrExtendedClass, string className, ClassForNameProvider classForNameProvider) {
            Type clazz;
            try {
                clazz = Typeof(classForNameProvider)ForName(className);
            } catch (ClassNotFoundException ex) {
                throw new ClassInstantiationException("Unable to load class '" + className + "', class not found", ex);
            }
    
            return Instantiate(implementedOrExtendedClass, clazz);
        }
    
        /// <summary>
        /// Checks that the given class : or : therequired interface (first parameter),
        /// and instantiates an object.
        /// </summary>
        /// <param name="implementedOrExtendedClass">is the class that the looked-up class should extend or implement</param>
        /// <param name="clazz">to check type and instantiate</param>
        /// <exception cref="ClassInstantiationException">if the type does not match or the class cannot be loaded or an object instantiated</exception>
        /// <returns>instance of given class, via newInstance</returns>
        public static Object Instantiate(Type implementedOrExtendedClass, Type clazz) {
            if (!JavaClassHelper.IsSubclassOrImplementsInterface(clazz, implementedOrExtendedClass)) {
                if (implementedOrExtendedClass.IsInterface) {
                    throw new ClassInstantiationException("Type '" + clazz.Name + "' does not implement interface '" + implementedOrExtendedClass.Name + "'");
                }
                throw new ClassInstantiationException("Type '" + clazz.Name + "' does not extend '" + implementedOrExtendedClass.Name + "'");
            }
    
            Object obj;
            try {
                obj = clazz.NewInstance();
            } catch (InstantiationException ex) {
                throw new ClassInstantiationException("Unable to instantiate from class '" + clazz.Name + "' via default constructor", ex);
            } catch (IllegalAccessException ex) {
                throw new ClassInstantiationException("Illegal access when instantiating class '" + clazz.Name + "' via default constructor", ex);
            }
    
            return obj;
        }
    
        /// <summary>
        /// Populates all interface and superclasses for the given class, recursivly.
        /// </summary>
        /// <param name="clazz">to reflect upon</param>
        /// <param name="result">set of classes to populate</param>
        public static void GetSuper(Type clazz, Set<Type> result) {
            GetSuperInterfaces(clazz, result);
            GetSuperClasses(clazz, result);
        }
    
        /// <summary>
        /// Returns true if the simple class name is the class name of the fully qualified classname.
        /// <para>
        /// This method does not verify validity of class and package names, it uses simple string compare
        /// inspecting the trailing part of the fully qualified class name.
        /// </para>
        /// </summary>
        /// <param name="simpleClassName">simple class name</param>
        /// <param name="fullyQualifiedClassname">fully qualified class name contains package name and simple class name</param>
        /// <returns>
        /// true if simple class name of the fully qualified class name, false if not
        /// </returns>
        public static bool IsSimpleNameFullyQualfied(string simpleClassName, string fullyQualifiedClassname) {
            if ((fullyQualifiedClassname.EndsWith("." + simpleClassName)) || (fullyQualifiedClassname.Equals(simpleClassName))) {
                return true;
            }
            return false;
        }
    
        /// <summary>
        /// Returns true if the Type is a fragmentable type, i.e. not a primitive or boxed type or
        /// any of the common built-in types or does not implement Map.
        /// </summary>
        /// <param name="propertyType">type to check</param>
        /// <returns>true if fragmentable</returns>
        public static bool IsFragmentableType(Type propertyType) {
            if (propertyType == null) {
                return false;
            }
            if (propertyType.IsArray) {
                return IsFragmentableType(propertyType.ComponentType);
            }
            if (JavaClassHelper.IsJavaBuiltinDataType(propertyType)) {
                return false;
            }
            if (propertyType.IsEnum) {
                return false;
            }
            if (JavaClassHelper.IsImplementsInterface(propertyType, Typeof(Map))) {
                return false;
            }
            if (propertyType == Typeof(Node)) {
                return false;
            }
            if (propertyType == Typeof(NodeList)) {
                return false;
            }
            if (propertyType == Typeof(Object)) {
                return false;
            }
            if (propertyType == Typeof(Calendar)) {
                return false;
            }
            if (propertyType == Typeof(Date)) {
                return false;
            }
            if (propertyType == Typeof(LocalDateTime)) {
                return false;
            }
            if (propertyType == Typeof(ZonedDateTime)) {
                return false;
            }
            if (propertyType == Typeof(LocalDate)) {
                return false;
            }
            if (propertyType == Typeof(LocalTime)) {
                return false;
            }
            if (propertyType == Typeof(java.sql.Date)) {
                return false;
            }
            if (propertyType == Typeof(java.sql.Time)) {
                return false;
            }
            if (propertyType == Typeof(java.sql.Timestamp)) {
                return false;
            }
            if (propertyType.Name.Equals(AvroConstantsNoDep.GENERIC_RECORD_CLASSNAME)) {
                return false;
            }
            return true;
        }
    
        public static Type[] GetSuperInterfaces(Type clazz) {
            var interfaces = new HashSet<Type>();
            Type[] declaredInterfaces = clazz.Interfaces;
    
            for (int i = 0; i < declaredInterfaces.Length; i++) {
                interfaces.Add(declaredInterfaces[i]);
                GetSuperInterfaces(declaredInterfaces[i], interfaces);
            }
    
            var superClasses = new HashSet<Type>();
            GetSuperClasses(clazz, superClasses);
            foreach (Type superClass in superClasses) {
                declaredInterfaces = superClass.Interfaces;
    
                for (int i = 0; i < declaredInterfaces.Length; i++) {
                    interfaces.Add(declaredInterfaces[i]);
                    GetSuperInterfaces(declaredInterfaces[i], interfaces);
                }
            }
    
            return Interfaces.ToArray(new Type[declaredInterfaces.Length]);
        }
    
        public static void GetSuperInterfaces(Type clazz, Set<Type> result) {
            Type[] interfaces = clazz.Interfaces;
    
            for (int i = 0; i < interfaces.Length; i++) {
                result.Add(interfaces[i]);
                GetSuperInterfaces(interfaces[i], result);
            }
        }
    
        private static void GetSuperClasses(Type clazz, Set<Type> result) {
            Type superClass = clazz.Superclass;
            if (superClass == null) {
                return;
            }
    
            result.Add(superClass);
            GetSuper(superClass, result);
        }
    
        /// <summary>
        /// Returns the generic type parameter of a return value by a field or method.
        /// </summary>
        /// <param name="method">method or null if field</param>
        /// <param name="field">field or null if method</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected Typeof(Object)</param>
        /// <returns>generic type parameter</returns>
        public static Type GetGenericReturnType(Method method, Field field, bool isAllowNull) {
            if (method == null) {
                return GetGenericFieldType(field, isAllowNull);
            } else {
                return GetGenericReturnType(method, isAllowNull);
            }
        }
    
        /// <summary>
        /// Returns the second generic type parameter of a return value by a field or method.
        /// </summary>
        /// <param name="method">method or null if field</param>
        /// <param name="field">field or null if method</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected Typeof(Object)</param>
        /// <returns>generic type parameter</returns>
        public static Type GetGenericReturnTypeMap(Method method, Field field, bool isAllowNull) {
            if (method == null) {
                return GetGenericFieldTypeMap(field, isAllowNull);
            } else {
                return GetGenericReturnTypeMap(method, isAllowNull);
            }
        }
    
        /// <summary>
        /// Returns the generic type parameter of a return value by a method.
        /// </summary>
        /// <param name="method">method or null if field</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected Typeof(Object)</param>
        /// <returns>generic type parameter</returns>
        public static Type GetGenericReturnType(Method method, bool isAllowNull) {
            Type t = method.GenericReturnType;
            Type result = GetGenericType(t, 0);
            if (!isAllowNull && result == null) {
                return Typeof(Object);
            }
            return result;
        }
    
        /// <summary>
        /// Returns the second generic type parameter of a return value by a field or method.
        /// </summary>
        /// <param name="method">method or null if field</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected Typeof(Object)</param>
        /// <returns>generic type parameter</returns>
        public static Type GetGenericReturnTypeMap(Method method, bool isAllowNull) {
            Type t = method.GenericReturnType;
            Type result = GetGenericType(t, 1);
            if (!isAllowNull && result == null) {
                return Typeof(Object);
            }
            return result;
        }
    
        /// <summary>
        /// Returns the generic type parameter of a return value by a field.
        /// </summary>
        /// <param name="field">field or null if method</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected Typeof(Object)</param>
        /// <returns>generic type parameter</returns>
        public static Type GetGenericFieldType(Field field, bool isAllowNull) {
            Type t = field.GenericType;
            Type result = GetGenericType(t, 0);
            if (!isAllowNull && result == null) {
                return Typeof(Object);
            }
            return result;
        }
    
        /// <summary>
        /// Returns the generic type parameter of a return value by a field or method.
        /// </summary>
        /// <param name="field">field or null if method</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected Typeof(Object)</param>
        /// <returns>generic type parameter</returns>
        public static Type GetGenericFieldTypeMap(Field field, bool isAllowNull) {
            Type t = field.GenericType;
            Type result = GetGenericType(t, 1);
            if (!isAllowNull && result == null) {
                return Typeof(Object);
            }
            return result;
        }
    
        public static Type GetGenericType(Type t, int index) {
            if (t == null) {
                return null;
            }
            if (!(t is ParameterizedType)) {
                return null;
            }
            ParameterizedType ptype = (ParameterizedType) t;
            if ((ptype.ActualTypeArguments == null) || (ptype.ActualTypeArguments.Length < (index + 1))) {
                return Typeof(Object);
            }
            Type typeParam = ptype.ActualTypeArguments[index];
            if (typeParam is GenericArrayType) {
                GenericArrayType genericArrayType = (GenericArrayType) typeParam;
                if (genericArrayType.GenericComponentType is Type) {
                    return JavaClassHelper.GetArrayType((Type) genericArrayType.GenericComponentType);
                }
            }
            if (!(typeParam is Type)) {
                return Typeof(Object);
            }
            return (Type) typeParam;
        }
    
        /// <summary>
        /// Returns an instance of a hook as specified by an annotation.
        /// </summary>
        /// <param name="annotations">to search</param>
        /// <param name="hookType">type to look for</param>
        /// <param name="interfaceExpected">interface required</param>
        /// <param name="engineImportService">for resolving references</param>
        /// <exception cref="ExprValidationException">if instantiation failed</exception>
        /// <returns>hook instance</returns>
        public static Object GetAnnotationHook(Annotation[] annotations, HookType hookType, Type interfaceExpected, EngineImportService engineImportService)
                {
            if (annotations == null) {
                return null;
            }
            string hookClass = null;
            for (int i = 0; i < annotations.Length; i++) {
                if (!(annotations[i] is Hook)) {
                    continue;
                }
                Hook hook = (Hook) annotations[i];
                if (hook.Type() != hookType) {
                    continue;
                }
                hookClass = hook.Hook();
            }
            if (hookClass == null) {
                return null;
            }
    
            Type clazz;
            try {
                clazz = engineImportService.ResolveClass(hookClass, false);
            } catch (Exception e) {
                throw new ExprValidationException("Failed to resolve hook provider of hook type '" + hookType +
                        "' import '" + hookClass + "' :" + e.Message);
            }
    
            if (!JavaClassHelper.IsImplementsInterface(clazz, interfaceExpected)) {
                throw new ExprValidationException("Hook provider for hook type '" + hookType + "' " +
                        "class '" + clazz.Name + "' does not implement the required '" + interfaceExpected.SimpleName +
                        "' interface");
            }
    
            Object hook;
            try {
                hook = clazz.NewInstance();
            } catch (Exception e) {
                throw new ExprValidationException("Failed to instantiate hook provider of hook type '" + hookType + "' " +
                        "class '" + clazz.Name + "' :" + e.Message);
            }
    
            return hook;
        }
    
        /// <summary>
        /// Resolve a string constant as a possible enumeration value, returning null if not resolved.
        /// </summary>
        /// <param name="constant">to resolve</param>
        /// <param name="engineImportService">for engine-level use to resolve enums, can be null</param>
        /// <param name="isAnnotation">whether we are in an annotation</param>
        /// <exception cref="ExprValidationException">if there is an error accessing the enum</exception>
        /// <returns>null or enumeration value</returns>
        public static Object ResolveIdentAsEnumConst(string constant, EngineImportService engineImportService, bool isAnnotation)
                {
            int lastDotIndex = constant.LastIndexOf('.');
            if (lastDotIndex == -1) {
                return null;
            }
            string className = constant.Substring(0, lastDotIndex);
            string constName = constant.Substring(lastDotIndex + 1);
    
            // un-escape
            className = Unescape(className);
            constName = Unescape(constName);
    
            Type clazz;
            try {
                clazz = engineImportService.ResolveClass(className, isAnnotation);
            } catch (EngineImportException e) {
                return null;
            }
    
            Field field;
            try {
                field = clazz.GetField(constName);
            } catch (NoSuchFieldException e) {
                return null;
            }
    
            int modifiers = field.Modifiers;
            if (Modifier.IsPublic(modifiers) && Modifier.IsStatic(modifiers)) {
                try {
                    return Field.Get(null);
                } catch (IllegalAccessException e) {
                    throw new ExprValidationException("Exception accessing field '" + field.Name + "': " + e.Message, e);
                }
            }
    
            return null;
        }
    
        public static Type GetArrayType(Type resultType) {
            return Array.NewInstance(resultType, 0).Class;
        }
    
        public static string GetClassNameFullyQualPretty(Type clazz) {
            if (clazz == null) {
                return "null";
            }
            if (clazz.IsArray) {
                return Clazz.ComponentType.Name + "(Array)";
            }
            return Clazz.Name;
        }
    
        public static string GetClassNameFullyQualPrettyWithClassloader(Type clazz) {
            string name = GetClassNameFullyQualPretty(clazz);
            string classloader = GetClassLoaderId(clazz.ClassLoader);
            return name + "(loaded by " + classloader + ")";
        }
    
        public static string GetClassLoaderId(ClassLoader classLoader) {
            if (classLoader == null) {
                return "(classloader is null)";
            }
            return ClassLoader.Class.Name + "@" + System.IdentityHashCode(classLoader);
        }
    
        public static Method GetMethodByName(Type clazz, string methodName) {
            foreach (Method m in clazz.Methods) {
                if (m.Name.Equals(methodName)) {
                    return m;
                }
            }
            throw new IllegalStateException("Expected '" + methodName + "' method not found on interface '" + clazz.Name);
        }
    
        public static string PrintInstance(Object instance, bool fullyQualified) {
            if (instance == null) {
                return "(null)";
            }
            var writer = new StringWriter();
            WriteInstance(writer, instance, fullyQualified);
            return Writer.ToString();
        }
    
        public static void WriteInstance(StringWriter writer, Object instance, bool fullyQualified) {
            if (instance == null) {
                writer.Write("(null)");
                return;
            }
    
            string className;
            if (fullyQualified) {
                className = instance.Class.Name;
            } else {
                className = instance.Class.SimpleName;
            }
            WriteInstance(writer, className, instance);
        }
    
        public static void WriteInstance(StringWriter writer, string title, Object instance) {
            writer.Write(title);
            writer.Write("@");
            if (instance == null) {
                writer.Write("(null)");
            } else {
                writer.Write(int?.ToHexString(System.IdentityHashCode(instance)));
            }
        }
    
        public static string GetMessageInvocationTarget(string statementName, Method method, string classOrPropertyName, Object[] args, InvocationTargetException e) {
    
            string parameters = args == null ? "null" : Arrays.ToString(args);
            if (args != null) {
                Type[] methodParameters = method.ParameterTypes;
                for (int i = 0; i < methodParameters.Length; i++) {
                    if (methodParameters[i].IsPrimitive && args[i] == null) {
                        return "NullPointerException invoking method '" + method.Name +
                                "' of class '" + classOrPropertyName +
                                "' in parameter " + i +
                                " passing parameters " + parameters +
                                " for statement '" + statementName + "': The method expects a primitive " + methodParameters[i].SimpleName +
                                " value but received a null value";
                    }
                }
            }
    
            return "Invocation exception when invoking method '" + method.Name +
                    "' of class '" + classOrPropertyName +
                    "' passing parameters " + parameters +
                    " for statement '" + statementName + "': " + e.TargetException.Class.SimpleName + " : " + e.TargetException.Message;
        }
    
        public static bool IsDatetimeClass(Type inputType) {
            if (inputType == null) {
                return false;
            }
            if ((!JavaClassHelper.IsSubclassOrImplementsInterface(inputType, Typeof(Calendar))) &&
                    (!JavaClassHelper.IsSubclassOrImplementsInterface(inputType, Typeof(Date))) &&
                    (!JavaClassHelper.IsSubclassOrImplementsInterface(inputType, Typeof(LocalDateTime))) &&
                    (!JavaClassHelper.IsSubclassOrImplementsInterface(inputType, Typeof(ZonedDateTime))) &&
                    (JavaClassHelper.GetBoxedType(inputType) != Typeof(long))) {
                return false;
            }
            return true;
        }
    
        public static IDictionary<string, Object> GetClassObjectFromPropertyTypeNames(Properties properties, ClassForNameProvider classForNameProvider) {
            var propertyTypes = new LinkedHashMap<string, Object>();
            foreach (var entry in properties) {
                string className = (string) entry.Value;
    
                if ("string".Equals(className)) {
                    className = Typeof(string).Name;
                }
    
                // use the boxed type for primitives
                string boxedClassName = JavaClassHelper.GetBoxedClassName(className);
    
                Type clazz;
                try {
                    clazz = Typeof(classForNameProvider)ForName(boxedClassName);
                } catch (ClassNotFoundException ex) {
                    throw new ConfigurationException("Unable to load class '" + boxedClassName + "', class not found", ex);
                }
    
                propertyTypes.Put((string) entry.Key, clazz);
            }
            return propertyTypes;
        }
    
        public static Type GetClassInClasspath(string classname, ClassForNameProvider classForNameProvider) {
            try {
                Type clazz = Typeof(classForNameProvider)ForName(classname);
                return clazz;
            } catch (ClassNotFoundException ex) {
                return null;
            }
        }
    
        public static bool IsSignatureCompatible(Type<?>[] one, Type<?>[] two) {
            if (Arrays.Equals(one, two)) {
                return true;
            }
            if (one.Length != two.Length) {
                return false;
            }
            for (int i = 0; i < one.Length; i++) {
                Type oneClass = one[i];
                Type twoClass = two[i];
                if (!JavaClassHelper.IsAssignmentCompatible(oneClass, twoClass)) {
                    return false;
                }
            }
            return true;
        }
    
        public static Method FindRequiredMethod(Type clazz, string methodName) {
            Method found = null;
            foreach (Method m in clazz.Methods) {
                if (m.Name.Equals(methodName)) {
                    found = m;
                    break;
                }
            }
            if (found == null) {
                throw new IllegalArgumentException("Not found method '" + methodName + "'");
            }
            return found;
        }
    
        public static List<Annotation> GetAnnotations(Type<? : Annotation> annotationClass, Annotation[] annotations) {
            List<Annotation> result = null;
            foreach (Annotation annotation in annotations) {
                if (annotation.AnnotationType() == annotationClass) {
                    if (result == null) {
                        result = new List<Annotation>();
                    }
                    result.Add(annotation);
                }
            }
            if (result == null) {
                return Collections.EmptyList();
            }
            return result;
        }
    
        public static bool IsAnnotationListed(Type<? : Annotation> annotationClass, Annotation[] annotations) {
            return !GetAnnotations(annotationClass, annotations).IsEmpty();
        }
    
        public static Set<Field> FindAnnotatedFields(Type targetClass, Type<? : Annotation> annotation) {
            var fields = new LinkedHashSet<Field>();
            FindFieldInternal(targetClass, annotation, fields);
    
            // superclass fields
            Type clazz = targetClass;
            while (true) {
                clazz = clazz.Superclass;
                if (clazz == Typeof(Object) || clazz == null) {
                    break;
                }
                FindFieldInternal(clazz, annotation, fields);
            }
            return fields;
        }
    
        private static void FindFieldInternal(Type currentClass, Type<? : Annotation> annotation, Set<Field> fields) {
            foreach (Field field in currentClass.DeclaredFields) {
                if (IsAnnotationListed(annotation, field.DeclaredAnnotations)) {
                    fields.Add(field);
                }
            }
        }
    
        public static Set<Method> FindAnnotatedMethods(Type targetClass, Type<? : Annotation> annotation) {
            var methods = new LinkedHashSet<Method>();
            FindAnnotatedMethodsInternal(targetClass, annotation, methods);
    
            // superclass fields
            Type clazz = targetClass;
            while (true) {
                clazz = clazz.Superclass;
                if (clazz == Typeof(Object) || clazz == null) {
                    break;
                }
                FindAnnotatedMethodsInternal(clazz, annotation, methods);
            }
            return methods;
        }
    
        private static void FindAnnotatedMethodsInternal(Type currentClass, Type<? : Annotation> annotation, Set<Method> methods) {
            foreach (Method method in currentClass.DeclaredMethods) {
                if (IsAnnotationListed(annotation, method.DeclaredAnnotations)) {
                    methods.Add(method);
                }
            }
        }
    
        public static void SetFieldForAnnotation(Object target, Type<? : Annotation> annotation, Object value) {
            bool found = SetFieldForAnnotation(target, annotation, value, target.Class);
            if (!found) {
    
                Type superClass = target.Class.Superclass;
                while (!found) {
                    found = SetFieldForAnnotation(target, annotation, value, superClass);
                    if (!found) {
                        superClass = superClass.Superclass;
                    }
                    if (superClass == Typeof(Object) || superClass == null) {
                        break;
                    }
                }
            }
        }
    
        private static bool SetFieldForAnnotation(Object target, Type<? : Annotation> annotation, Object value, Type currentClass) {
            bool found = false;
            foreach (Field field in currentClass.DeclaredFields) {
                if (IsAnnotationListed(annotation, field.DeclaredAnnotations)) {
                    field.Accessible = true;
                    try {
                        field.Set(target, value);
                    } catch (IllegalAccessException e) {
                        throw new RuntimeException("Failed to set field " + field + " on class " + currentClass.Name + ": " + e.Message, e);
                    }
                    return true;
                }
            }
            return found;
        }
    
        public static Pair<string, bool?> IsGetArrayType(string type) {
            int index = type.IndexOf('[');
            if (index == -1) {
                return new Pair<string, bool?>(type, false);
            }
            string typeOnly = type.Substring(0, index);
            return new Pair<string, bool?>(typeOnly.Trim(), true);
        }
    
        public static Type[] TakeFirstN(Type[] classes, int numToTake) {
            var shrunk = new Type[numToTake];
            System.Arraycopy(classes, 0, shrunk, 0, shrunk.Length);
            return shrunk;
        }
    
        public static Type[] TakeFirstN(Type[] types, int numToTake) {
            var shrunk = new Type[numToTake];
            System.Arraycopy(types, 0, shrunk, 0, shrunk.Length);
            return shrunk;
        }
    
        private static string Unescape(string name) {
            if (name.StartsWith("`") && name.EndsWith("`")) {
                return Name.Substring(1, name.Length() - 1);
            }
            return name;
        }
    }
} // end of namespace
