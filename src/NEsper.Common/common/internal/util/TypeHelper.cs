///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Loader;
#endif

using System.Text;
using System.Xml;

using Antlr4.Runtime;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.magic;
using com.espertech.esper.compat.util;
using com.espertech.esper.grammar.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

using TypeExtensions = com.espertech.esper.compat.TypeExtensions;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Helper for questions about types.
    ///     <para> what is the boxed type for a primitive type</para>
    ///     <para> is this a numeric type.</para>
    /// </summary>
    public static class TypeHelper
    {
        public const string AVRO_GENERIC_RECORD_CLASSNAME = "Avro.Generic.GenericRecord";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Integral types (used for testing)
        /// </summary>
        private static readonly Type[] IntegralTypes = {
            typeof(sbyte?), typeof(sbyte),
            typeof(byte?), typeof(byte),
            typeof(short?), typeof(short),
            typeof(ushort?), typeof(ushort),
            typeof(int?), typeof(int),
            typeof(uint?), typeof(uint),
            typeof(long?), typeof(long),
            typeof(ulong?), typeof(ulong)
        };

        private static readonly Dictionary<Type, int> IntegralTable;

        private static readonly Type BaseGenericDictionary =
            typeof(IDictionary<object, object>).GetGenericTypeDefinition();

        /// <summary>
        ///     Initializes the <see cref="TypeHelper" /> class.
        /// </summary>
        static TypeHelper()
        {
            IntegralTable = new Dictionary<Type, int>();
            for (var ii = 0; ii < IntegralTypes.Length; ii++) {
                IntegralTable[IntegralTypes[ii]] = ii;
            }
        }

        /// <summary>
        ///     When provided allows for applications to define the search path for assemblies.
        ///     In the absence of this, the AppDomain is queried for assemblies to search.
        /// </summary>
        public static Func<IEnumerable<Assembly>> AssemblySearchPath { get; }

        /// <summary>
        ///     When provided allows for applications to define the entire mechanism for
        ///     resolving a type.  In the absence, the default search algorithm is used which
        ///     employs the AssemblySearchPath.
        /// </summary>
        public static Func<TypeResolverEventArgs, Type> TypeResolver { get; }

        public static string GetSimpleName(this Type type)
        {
            return type.Name;
        }

        /// <summary>
        ///     Gets the parameter as string.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static string GetParameterAsString(IEnumerable<ParameterInfo> parameters)
        {
            return GetParameterAsString(parameters.Select(param => param.ParameterType));
        }

        /// <summary>
        ///     Returns a comma-separated parameter type list in readable form,considering arrays and null-type parameters.
        /// </summary>
        /// <param name="parameters">is the parameter types to render</param>
        /// <param name="useFullName">if set to <c>true</c> [use full name].</param>
        /// <returns>rendered list of parameters</returns>
        public static string GetParameterAsString(
            IEnumerable<Type> parameters,
            bool useFullName = true)
        {
            var builder = new StringBuilder();
            var delimiterComma = ", ";
            var delimiter = "";
            foreach (var param in parameters) {
                builder.Append(delimiter);
                builder.Append(GetParameterAsString(param, useFullName));
                delimiter = delimiterComma;
            }

            return builder.ToString();
        }

        /// <summary>
        ///     Returns a parameter as a string text, allowing null values to represent a nullselect expression type.
        /// </summary>
        /// <param name="param">is the parameter type</param>
        /// <param name="useFullName">if set to <c>true</c> [use full name].</param>
        /// <returns>string representation of parameter</returns>
        public static string GetParameterAsString(
            this Type param,
            bool useFullName = true)
        {
            return param.CleanName(useFullName);
        }

        /// <summary>
        ///     Determines whether the specified type is comparable.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsComparable(this Type type)
        {
            return type.GetUnboxedType().IsImplementsInterface<IComparable>();
        }

        /// <summary>
        ///     Returns the un-boxed class for the given class, or the class itself if already un-boxed or not a primitive type.
        ///     For primitive boxed types returns the unboxed primitive type, e.g. returns typeof(int) for passing typeof(int?).
        ///     For type other class, returns the class passed.
        /// </summary>
        /// <param name="type">
        ///     is the class to return the unboxed (or primitive) class for
        /// </param>
        /// <returns>primitive variant of the same class</returns>
        public static Type GetPrimitiveType(this Type type)
        {
            if (type == typeof(bool?)) {
                return typeof(bool);
            }

            if (type == typeof(char?)) {
                return typeof(char);
            }

            if (type == typeof(decimal?)) {
                return typeof(decimal);
            }

            if (type == typeof(double?)) {
                return typeof(double);
            }

            if (type == typeof(float?)) {
                return typeof(float);
            }

            if (type == typeof(sbyte?)) {
                return typeof(sbyte);
            }

            if (type == typeof(byte?)) {
                return typeof(byte);
            }

            if (type == typeof(short?)) {
                return typeof(short);
            }

            if (type == typeof(ushort?)) {
                return typeof(ushort);
            }

            if (type == typeof(int?)) {
                return typeof(int);
            }

            if (type == typeof(uint?)) {
                return typeof(uint);
            }

            if (type == typeof(long?)) {
                return typeof(long);
            }

            if (type == typeof(ulong?)) {
                return typeof(ulong);
            }

            return type;
        }

        /// <summary>
        ///     Returns for the class name given the class name of the boxed (wrapped) type if
        ///     the class name is one of the CLR primitive types.
        /// </summary>
        /// <param name="typeName">a type name, a CLR primitive type or other class</param>
        /// <returns>boxed type name if CLR primitive type, or just same class name passed in if not a primitive type</returns>
        public static string GetBoxedTypeName(string typeName)
        {
            if (typeName == typeof(char).FullName) {
                return typeof(char?).FullName;
            }

            if (typeName == typeof(sbyte).FullName) {
                return typeof(sbyte?).FullName;
            }

            if (typeName == typeof(byte).FullName) {
                return typeof(byte?).FullName;
            }

            if (typeName == typeof(short).FullName ||
                typeName == "short") {
                return typeof(short?).FullName;
            }

            if (typeName == typeof(ushort).FullName ||
                typeName == "ushort") {
                return typeof(ushort?).FullName;
            }

            if (typeName == typeof(int).FullName ||
                typeName == "int") {
                return typeof(int?).FullName;
            }

            if (typeName == typeof(uint).FullName ||
                typeName == "uint") {
                return typeof(uint?).FullName;
            }

            if (typeName == typeof(long).FullName ||
                typeName == "long") {
                return typeof(long?).FullName;
            }

            if (typeName == typeof(ulong).FullName ||
                typeName == "ulong") {
                return typeof(ulong?).FullName;
            }

            if (typeName == typeof(float).FullName ||
                typeName == "float") {
                return typeof(float?).FullName;
            }

            if (typeName == typeof(double).FullName ||
                typeName == "double") {
                return typeof(double?).FullName;
            }

            if (typeName == typeof(decimal).FullName ||
                typeName == "decimal") {
                return typeof(decimal?).FullName;
            }

            if (typeName == typeof(bool).FullName ||
                typeName == "bool" ||
                typeName == "bool") {
                return typeof(bool?).FullName;
            }

            if (typeName == typeof(BigInteger).FullName ||
                typeName == "Biginteger" ||
                typeName == "BigInteger" ||
                typeName == "Bigint") {
                return typeof(BigInteger?).FullName;
            }

            if (string.Equals(typeName, "string", StringComparison.OrdinalIgnoreCase)) {
                typeName = typeof(string).FullName;
            }

            return typeName;
        }

        /// <summary>
        ///     Determines whether the specified type is bool.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///     <c>true</c> if the specified type is bool; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTypeBoolean(this Type type)
        {
            return
                type == typeof(bool) ||
                type == typeof(bool?);
        }

        /// <summary>
        ///     Returns true if the type represents a character type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsTypeCharacter(this Type type)
        {
            return
                type == typeof(char) ||
                type == typeof(char?);
        }

        /// <summary>
        ///     Returns true if the type represents a floating point numeric type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsTypeFloatingPoint(this Type type)
        {
            return
                type == typeof(float) ||
                type == typeof(float?) ||
                type == typeof(double) ||
                type == typeof(double?) ||
                type == typeof(decimal) ||
                type == typeof(decimal?)
                ;
        }

        public static bool IsNotTypeInt32(this Type type)
        {
            return type != typeof(int) &&
                   type != typeof(int?);
        }

        public static bool IsTypeInt32(this Type type)
        {
            return type == typeof(int) ||
                   type == typeof(int?);
        }

        public static bool IsTypeInt64(this Type type)
        {
            return type == typeof(long) ||
                   type == typeof(long?);
        }

        public static bool IsTypeInt16(this Type type)
        {
            return type == typeof(short) ||
                   type == typeof(short?);
        }

        public static bool IsTypeDecimal(this Type type)
        {
            return type == typeof(decimal) ||
                   type == typeof(decimal?);
        }

        public static bool IsTypeDouble(this Type type)
        {
            return type == typeof(double) ||
                   type == typeof(double?);
        }

        public static bool IsTypeSingle(this Type type)
        {
            return type == typeof(float) ||
                   type == typeof(float?);
        }

        public static bool IsTypeBigInteger(this Type type)
        {
            return type == typeof(BigInteger) ||
                   type == typeof(BigInteger?);
        }

        /// <summary>
        ///     Determines whether the specified type is integral.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="maxIntegralType">Widest integral type.</param>
        /// <returns>
        ///     <c>true</c> if the specified type is integral; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTypeInteger(
            this Type type,
            Type maxIntegralType)
        {
            int intIndex;
            if (IntegralTable.TryGetValue(type, out intIndex)) {
                int maxIndex;
                if (IntegralTable.TryGetValue(maxIntegralType, out maxIndex)) {
                    return intIndex <= maxIndex;
                }
            }

            return false;
        }

        /// <summary> Determines if the type passed in is one of the integral numeric types.</summary>
        /// <param name="type">to check</param>
        public static bool IsTypeInteger(this Type type)
        {
            return
                type == typeof(int?) ||
                type == typeof(int) ||
                type == typeof(long?) ||
                type == typeof(long) ||
                type == typeof(short?) ||
                type == typeof(short) ||
                type == typeof(sbyte?) ||
                type == typeof(sbyte) ||
                type == typeof(byte?) ||
                type == typeof(byte) ||
                type == typeof(ushort?) ||
                type == typeof(ushort) ||
                type == typeof(uint?) ||
                type == typeof(uint) ||
                type == typeof(ulong?) ||
                type == typeof(ulong);
        }

        /// <summary>
        ///     Determines if the type passed in is one of the integral numeric types.
        /// </summary>
        /// <param name="value">to check</param>
        /// <param name="maxIntegralType">Type of the max integral.</param>
        /// <returns>
        ///     <c>true</c> if [is integral number] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIntegralNumber(
            this object value,
            Type maxIntegralType)
        {
            return value != null && IsTypeInteger(value.GetType(), maxIntegralType);
        }

        /// <summary> Determines if the type passed in is one of the integral numeric types.</summary>
        /// <param name="value">to check</param>
        public static bool IsIntegralNumber(this object value)
        {
            return value != null && IsTypeInteger(value.GetType());
        }

        /// <summary> Determines if the type passed in is one of the numeric types.</summary>
        /// <param name="type">to check</param>
        /// <returns>
        ///     true if numeric, false if not
        /// </returns>
        public static bool IsTypeNumeric(this Type type)
        {
            if (type == null) {
                return false;
            }

            if (type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(double) ||
                type == typeof(float) ||
                type == typeof(decimal) ||
                type == typeof(BigInteger)) {
                return true;
            }

            if (type.IsGenericType) {
                return
                    type == typeof(int?) ||
                    type == typeof(long?) ||
                    type == typeof(decimal?) ||
                    type == typeof(double?) ||
                    type == typeof(float?) ||
                    type == typeof(short?) ||
                    type == typeof(ushort?) ||
                    type == typeof(uint?) ||
                    type == typeof(ulong?) ||
                    type == typeof(sbyte?) ||
                    type == typeof(byte?) ||
                    type == typeof(BigInteger?);
            }

            return
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(uint) ||
                type == typeof(ulong) ||
                type == typeof(sbyte) ||
                type == typeof(byte);
        }

        /// <summary>Determines if the class passed in is one of the numeric classes and not a floating point.</summary>
        /// <param name="type">type to check</param>
        /// <returns>true if numeric and not a floating point, false if not</returns>
        public static bool IsTypeNumericNonFP(this Type type)
        {
            return IsTypeNumeric(type) && !IsTypeFloatingPoint(type);
        }

        /// <summary> Determines if the value passed in is one of the numeric types.</summary>
        /// <param name="value">to check</param>
        /// <returns> true if numeric, false if not</returns>
        public static bool IsNumber(this object value)
        {
            return value != null && IsTypeNumeric(value.GetType());
        }

        /// <summary>
        ///     Determines whether the specified type is void.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///     <c>true</c> if the specified type is void; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTypeVoid(this Type type)
        {
            return
                type == typeof(void) ||
                type == typeof(void);
        }

        /// <summary>
        ///     Returns the coercion type for the 2 numeric types for use in arithmetic.
        ///     Note: byte and short types always result in integer.
        /// </summary>
        /// <param name="typeOne">The first type.</param>
        /// <param name="typeTwo">The second type.</param>
        /// <returns>coerced type</returns>
        /// <throws>  CoercionException if types don'type allow coercion </throws>
        public static Type GetArithmaticCoercionType(
            this Type typeOne,
            Type typeTwo)
        {
            var boxedOne = typeOne.GetBoxedType();
            var boxedTwo = typeTwo.GetBoxedType();

            if (!IsTypeNumeric(boxedOne) || !IsTypeNumeric(boxedTwo)) {
                throw new CoercionException(
                    "Cannot coerce types " + typeOne.CleanName() + " and " + typeTwo.CleanName());
            }

            if (boxedOne == typeof(decimal?) ||
                boxedTwo == typeof(decimal?)) {
                return typeof(decimal?);
            }

            if ((boxedOne == typeof(BigInteger?) && IsFloatingPointClass(boxedTwo)) ||
                (boxedTwo == typeof(BigInteger?) && IsFloatingPointClass(boxedOne))) {
                return typeof(decimal?);
            }

            if (boxedOne == typeof(BigInteger?) ||
                boxedTwo == typeof(BigInteger?)) {
                return typeof(BigInteger?);
            }

            if (boxedOne == typeof(double?) ||
                boxedTwo == typeof(double?)) {
                return typeof(double?);
            }

            if (boxedOne == typeof(float?) && !IsFloatingPointClass(typeTwo)) {
                return typeof(double?);
            }

            if (boxedTwo == typeof(float?) && !IsFloatingPointClass(typeOne)) {
                return typeof(double?);
            }

            if (boxedOne == typeof(float?) ||
                boxedTwo == typeof(float?)) {
                return typeof(float?);
            }

            if (boxedOne == typeof(ulong?) ||
                boxedTwo == typeof(ulong?)) {
                return typeof(ulong?);
            }

            if (boxedOne == typeof(long?) ||
                boxedTwo == typeof(long?)) {
                return typeof(long?);
            }

            if (boxedOne == typeof(uint?) ||
                boxedTwo == typeof(uint?)) {
                return typeof(uint?);
            }

            return typeof(int?);
        }

        public static object CoerceBoxed(
            object numToCoerce,
            Type resultBoxedType)
        {
            resultBoxedType = resultBoxedType.GetBoxedType();

            if (numToCoerce.GetType() == resultBoxedType) {
                return numToCoerce;
            }

            if (resultBoxedType == typeof(double?)) {
                return numToCoerce.AsDouble();
            }

            if (resultBoxedType == typeof(long?)) {
                return numToCoerce.AsInt64();
            }

            if (resultBoxedType == typeof(BigInteger?)) {
                return numToCoerce.AsBigInteger();
            }

            if (resultBoxedType == typeof(decimal?)) {
                return numToCoerce.AsDecimal();
            }

            if (resultBoxedType == typeof(float?)) {
                return numToCoerce.AsFloat();
            }

            if (resultBoxedType == typeof(int?)) {
                return numToCoerce.AsInt32();
            }

            if (resultBoxedType == typeof(short?)) {
                return numToCoerce.AsInt16();
            }

            if (resultBoxedType == typeof(byte?)) {
                return numToCoerce.AsByte();
            }

            throw new ArgumentException("Cannot coerce to number subtype " + resultBoxedType.CleanName());
        }

        public static CodegenExpression CoerceNumberBoxedToBoxedCodegen(
            CodegenExpression exprReturningBoxed,
            Type fromTypeBoxed,
            Type targetTypeBoxed)
        {
            if (fromTypeBoxed == targetTypeBoxed) {
                return exprReturningBoxed;
            }

            if (exprReturningBoxed is CodegenExpressionConstantNull ||
                (exprReturningBoxed is CodegenExpressionConstant exprConstant &&
                 exprConstant.IsNull)) {
                return exprReturningBoxed;
            }

            if (targetTypeBoxed == typeof(double?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedDouble", exprReturningBoxed);
            }
            else if (targetTypeBoxed == typeof(long?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedInt64", exprReturningBoxed);
            }

            else if (targetTypeBoxed == typeof(BigInteger?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBigInteger", exprReturningBoxed);
            }

            else if (targetTypeBoxed == typeof(decimal?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedDecimal", exprReturningBoxed);
            }

            else if (targetTypeBoxed == typeof(float?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedFloat", exprReturningBoxed);
            }
            else if (targetTypeBoxed == typeof(int?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedInt32", exprReturningBoxed);
            }

            else if (targetTypeBoxed == typeof(short?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedInt16", exprReturningBoxed);
            }

            else if (targetTypeBoxed == typeof(byte?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedByte", exprReturningBoxed);
            }

            throw new ArgumentException("Cannot coerce to number subtype " + fromTypeBoxed.CleanName());
        }

        public static CodegenExpression CoerceNumberToBoxedCodegen(
            CodegenExpression expr,
            Type fromType,
            Type targetTypeBoxed)
        {
            if (fromType.CanBeNull()) {
                return CoerceNumberBoxedToBoxedCodegen(expr, fromType, targetTypeBoxed);
            }

            if (expr is CodegenExpressionConstantNull ||
                (expr is CodegenExpressionConstant exprConstant &&
                 exprConstant.IsNull)) {
                return expr;
            }

            if (targetTypeBoxed == typeof(double?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedDouble", expr);
            }

            if (targetTypeBoxed == typeof(long?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedInt64", expr);
            }

            if (targetTypeBoxed == typeof(BigInteger?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBigInteger", expr);
            }

            if (targetTypeBoxed == typeof(decimal?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedDecimal", expr);
            }

            if (targetTypeBoxed == typeof(float?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedFloat", expr);
            }

            if (targetTypeBoxed == typeof(int?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedInt32", expr);
            }

            if (targetTypeBoxed == typeof(short?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedInt16", expr);
            }

            if (targetTypeBoxed == typeof(byte?)) {
                return StaticMethod(typeof(TypeExtensions), "AsBoxedByte", expr);
            }

            throw new ArgumentException("Cannot coerce to number subtype " + targetTypeBoxed.CleanName());
        }

        public static bool IsLongNumber(this object number)
        {
            return
                number is long ||
                number is ulong;
        }

        /// <summary>
        ///     Returns true if the Number instance is a floating point number.
        /// </summary>
        /// <param name="number">to check</param>
        /// <returns>true if number is Float or double type</returns>
        public static bool IsFloatingPointNumber(this object number)
        {
            return
                number is float ||
                number is double ||
                number is decimal;
        }

        /// <summary>
        ///     Returns true if the supplied type is a floating point number.
        /// </summary>
        /// <param name="type">to check</param>
        /// <returns>
        ///     true if primitive or boxed float or double
        /// </returns>
        public static bool IsFloatingPointClass(this Type type)
        {
            return
                type == typeof(float?) ||
                type == typeof(float) ||
                type == typeof(double?) ||
                type == typeof(double)
                ;
        }

        /// <summary>
        ///     Returns for 2 classes to be compared via relational operator the Class type of
        ///     common comparison. The output is always typeof(long?), typeof(double), typeof(String) or typeof(bool)
        ///     depending on whether the passed types are numeric and floating-point.
        ///     Accepts primitive as well as boxed types.
        /// </summary>
        /// <param name="typeOne">The first type.</param>
        /// <param name="typeTwo">The second type.</param>
        /// <returns>
        ///     One of typeof(long?), typeof(double) or typeof(String)
        /// </returns>
        /// <throws>  ArgumentException if the types cannot be compared </throws>
        public static Type GetCompareToCoercionType(
            this Type typeOne,
            Type typeTwo)
        {
            if (typeOne == typeTwo) {
                return typeOne;
            }

            if (typeOne == null) {
                return typeTwo;
            }

            if (typeTwo == null) {
                return typeOne;
            }

            if (typeOne.GetBoxedType() == typeTwo.GetBoxedType()) {
                return typeOne.GetBoxedType();
            }

            if (IsTypeNumeric(typeOne) && IsTypeNumeric(typeTwo)) {
                return GetArithmaticCoercionType(typeOne, typeTwo);
            }

            if (IsArray(typeOne) && IsArray(typeTwo)) {
                return typeof(Array);
            }

            if (!IsBuiltinDataType(typeOne) && !IsBuiltinDataType(typeTwo) && typeOne != typeTwo) {
                return typeof(object);
            }

            throw new CoercionException(
                $"Types cannot be compared: {typeOne.FullName} and {typeTwo.FullName}");
        }

        /// <summary>
        ///     Determines if a number can be coerced upwards to another number class without loss.
        ///     <para>
        ///         Clients must pass in two classes that are numeric types.
        ///     </para>
        ///     <para>
        ///         Any number class can be coerced to double, while only double cannot be coerced to float.
        ///         Any non-floating point number can be coerced to long.
        ///         Integer can be coerced to Byte and Short even though loss is possible, for convenience.
        ///     </para>
        /// </summary>
        /// <param name="numberClassToBeCoerced">the number class to be coerced</param>
        /// <param name="numberClassToCoerceTo">the number class to coerce to</param>
        /// <returns>true if numbers can be coerced without loss, false if not</returns>
        public static bool CanCoerce(
            this Type numberClassToBeCoerced,
            Type numberClassToCoerceTo)
        {
            var boxedFrom = numberClassToBeCoerced.GetBoxedType();
            var boxedTo = numberClassToCoerceTo.GetBoxedType();

            if (!IsTypeNumeric(numberClassToBeCoerced)) {
                throw new CoercionException("Type '" + numberClassToBeCoerced + "' is not a numeric type'");
            }

            if (boxedTo == typeof(float?)) {
                return boxedFrom == typeof(byte?) ||
                       boxedFrom == typeof(sbyte?) ||
                       boxedFrom == typeof(short?) ||
                       boxedFrom == typeof(ushort?) ||
                       boxedFrom == typeof(int?) ||
                       boxedFrom == typeof(uint?) ||
                       boxedFrom == typeof(long?) ||
                       boxedFrom == typeof(ulong?) ||
                       boxedFrom == typeof(float?);
            }

            if (boxedTo == typeof(double?)) {
                return boxedFrom == typeof(byte?) ||
                       boxedFrom == typeof(sbyte?) ||
                       boxedFrom == typeof(short?) ||
                       boxedFrom == typeof(ushort?) ||
                       boxedFrom == typeof(int?) ||
                       boxedFrom == typeof(uint?) ||
                       boxedFrom == typeof(long?) ||
                       boxedFrom == typeof(ulong?) ||
                       boxedFrom == typeof(float?) ||
                       boxedFrom == typeof(double?);
            }

            if (boxedTo == typeof(decimal?)) {
                return boxedFrom == typeof(byte?) ||
                       boxedFrom == typeof(sbyte?) ||
                       boxedFrom == typeof(short?) ||
                       boxedFrom == typeof(ushort?) ||
                       boxedFrom == typeof(int?) ||
                       boxedFrom == typeof(uint?) ||
                       boxedFrom == typeof(long?) ||
                       boxedFrom == typeof(ulong?) ||
                       boxedFrom == typeof(float?) ||
                       boxedFrom == typeof(double?) ||
                       boxedFrom == typeof(decimal?);
            }

            if (boxedTo == typeof(long?)) {
                return boxedFrom == typeof(byte?) ||
                       boxedFrom == typeof(sbyte?) ||
                       boxedFrom == typeof(short?) ||
                       boxedFrom == typeof(ushort?) ||
                       boxedFrom == typeof(int?) ||
                       boxedFrom == typeof(uint?) ||
                       boxedFrom == typeof(long?);
            }

            if (boxedTo == typeof(int?) ||
                boxedTo == typeof(short?) ||
                boxedTo == typeof(ushort?) ||
                boxedTo == typeof(byte?) ||
                boxedTo == typeof(sbyte?)) {
                return boxedFrom == typeof(byte?) ||
                       boxedFrom == typeof(sbyte?) ||
                       boxedFrom == typeof(short?) ||
                       boxedFrom == typeof(ushort?) ||
                       boxedFrom == typeof(int?);
            }

            if (boxedTo == typeof(BigInteger?)) {
                return boxedFrom == typeof(byte?) ||
                       boxedFrom == typeof(sbyte?) ||
                       boxedFrom == typeof(short?) ||
                       boxedFrom == typeof(ushort?) ||
                       boxedFrom == typeof(int?) ||
                       boxedFrom == typeof(uint?) ||
                       boxedFrom == typeof(long?) ||
                       boxedFrom == typeof(ulong?) ||
                       boxedFrom == typeof(float?) ||
                       boxedFrom == typeof(double?) ||
                       boxedFrom == typeof(decimal?);
            }

            throw new CoercionException("Type '" + numberClassToCoerceTo + "' is not a numeric type'");
        }

        /// <summary>
        ///     Returns true if the class passed in is a built-in data type (primitive or wrapper)
        ///     including String.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///     true if built-in data type, or false if not
        /// </returns>
        public static bool IsBuiltinDataType(this Type type)
        {
            if (type == null) {
                return true;
            }

            var typeBoxed = type.GetBoxedType();

            if (IsTypeNumeric(typeBoxed) ||
                IsTypeBoolean(typeBoxed) ||
                IsTypeCharacter(typeBoxed) ||
                type == typeof(string)) {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Returns true if 2 classes are assignment compatible.
        /// </summary>
        /// <param name="invocationType">type to assign from</param>
        /// <param name="declarationType">type to assign to</param>
        /// <returns>
        ///     true if assignment compatible, false if not
        /// </returns>
        public static bool IsAssignmentCompatible(
            this Type invocationType,
            Type declarationType)
        {
            if (invocationType == null) {
                return true;
            }

            if (invocationType == declarationType) {
                return true;
            }

            if (invocationType.IsAssignableFrom(declarationType)) {
                return true;
            }

            if (invocationType.IsValueType) {
                if (declarationType == typeof(object)) {
                    return true;
                }

                var parameterWrapperType = invocationType.GetBoxedType();
                if (parameterWrapperType != null) {
                    if (parameterWrapperType == declarationType) {
                        return true;
                    }
                }
            }

            if (invocationType.GetBoxedType() == declarationType) {
                return true;
            }

            var widenings = MethodResolver.WIDENING_CONVERSIONS.Get(declarationType);
            if (widenings != null) {
                return widenings.Contains(invocationType);
            }

            if (declarationType.IsInterface) {
                if (IsImplementsInterface(invocationType, declarationType)) {
                    return true;
                }
            }

            return TypeExtensions.RecursiveIsSuperClass(invocationType, declarationType);
        }

        /// <summary>
        ///     Determines a common denominator type to which one or more types can be casted or coerced.
        ///     For use in determining the result type in certain expressions (coalesce, case).
        ///     <para>
        ///         Null values are allowed as part of the input and indicate a 'null' constant value
        ///         in an expression tree. Such as value doesn'type have type type and can be ignored in
        ///         determining a result type.
        ///     </para>
        ///     <para>
        ///         For numeric types, determines a coercion type that all types can be converted to
        ///         via the method GetArithmaticCoercionType.
        ///     </para>
        ///     <para>
        ///         Indicates that there is no common denominator type by throwing <see cref="CoercionException" />.
        ///     </para>
        /// </summary>
        /// <param name="types">
        ///     is an array of one or more types, which can be built-in (primitive or wrapper)
        ///     or user types
        /// </param>
        /// <returns>
        ///     common denominator type if type can be found, for use in comparison
        /// </returns>
        /// <throws>  CoercionException </throws>
        public static Type GetCommonCoercionType(IList<Type> types)
        {
            if (types.Count == 0) {
                throw new ArgumentException("Unexpected zero length array");
            }

            if (types.Count == 1) {
                return types[0];
            }

            // Determine if we need boxing escalation
            var boxEscalation = types.Any(t => t == null);

            // Reduce to non-null types
            types = types.Where(t => t != null).ToArray();
            if (types.Count == 0) {
                return null; // only null types, result is null
            }

            if (types.Count == 1) {
                return boxEscalation
                    ? types[0].GetBoxedType()
                    : types[0];
            }

            // if all the types are the same, then return the value
            var typeSet = new HashSet<Type>(types);
            if (typeSet.Count == 1) {
                return boxEscalation
                    ? typeSet.First().GetBoxedType()
                    : typeSet.First();
            }

            // Check if all String
            if (types[0] == typeof(string)) {
                var errorType = types.FirstOrDefault(t => t != typeof(string));
                if (errorType != null) {
                    throw new CoercionException("Cannot coerce to String type " + errorType.CleanName());
                }

                return typeof(string);
            }

            // Convert to boxed types
            types = types
                .Select(t => t.GetBoxedType())
                .ToArray();

            // Check if all bool
            if (types[0] == typeof(bool?)) {
                var errorType = types.FirstOrDefault(t => t != typeof(bool?));
                if (errorType != null) {
                    throw new CoercionException("Cannot coerce to bool type " + errorType.CleanName());
                }

                return typeof(bool?);
            }

            // Check if all char
            if (types[0] == typeof(char?)) {
                var errorType = types.FirstOrDefault(t => t != typeof(char?));
                if (errorType != null) {
                    throw new CoercionException("Cannot coerce to char type " + errorType.CleanName());
                }

                return typeof(char?);
            }

            // handle arrays
            if (types[0].IsArray) {
                var componentType = types[0].GetComponentType();
                var sameComponentType = true;
                for (int ii = 1; ii < types.Count; ii++) {
                    if (!types[ii].IsArray) {
                        throw GetCoercionException(types[0], types[ii]);
                    }
                    var otherComponentType = types[ii].GetComponentType();
                    if (componentType != otherComponentType) {
                        if (componentType.IsPrimitive || otherComponentType.IsPrimitive) {
                            throw GetCoercionException(types[0], types[ii]);
                        }
                        sameComponentType = false;
                    }
                }
                if (sameComponentType) {
                    return types[0];
                }

                return typeof(Array);
            }
            
            // Check if all the same builtin type
            var isAllBuiltinTypes = true;
            var isAllNumeric = true;
            foreach (var type in types) {
                if (!IsTypeNumeric(type) && !IsBuiltinDataType(type)) {
                    isAllBuiltinTypes = false;
                }
            }

            // handle all built-in types
            if (!isAllBuiltinTypes) {
                foreach (var type in types) {
                    if (IsBuiltinDataType(type))
                    {
                        throw GetCoercionException(types[0], type);
                    }

                    if (type != types[0]) {
                        return typeof(object);
                    }
                }

                return types[0];
            }

            // test for numeric
            if (!isAllNumeric) {
                throw new CoercionException("Cannot coerce to numeric type " + types[0].CleanName());
            }

            // Use arithmetic coercion type as the final authority, considering all types
            var result = GetArithmaticCoercionType(types[0], types[1]);
            for (var ii = 2; ii < types.Count; ii++) {
                result = GetArithmaticCoercionType(result, types[ii]);
            }

            return result;
        }

        private static CoercionException GetCoercionException(Type typeA, Type typeB) {
            throw new CoercionException($"Cannot coerce to {typeA.CleanName()} type {typeB.CleanName()}");
        }

        public static string GetSimpleTypeName(this Type type)
        {
            type = type.GetBoxedType();
            if (type == typeof(byte?)) {
                return "byte";
            }

            if (type == typeof(short?)) {
                return "short";
            }

            if (type == typeof(int?)) {
                return "int";
            }

            if (type == typeof(long?)) {
                return "long";
            }

            if (type == typeof(ushort?)) {
                return "ushort";
            }

            if (type == typeof(uint?)) {
                return "uint";
            }

            if (type == typeof(ulong?)) {
                return "ulong";
            }

            if (type == typeof(string)) {
                return "string";
            }

            if (type == typeof(bool?)) {
                return "bool";
            }

            if (type == typeof(char?)) {
                return "char";
            }

            if (type == typeof(float?)) {
                return "float";
            }

            if (type == typeof(double?)) {
                return "double";
            }

            if (type == typeof(decimal?)) {
                return "decimal";
            }

            if (type == typeof(Guid?)) {
                return "guid";
            }

            if (type == typeof(BigInteger?)) {
                return "Bigint";
            }

            return type.FullName;
        }

        public static string GetExtendedTypeName(this Type type)
        {
            type = type.GetBoxedType();
            if (type == typeof(byte?)) {
                return "byte";
            }

            if (type == typeof(short?)) {
                return "short";
            }

            if (type == typeof(int?)) {
                return "integer";
            }

            if (type == typeof(long?)) {
                return "long";
            }

            if (type == typeof(ushort?)) {
                return "ushort";
            }

            if (type == typeof(uint?)) {
                return "uinteger";
            }

            if (type == typeof(ulong?)) {
                return "ulong";
            }

            if (type == typeof(string)) {
                return "string";
            }

            if (type == typeof(bool?)) {
                return "bool";
            }

            if (type == typeof(char?)) {
                return "char";
            }

            if (type == typeof(float?)) {
                return "float";
            }

            if (type == typeof(double?)) {
                return "double";
            }

            if (type == typeof(decimal?)) {
                return "decimal";
            }

            if (type == typeof(Guid?)) {
                return "guid";
            }

            if (type == typeof(BigInteger)) {
                return "Bigint";
            }

            return type.FullName;
        }

        public static Type GetTypeForBuiltin(
            string typeName,
            bool boxed = false)
        {
            typeName = typeName.Trim();

            switch (typeName.ToLower()) {
                case "object":
                    return typeof(object);

                case "string":
                case "varchar":
                case "varchar2":
                    return typeof(string);

                case "bool":
                case "boolean":
                    return boxed
                        ? typeof(bool?)
                        : typeof(bool);

                case "byte":
                    return boxed
                        ? typeof(byte?)
                        : typeof(byte);

                case "char":
                case "character":
                    return boxed
                        ? typeof(char?)
                        : typeof(char);

                case "int16":
                case "short":
                    return boxed
                        ? typeof(short?)
                        : typeof(short);

                case "uint16":
                case "ushort":
                    return boxed
                        ? typeof(ushort?)
                        : typeof(ushort);

                case "int":
                case "int32":
                case "integer":
                    return boxed
                        ? typeof(int?)
                        : typeof(int);

                case "uint":
                case "uint32":
                case "uinteger":
                    return boxed
                        ? typeof(uint?)
                        : typeof(uint);

                case "int64":
                case "long":
                    return boxed
                        ? typeof(long?)
                        : typeof(long);

                case "uint64":
                case "ulong":
                    return boxed
                        ? typeof(ulong?)
                        : typeof(ulong);

                case "double":
                    return boxed
                        ? typeof(double?)
                        : typeof(double);

                case "float":
                case "single":
                    return boxed
                        ? typeof(float?)
                        : typeof(float);

                case "decimal":
                    return boxed
                        ? typeof(decimal?)
                        : typeof(decimal);

                case "guid":
                    return boxed
                        ? typeof(Guid?)
                        : typeof(Guid);

                case "date":
                case "datetime":
                    return boxed
                        ? typeof(DateTime?)
                        : typeof(DateTime);

                case "dto":
                case "datetimeoffet":
                    return boxed
                        ? typeof(DateTimeOffset?)
                        : typeof(DateTimeOffset);

                case "dtx":
                case "datetimeex":
                    return typeof(DateTimeEx);

                case "bigint":
                case "biginteger":
                    return boxed
                        ? typeof(BigInteger?)
                        : typeof(BigInteger);

                case "map":
                    return typeof(IDictionary<string, object>);
            }

            return null;
        }

        /// <summary>
        ///     Returns the boxed class for the given type name, recognizing all primitive and abbreviations,
        ///     uppercase and lowercase.
        ///     <para />
        ///     Recognizes "int" as System.Int32 and "strIng" as System.String, and "Integer" as System.Int32,
        ///     and so on.
        /// </summary>
        /// <param name="typeName">is the name to recognize</param>
        /// <param name="typeResolver">the type resolver</param>
        /// <param name="boxed">if set to <c>true</c> [boxed].</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        /// <returns>
        ///     class
        /// </returns>
        /// <throws>EventAdapterException is throw if the class cannot be identified</throws>
        public static Type GetTypeForSimpleName(
            string typeName,
            TypeResolver typeResolver,
            bool boxed = false,
            bool throwOnError = false)
        {
            typeName = typeName.Trim();

            var builtin = GetTypeForBuiltin(typeName, boxed);
            if (builtin != null) {
                return builtin;
            }

            if (typeResolver != null) {
                try {
                    var clazz = typeResolver.ResolveType(typeName, false);
                    if (clazz != null) {
                        return clazz;
                    }
                }
                catch (TypeLoadException) {
                    if (throwOnError) {
                        throw;
                    }
                }
            }

            var type = ResolveType(typeName, throwOnError);
            if (type == null) {
                return null;
            }

            return boxed ? type.GetBoxedType() : type;
        }

        /// <summary>
        ///     Returns the boxed class for the given type name, recognizing all primitive and abbreviations,
        ///     uppercase and lowercase.
        ///     <para />
        ///     Recognizes "int" as System.Int32 and "strIng" as System.String, and "Integer" as System.Int32,
        ///     and so on.
        /// </summary>
        /// <param name="typeName">is the name to recognize</param>
        /// <param name="boxed">if set to <c>true</c> [boxed].</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        /// <returns>
        ///     class
        /// </returns>
        /// <throws>EventAdapterException is throw if the class cannot be identified</throws>
        public static Type GetTypeForSimpleName(
            string typeName,
            bool boxed = false,
            bool throwOnError = false)
        {
            return GetTypeForSimpleName(typeName, null, boxed, throwOnError);
        }

        public static string GetSimpleNameForType(Type clazz)
        {
            if (clazz == null) {
                return "(null)";
            }

            if (clazz == typeof(string)) {
                return "string";
            }

            var boxed = clazz.GetBoxedType();
            if (boxed == typeof(int?)) {
                return "int";
            }

            if (boxed == typeof(uint?)) {
                return "uint";
            }

            if (boxed == typeof(bool?)) {
                return "boolean";
            }

            if (boxed == typeof(char?)) {
                return "character";
            }

            if (boxed == typeof(decimal?)) {
                return "decimal";
            }

            if (boxed == typeof(double?)) {
                return "double";
            }

            if (boxed == typeof(float?)) {
                return "float";
            }

            if (boxed == typeof(byte?)) {
                return "byte";
            }

            if (boxed == typeof(short?)) {
                return "short";
            }

            if (boxed == typeof(ushort?)) {
                return "ushort";
            }

            if (boxed == typeof(long?)) {
                return "long";
            }

            if (boxed == typeof(ulong?)) {
                return "ulong";
            }

            if (boxed == typeof(Guid?)) {
                return "guid";
            }

            if (boxed == typeof(BigInteger?)) {
                return "Bigint";
            }

            return clazz.Name;
        }

        /// <summary>
        ///     Gets the primitive type for the given name.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        public static Type GetPrimitiveTypeForName(string typeName)
        {
            var typeNameLower = typeName.ToLower();
            if (typeNameLower.StartsWith("nullable<") && typeNameLower.EndsWith(">")) {
                var nestedTypeName = typeNameLower.Substring(9, typeNameLower.Length - 10);
                var nestedType = GetPrimitiveTypeForName(nestedTypeName);
                return typeof(Nullable<>).MakeGenericType(nestedType);
            }
            else if (typeNameLower.StartsWith("system.nullable<") && typeNameLower.EndsWith(">")) {
                var nestedTypeName = typeNameLower.Substring(16, typeNameLower.Length - 17);
                var nestedType = GetPrimitiveTypeForName(nestedTypeName);
                return typeof(Nullable<>).MakeGenericType(nestedType);
            }

            switch (typeNameLower) {
                case "bool":
                case "boolean":
                case "system.boolean":
                    return typeof(bool);

                case "char":
                case "character":
                case "system.character":
                    return typeof(char);

                case "float":
                case "single":
                case "system.single":
                    return typeof(float);

                case "double":
                case "system.double":
                    return typeof(double);

                case "decimal":
                case "system.decimal":
                    return typeof(decimal);

                case "sbyte":
                case "system.sbyte":
                    return typeof(sbyte);

                case "byte":
                case "system.byte":
                    return typeof(byte);

                case "short":
                case "int16":
                case "system.int16":
                    return typeof(short);

                case "ushort":
                case "uint16":
                case "system.uint16":
                    return typeof(ushort);

                case "int":
                case "int32":
                case "system.int32":
                    return typeof(int);

                case "uint":
                case "uint32":
                case "system.uint32":
                    return typeof(uint);

                case "long":
                case "int64":
                case "system.int64":
                    return typeof(long);

                case "ulong":
                case "uint64":
                case "system.uint64":
                    return typeof(ulong);

                case "string":
                case "system.string":
                    return typeof(string);

                case "datetime":
                case "system.datetime":
                    return typeof(DateTime);

                case "dto":
                case "datetimeoffset":
                case "system.datetimeoffset":
                    return typeof(DateTimeOffset);

                case "dtx":
                    return typeof(DateTimeEx);

                case "bigint":
                case "biginteger":
                    return typeof(BigInteger?);

                default:
                    return null;
            }
        }

        public static object Parse(
            Type clazz,
            string text)
        {
            var classBoxed = clazz.GetBoxedType();

            if (classBoxed == typeof(string)) {
                return text;
            }

            if (classBoxed == typeof(char?)) {
                return text[0];
            }

            if (classBoxed == typeof(bool?)) {
                return SimpleTypeParserFunctions.ParseBoolean(text);
            }

            if (classBoxed == typeof(byte?)) {
                return SimpleTypeParserFunctions.ParseByte(text);
            }

            if (classBoxed == typeof(short?)) {
                return SimpleTypeParserFunctions.ParseInt16(text);
            }

            if (classBoxed == typeof(long?)) {
                return SimpleTypeParserFunctions.ParseInt64(text);
            }

            if (classBoxed == typeof(float?)) {
                return SimpleTypeParserFunctions.ParseFloat(text);
            }

            if (classBoxed == typeof(double?)) {
                return SimpleTypeParserFunctions.ParseDouble(text);
            }

            if (classBoxed == typeof(int?)) {
                return SimpleTypeParserFunctions.ParseInt32(text);
            }

            if (classBoxed == typeof(BigInteger?)) {
                return SimpleTypeParserFunctions.ParseBigInteger(text);
            }

            if (classBoxed == typeof(DateTime?)) {
                return SimpleTypeParserFunctions.ParseDateTime(text);
            }

            if (classBoxed == typeof(DateTimeOffset?)) {
                return SimpleTypeParserFunctions.ParseDateTimeOffset(text);
            }

            if (classBoxed == typeof(Guid?)) {
                return SimpleTypeParserFunctions.ParseGuid(text);
            }

            return null;
        }

        public static bool IsImplementsInterface<T>(this Type clazz)
        {
            return TypeExtensions.IsImplementsInterface<T>(clazz);
        }

        /// <summary>
        ///     Method to check if a given class, and its superclasses and interfaces (deep), implement a given interface.
        /// </summary>
        /// <param name="clazz">to check, including all its superclasses and their interfaces and extends</param>
        /// <param name="interfaceClass">is the interface class to look for</param>
        /// <returns>
        ///     true if such interface is implemented by type of the clazz or its superclasses orextends by type interface and
        ///     superclasses (deep check)
        /// </returns>
        public static bool IsImplementsInterface(
            this Type clazz,
            Type interfaceClass)
        {
            return TypeExtensions.IsImplementsInterface(clazz, interfaceClass);
        }

        public static string ResolveAbsoluteTypeName(string assemblyQualifiedTypeName)
        {
            var trueTypeName = ResolveType(assemblyQualifiedTypeName, false);
            if (trueTypeName == null) {
                throw new EPException("unable to determine assembly qualified class for " + assemblyQualifiedTypeName);
            }

            return trueTypeName.AssemblyQualifiedName;
        }

        public static string TryResolveAbsoluteTypeName(string typeName)
        {
            var trueTypeName = ResolveType(typeName, false);
            if (trueTypeName == null) {
                return typeName;
            }

            return trueTypeName.AssemblyQualifiedName;
        }

        /// <summary>
        ///     Resolves a type using the assembly qualified type name.  If the type
        ///     can not be resolved using a simple Type.GetType() [which many can not],
        ///     then the method will check all assemblies in the assembly search path.
        /// </summary>
        /// <param name="typeName">Name of the assembly qualified type.</param>
        /// <param name="assemblySearchPath">The assembly search path.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on missing].</param>
        /// <returns></returns>
        public static Type ResolveType(
            string typeName,
            IEnumerable<Assembly> assemblySearchPath,
            bool throwOnError)
        {
            Exception coreException = null;

            var isHandled = false;

            // as part of the process, we want to unwind type esperized type names
            typeName = typeName.Replace('$', '+');

            if (TypeResolver != null) {
                try {
                    var typeResolverEventArgs = new TypeResolverEventArgs(typeName);
                    var typeResult = TypeResolver.Invoke(typeResolverEventArgs);
                    if (typeResult != null || typeResolverEventArgs.Handled) {
                        return typeResult;
                    }
                }
                catch (Exception e) {
                    coreException = e;
                    isHandled = true;
                }
            }

            if (!isHandled) {
                var typeResult = ResolveTypeInternal(typeName, assemblySearchPath, throwOnError);
                if (typeResult != null) {
                    return typeResult;
                }
            }

            // Type was not found in type of our search points
            if (throwOnError && coreException != null) {
                throw coreException;
            }

            return null;
        }

        public static Type ThrowOrReturnNull(
            Exception error,
            bool throwOnError)
        {
            if (throwOnError) {
                throw error;
            }

            return null;
        }

        public static Type ThrowOrReturnNull(
            string errorMessage,
            bool throwOnError)
        {
            if (throwOnError) {
                throw new TypeLoadException(errorMessage);
            }

            return null;
        }

        private static Type ResolveTypeInternal(
            string typeName,
            IEnumerable<Assembly> assemblySearchPath,
            bool throwOnError)
        {
            var genericFirst = typeName.IndexOf('<');
            if (genericFirst != -1) {
                var genericLast = typeName.LastIndexOf('>');
                if (genericLast > genericFirst) {
                    var genericTypeName = typeName.Substring(0, genericFirst).TrimEnd();
                    var genericTypeList = typeName.Substring(genericFirst + 1, genericLast - genericFirst - 1);

                    // We need to know how many generic arguments we are expected to parse, this helps determine
                    // the signature for the generic type we are looking for.

                    var lex = GrammarHelper.CreateLexer(genericTypeList);
                    var parser = GrammarHelper.CreateParser(new CommonTokenStream(lex));
                    var parseResult = parser.classIdentifierGenericArgsList();
                    var identifiers = parseResult.classIdentifier();

                    // The true genericTypeName must include the name, followed by a backtick and the # of generic
                    // arguments we expect.  For example, IDictionary`2 or IList`1.

                    genericTypeName += $"`{identifiers.Length}";

                    var genericType = ResolveType(genericTypeName, assemblySearchPath, throwOnError);
                    if (genericType == null) {
                        return ThrowOrReturnNull(
                            new TypeLoadException($"unable to resolve generic type '{genericTypeName}'"),
                            throwOnError);
                    }

                    if (!genericType.IsGenericType) {
                        return ThrowOrReturnNull(
                            new TypeLoadException($"type '{genericTypeName}' is not a generic type"),
                            throwOnError);
                    }

                    // Alright, proceeding to the generic arguments...

                    if (identifiers.Length != genericType.GetGenericArguments().Length) {
                        return ThrowOrReturnNull(
                            $"type '{genericTypeName}' received incorrect number of generic arguments",
                            throwOnError);
                    }

                    var genericTypeArgs = identifiers
                        .Select(_ => _.GetText())
                        .Select(
                            _ => {
                                // TODO: Consider the need for handling escaped arguments.  In ASTUtil, there is some handling for unescaping these
                                // values.  We need to move the content into NEsper.Grammar so that it is usable across multiple projects.
                                var result = GetTypeForBuiltin(_, false);
                                return result ?? ResolveType(_, assemblySearchPath, throwOnError);
                            })
                        .ToArray();

                    try {
                        return genericType.MakeGenericType(genericTypeArgs);
                    }
                    catch (TypeLoadException) {
                        if (throwOnError) {
                            throw; // Don't use ThrowOnReturnNull because you will lose the exception stack
                        }

                        return null;
                    }
                }
            }


            Exception coreException = null;

            try {
                return Type.GetType(typeName, true, false);
            }
            catch (Exception e) {
                coreException = e;
            }

            // Search the assembly path to resolve the type

            foreach (var assembly in assemblySearchPath) {
                var type = assembly.GetType(typeName, false, false);
                if (type != null) {
                    return type;
                }
            }

            // Type was not found in type of our search points

            if (throwOnError && coreException != null) {
                throw coreException;
            }

            return null;
        }

        /// <summary>
        ///     Resolves a type using the assembly qualified type name.  If the type
        ///     can not be resolved using a simple Type.GetType() [which many can not],
        ///     then the method will check all assemblies currently loaded into the
        ///     AppDomain.
        /// </summary>
        /// <param name="typeName">Name of the assembly qualified type.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on missing].</param>
        /// <returns></returns>
        public static Type ResolveType(
            string typeName,
            bool throwOnError = true)
        {
            var assemblySearchPath = AssemblySearchPath?.Invoke();
            if (assemblySearchPath == null) {
#if NETCOREAPP3_0_OR_GREATER
                assemblySearchPath = AssemblyLoadContext.Default.Assemblies.ToList();
#else
                assemblySearchPath = AppDomain.CurrentDomain.GetAssemblies();
#endif
            }

            return ResolveType(typeName, assemblySearchPath, throwOnError);
        }

        public static Type ResolveType(
            string typeName,
            string assemblyName)
        {
            if (assemblyName == null) {
                return ResolveType(typeName);
            }

            var assembly = ResolveAssembly(assemblyName);
            if (assembly != null) {
                return assembly.GetType(typeName);
            }

            if (Log.IsWarnEnabled) {
                Log.Warn(
                    "Assembly {0} not found while resolving type: {1}",
                    assemblyName,
                    typeName);
            }

            return null;
        }

        public static Type GetTypeForName(
            string typeName,
            TypeResolver typeResolver)
        {
            return typeResolver.ResolveType(typeName, false);
        }

        private static Type MakeArrayType(
            int arrayRank,
            Type typeInstance)
        {
            for (; arrayRank > 0; arrayRank--) {
                typeInstance = typeInstance.MakeArrayType();
            }

            return typeInstance;
        }

        /// <summary>
        ///     Determines whether the type is usable as an dictionary.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>
        ///     <c>true</c> if [is dictionary type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsOpenDictionary(Type t)
        {
            if (typeof(IDictionary<object, object>).IsAssignableFrom(t)) {
                return true;
            }

            // Look for implementation of the System.Collections.Generic.IDictionary
            // interface.  Once found, we just need to check the key type... the return
            // type is irrelevant.

            foreach (var iface in t.GetInterfaces()) {
                if (iface.IsGenericType) {
                    var baseT1 = iface.GetGenericTypeDefinition();
                    if (baseT1 == BaseGenericDictionary) {
                        var genericParameterTypes = iface.GetGenericArguments();
                        if (genericParameterTypes[0] == typeof(string) ||
                            genericParameterTypes[1] == typeof(object)) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool IsSubclassOrImplementsInterface<T>(this Type extendorOrImplementor)
        {
            return IsSubclassOrImplementsInterface(extendorOrImplementor, typeof(T));
        }

        /// <summary>
        ///     Method to check if a given class, and its superclasses and interfaces (deep), implement a given interface or
        ///     extend a given class.
        /// </summary>
        /// <param name="extendorOrImplementor">is the class to inspects its : and : clauses</param>
        /// <param name="extendedOrImplemented">is the potential interface, or superclass, to check</param>
        /// <returns>
        ///     true if such interface is implemented by type of the clazz or its superclasses orextends by type interface and
        ///     superclasses (deep check)
        /// </returns>
        public static bool IsSubclassOrImplementsInterface(
            Type extendorOrImplementor,
            Type extendedOrImplemented)
        {
            return extendedOrImplemented.IsAssignableFrom(extendorOrImplementor);
        }

        /// <summary>
        ///     Instantiates the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static object Instantiate(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            
            try {
                return Activator.CreateInstance(type);
            }
            catch (TypeInstantiationException ex) {
                throw new TypeInstantiationException(
                    "Unable to instantiate from class '" + type.CleanName() + "' via default constructor",
                    ex);
            }
            catch (TargetInvocationException ex) {
                throw new TypeInstantiationException(
                    "Invocation exception when instantiating class '" +
                    type.CleanName() +
                    "' via default constructor",
                    ex);
            }
            catch (MethodAccessException ex) {
                throw new TypeInstantiationException(
                    "Method access when instantiating class '" + type.CleanName() + "' via default constructor",
                    ex);
            }
            catch (MemberAccessException ex) {
                throw new TypeInstantiationException(
                    "Member access when instantiating class '" + type.CleanName() + "' via default constructor",
                    ex);
            }
        }

        /// <summary>
        ///     Looks up the given class and checks that it : or : the required interface,and instantiates an object.
        /// </summary>
        /// <typeparam name="T">is the type that the looked-up class should extend or implement</typeparam>
        /// <param name="type">of the class to load, check type and instantiate</param>
        /// <returns>instance of given class, via newInstance</returns>
        public static T Instantiate<T>(Type type) where T : class
        {
            var implementedOrExtendedType = typeof(T);
            var typeName = type.FullName;
            var typeNameClean = type.CleanName();

            if (!IsSubclassOrImplementsInterface(type, implementedOrExtendedType)) {
                if (implementedOrExtendedType.IsInterface) {
                    throw new TypeInstantiationException(
                        "Type '" +
                        typeNameClean +
                        "' does not implement interface '" +
                        implementedOrExtendedType.CleanName() +
                        "'");
                }

                throw new TypeInstantiationException(
                    "Type '" +
                    typeNameClean +
                    "' does not extend '" +
                    implementedOrExtendedType.CleanName() +
                    "'");
            }

            try {
                return (T)Activator.CreateInstance(type);
            }
            catch (TypeInstantiationException ex) {
                throw new TypeInstantiationException(
                    "Unable to instantiate from class '" + typeName + "' via default constructor",
                    ex);
            }
            catch (TargetInvocationException ex) {
                throw new TypeInstantiationException(
                    "Invocation exception when instantiating class '" + typeName + "' via default constructor",
                    ex);
            }
            catch (MethodAccessException ex) {
                throw new TypeInstantiationException(
                    "Method access when instantiating class '" + typeName + "' via default constructor",
                    ex);
            }
            catch (MemberAccessException ex) {
                throw new TypeInstantiationException(
                    "Member access when instantiating class '" + typeName + "' via default constructor",
                    ex);
            }
        }

        /// <summary>
        ///     Looks up the given class and checks that it : or : the required interface,and instantiates an object.
        /// </summary>
        /// <typeparam name="T">is the type that the looked-up class should extend or implement</typeparam>
        /// <param name="typeName">of the class to load, check type and instantiate</param>
        /// <returns>instance of given class, via newInstance</returns>
        public static T Instantiate<T>(string typeName) where T : class
        {
            var implementedOrExtendedType = typeof(T);

            Type type;
            try {
                type = ResolveType(typeName);
            }
            catch (Exception ex) {
                throw new TypeInstantiationException("Unable to load class '" + typeName + "', class not found", ex);
            }

            if (!IsSubclassOrImplementsInterface(type, implementedOrExtendedType)) {
                if (implementedOrExtendedType.IsInterface) {
                    throw new TypeInstantiationException(
                        "Class '" +
                        typeName +
                        "' does not implement interface '" +
                        implementedOrExtendedType.CleanName() +
                        "'");
                }

                throw new TypeInstantiationException(
                    "Class '" +
                    typeName +
                    "' does not extend '" +
                    implementedOrExtendedType.CleanName() +
                    "'");
            }

            try {
                return (T)Activator.CreateInstance(type);
            }
            catch (TypeInstantiationException ex) {
                throw new TypeInstantiationException(
                    "Unable to instantiate from class '" + typeName + "' via default constructor",
                    ex);
            }
            catch (TargetInvocationException ex) {
                throw new TypeInstantiationException(
                    "Invocation exception when instantiating class '" + typeName + "' via default constructor",
                    ex);
            }
            catch (MethodAccessException ex) {
                throw new TypeInstantiationException(
                    "Method access when instantiating class '" + typeName + "' via default constructor",
                    ex);
            }
            catch (MemberAccessException ex) {
                throw new TypeInstantiationException(
                    "Member access when instantiating class '" + typeName + "' via default constructor",
                    ex);
            }
        }

        /// <summary>
        ///     Looks up the given class and checks that it : or : the required interface,and instantiates an object.
        /// </summary>
        /// <typeparam name="T">is the type that the looked-up class should extend or implement</typeparam>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="typeResolver">The class for name provider.</param>
        /// <returns>
        ///     instance of given class, via newInstance
        /// </returns>
        public static T Instantiate<T>(
            string typeName,
            TypeResolver typeResolver)
        {
            var implementedOrExtendedType = typeof(T);

            Type type;
            try {
                type = typeResolver.ResolveType(typeName, false);
            }
            catch (Exception ex) {
                throw new TypeInstantiationException("Unable to load class '" + typeName + "', class not found", ex);
            }

            if (!IsSubclassOrImplementsInterface(type, implementedOrExtendedType)) {
                if (implementedOrExtendedType.IsInterface) {
                    throw new TypeInstantiationException(
                        "Type '" +
                        typeName +
                        "' does not implement interface '" +
                        implementedOrExtendedType.CleanName() +
                        "'");
                }

                throw new TypeInstantiationException(
                    "Type '" +
                    typeName +
                    "' does not extend '" +
                    implementedOrExtendedType.CleanName() +
                    "'");
            }

            try {
                return (T)Activator.CreateInstance(type);
            }
            catch (TypeInstantiationException ex) {
                throw new TypeInstantiationException(
                    "Unable to instantiate from class '" + typeName + "' via default constructor",
                    ex);
            }
            catch (TargetInvocationException ex) {
                throw new TypeInstantiationException(
                    "Invocation exception when instantiating class '" + typeName + "' via default constructor",
                    ex);
            }
            catch (MethodAccessException ex) {
                throw new TypeInstantiationException(
                    "Method access when instantiating class '" + typeName + "' via default constructor",
                    ex);
            }
            catch (MemberAccessException ex) {
                throw new TypeInstantiationException(
                    "Member access when instantiating class '" + typeName + "' via default constructor",
                    ex);
            }
        }

        /// <summary>
        ///     Applies a visitor pattern to the base interfaces for the provided type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="visitor">The visitor.</param>
        public static void VisitBaseInterfaces(
            this Type type,
            Action<Type> visitor)
        {
            if (type != null) {
                var interfaces = type.GetInterfaces();
                for (var ii = 0; ii < interfaces.Length; ii++) {
                    visitor.Invoke(interfaces[ii]);
                    VisitBaseInterfaces(interfaces[ii], visitor);
                }
            }
        }

        /// <summary>
        ///     Gets the base interfaces for the provided type and store them
        ///     in the result set.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="result">The result.</param>
        public static void GetBaseInterfaces(
            Type type,
            ICollection<Type> result)
        {
            VisitBaseInterfaces(type, result.Add);
        }

        /// <summary>
        ///     Visits the base classes for the provided type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="visitor">The visitor.</param>
        public static void VisitBaseClasses(
            this Type type,
            Action<Type> visitor)
        {
            if (type != null) {
                var baseType = type.BaseType;
                if (baseType != null) {
                    visitor.Invoke(baseType);
                    VisitBaseClasses(baseType, visitor);
                }
            }
        }

        /// <summary>
        ///     Gets the base classes for the provided type and store them
        ///     in the result set.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="result">The result.</param>
        public static void GetBaseClasses(
            Type type,
            ICollection<Type> result)
        {
            VisitBaseClasses(type, result.Add);
        }

        /// <summary>
        ///     Visits the base class and all interfaces.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="visitor">The visitor.</param>
        public static void VisitBase(
            this Type type,
            Action<Type> visitor)
        {
            VisitBaseInterfaces(type, visitor);
            VisitBaseClasses(type, visitor);
        }

        /// <summary>
        ///     Populates all interface and superclasses for the given class, recursivly.
        /// </summary>
        /// <param name="type">to reflect upon</param>
        /// <param name="result">set of classes to populate</param>
        public static void GetBase(
            Type type,
            ICollection<Type> result)
        {
            GetBaseInterfaces(type, result);
            GetBaseClasses(type, result);
        }

        /// <summary>
        ///     Visits the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="visitor">The visitor.</param>
        public static void Visit(
            this Type type,
            Action<Type> visitor)
        {
            if (type != null) {
                visitor.Invoke(type);
                VisitBase(type, visitor);
            }
        }

        public static bool IsSimpleNameFullyQualfied(
            string simpleTypeName,
            string fullyQualifiedTypename)
        {
            if (fullyQualifiedTypename.EndsWith("." + simpleTypeName) ||
                fullyQualifiedTypename.Equals(simpleTypeName)) {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Returns true if the Class is a fragmentable type, i.e. not a primitive or boxed
        ///     type or type of the common built-in types or does not implement Map.
        /// </summary>
        /// <param name="propertyType">type to check</param>
        /// <returns>
        ///     true if fragmentable
        /// </returns>
        public static bool IsFragmentableType(this Type propertyType)
        {
            if (propertyType == null) {
                return false;
            }

            if (propertyType.IsArray) {
                return IsFragmentableType(propertyType.GetElementType());
            }

            if (propertyType.IsNullable()) {
                propertyType = Nullable.GetUnderlyingType(propertyType);
            }

            if (IsBuiltinDataType(propertyType)) {
                return false;
            }

            if (propertyType.IsEnum) {
                return false;
            }

            if (propertyType.IsGenericDictionary()) {
                return false;
            }

            if (propertyType == typeof(XmlNode)) {
                return false;
            }

            if (propertyType == typeof(XmlNodeList)) {
                return false;
            }

            if (propertyType == typeof(object)) {
                return false;
            }

            if (propertyType == typeof(DateTimeEx)) {
                return false;
            }

            if (propertyType == typeof(DateTimeOffset)) {
                return false;
            }

            if (propertyType == typeof(DateTime)) {
                return false;
            }

            if (propertyType.FullName == AvroConstantsNoDep.GENERIC_RECORD_CLASSNAME) {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Returns the generic type parameter of a return value by a field, property or method.
        /// </summary>
        /// <param name="memberInfo">The member INFO.</param>
        /// <param name="isAllowNull">if set to <c>true</c> [is allow null].</param>
        /// <returns>generic type parameter</returns>
        public static Type GetGenericReturnType(
            MemberInfo memberInfo,
            bool isAllowNull)
        {
            if (memberInfo is MethodInfo methodInfo) {
                return GetGenericReturnType(methodInfo, isAllowNull);
            }

            if (memberInfo is PropertyInfo info) {
                return GetGenericPropertyType(info, isAllowNull);
            }

            return GetGenericFieldType(memberInfo as FieldInfo, isAllowNull);
        }

        /// <summary>
        ///     Returns the second generic type parameter of a return value by a field or
        ///     method.
        /// </summary>
        /// <param name="memberInfo">The member INFO.</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected typeof(object)</param>
        /// <returns>generic type parameter</returns>
        public static Type GetGenericReturnTypeMap(
            MemberInfo memberInfo,
            bool isAllowNull)
        {
            if (memberInfo is MethodInfo methodInfo) {
                return GetGenericReturnTypeMap(methodInfo, isAllowNull);
            }

            if (memberInfo is PropertyInfo info) {
                return GetGenericPropertyTypeMap(info, isAllowNull);
            }

            return GetGenericFieldTypeMap(memberInfo as FieldInfo, isAllowNull);
        }

        /// <summary>
        ///     Returns the generic type parameter of a return value by a method.
        /// </summary>
        /// <param name="method">method or null if field</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected typeof(object)</param>
        /// <returns>
        ///     generic type parameter
        /// </returns>
        public static Type GetGenericReturnType(
            MethodInfo method,
            bool isAllowNull)
        {
            var t = method.ReturnType;
            var result = GetGenericType(t, 0);
            if (!isAllowNull && result == null) {
                return typeof(object);
            }

            return result;
        }

        /// <summary>
        ///     Returns the second generic type parameter of a return value by a field or
        ///     method.
        /// </summary>
        /// <param name="method">method or null if field</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected typeof(object)</param>
        /// <returns>
        ///     generic type parameter
        /// </returns>
        public static Type GetGenericReturnTypeMap(
            MethodInfo method,
            bool isAllowNull)
        {
            var t = method.ReturnType;
            var result = GetGenericMapType(t);
            if (!isAllowNull && result == null) {
                return typeof(object);
            }

            return result;
        }

        /// <summary>
        ///     Returns the generic type parameter of a return value by a property.
        /// </summary>
        /// <param name="property">property or null if method</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected typeof(object)</param>
        /// <returns>
        ///     generic type parameter
        /// </returns>
        public static Type GetGenericPropertyType(
            PropertyInfo property,
            bool isAllowNull)
        {
            var t = property.PropertyType;
            var result = GetGenericType(t, 0);
            if (!isAllowNull && result == null) {
                return typeof(object);
            }

            return result;
        }

        /// <summary>
        ///     Returns the generic type parameter of a return value by a property.
        /// </summary>
        /// <param name="property">property or null if method</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected typeof(object)</param>
        /// <returns>
        ///     generic type parameter
        /// </returns>
        public static Type GetGenericPropertyTypeMap(
            PropertyInfo property,
            bool isAllowNull)
        {
            var t = property.PropertyType;
            var result = GetGenericMapType(t);
            if (!isAllowNull && result == null) {
                return typeof(object);
            }

            return result;
        }

        /// <summary>
        ///     Returns the generic type parameter of a return value by a field.
        /// </summary>
        /// <param name="field">field or null if method</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected typeof(object)</param>
        /// <returns>
        ///     generic type parameter
        /// </returns>
        public static Type GetGenericFieldType(
            FieldInfo field,
            bool isAllowNull)
        {
            var t = field.FieldType;
            var result = GetGenericType(t, 0);
            if (!isAllowNull && result == null) {
                return typeof(object);
            }

            return result;
        }

        /// <summary>
        ///     Returns the generic type parameter of a return value by a field or method.
        /// </summary>
        /// <param name="field">field or null if method</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected typeof(object)</param>
        /// <returns>
        ///     generic type parameter
        /// </returns>
        public static Type GetGenericFieldTypeMap(
            FieldInfo field,
            bool isAllowNull)
        {
            var t = field.FieldType;
            var result = GetGenericMapType(t);
            if (!isAllowNull && result == null) {
                return typeof(object);
            }

            return result;
        }

        /// <summary>Returns the generic type parameter of a return value by a field or method. </summary>
        /// <param name="method">method or null</param>
        /// <param name="field">field or null</param>
        /// <param name="property">property or null</param>
        /// <param name="isAllowNull">whether null is allowed as a return value or expected typeof(Object)</param>
        /// <returns>generic type parameter</returns>
        public static Type GetGenericReturnType(
            MethodInfo method,
            FieldInfo field,
            PropertyInfo property,
            bool isAllowNull)
        {
            if (method != null) {
                return GetGenericReturnType(method, isAllowNull);
            }
            else if (property != null) {
                return GetGenericPropertyType(property, isAllowNull);
            }
            else {
                return GetGenericFieldType(field, isAllowNull);
            }
        }

        private static Type GetGenericMapType(this Type t)
        {
            if (t == null) {
                return null;
            }

            if (!t.IsGenericType) {
                // See if we are a dictionary
                var dictionaryType = t.FindGenericDictionaryInterface();
                if (dictionaryType != null) {
                    t = dictionaryType;
                }
            }

            if (!t.IsGenericType) {
                return null;
            }

            // See if we're dealing with a nullable ... nullables register
            // as generics.
            if (Nullable.GetUnderlyingType(t) != null) {
                return null;
            }

            var genericArguments = t.GetGenericArguments();
            if (genericArguments == null ||
                genericArguments.Length < 2) {
                return typeof(object);
            }

            return genericArguments[1];
        }

        public static Type GetGenericType(
            this Type t,
            int index)
        {
            if (t == null) {
                return null;
            }

            if (!t.IsGenericType) {
                var enumType = t.FindGenericEnumerationInterface();
                if (enumType != null) {
                    t = enumType;
                }
            }

            if (!t.IsGenericType) {
                return null;
            }

            // See if we're dealing with a nullable ... nullables register
            // as generics.
            if (Nullable.GetUnderlyingType(t) != null) {
                return null;
            }

            var genericArguments = t.GetGenericArguments();
            if (genericArguments == null ||
                genericArguments.Length < index + 1) {
                return typeof(object);
            }

            return genericArguments[index];
        }

        public static Assembly ResolveAssembly(string assemblyNameOrFile)
        {
            try {
                return Assembly.Load(assemblyNameOrFile);
            }
            catch (FileNotFoundException) {
            }
            catch (FileLoadException) {
            }

            try {
                var fileInfo = new FileInfo(assemblyNameOrFile);
                if (fileInfo.Exists) {
                    return Assembly.LoadFile(fileInfo.FullName);
                }
            }
            catch (FileLoadException) {
            }

            return null;
        }

        public static Type GetArrayType(Type resultType)
        {
            return resultType.MakeArrayType();
        }

        public static Type GetArrayType(
            Type resultType,
            int arrayDimensions)
        {
            if (resultType == null) {
                return null;
            }

            if (arrayDimensions == 0) {
                return resultType;
            }

            // Okay, so technically there are two ways to represent nested arrays.  The first is with the annotation T[][][] which most people
            // coming from Java are familiar with.  In C#, we can also declare arrays as T[,,] which is a different type.  The goal of this call
            // is to generate the former.  To do that, we must declare each type outward until we have the array of the type we really want.

            for (var ii = 0; ii < arrayDimensions; ii++) {
                resultType = resultType.MakeArrayType();
            }

            return resultType;
        }

        public static string PrintInstance(
            object instance,
            bool fullyQualified)
        {
            if (instance == null) {
                return "(null)";
            }

            using (var writer = new StringWriter()) {
                WriteInstance(writer, instance, fullyQualified);
                return writer.ToString();
            }
        }

        public static void WriteInstance(
            TextWriter writer,
            object instance,
            bool fullyQualified)
        {
            if (instance == null) {
                writer.Write("(null)");
                return;
            }

            var className = fullyQualified ? instance.GetType().FullName : instance.GetType().Name;
            WriteInstance(writer, className, instance);
        }

        public static void WriteInstance(
            TextWriter writer,
            string title,
            object instance)
        {
            writer.Write(title);
            writer.Write("@");
            if (instance == null) {
                writer.Write("(null)");
            }
            else {
                writer.Write("{0:X}", RuntimeHelpers.GetHashCode(instance));
            }
        }

        /// <summary>Returns an instance of a hook as specified by an annotation. </summary>
        /// <param name="annotations">to search</param>
        /// <param name="hookType">type to look for</param>
        /// <param name="interfaceExpected">interface required</param>
        /// <returns>hook instance</returns>
        /// <throws>ExprValidationException if instantiation failed</throws>
        public static object GetAnnotationHook(
            Attribute[] annotations,
            HookType hookType,
            Type interfaceExpected)
        {
            if (annotations == null) {
                return null;
            }

            string hookClass = null;
            for (var i = 0; i < annotations.Length; i++) {
                if (!(annotations[i] is HookAttribute)) {
                    continue;
                }

                var hook = (HookAttribute)annotations[i];
                if (hook.HookType != hookType) {
                    continue;
                }

                hookClass = hook.Hook;
            }

            if (hookClass == null) {
                return null;
            }

            Type clazz;
            try {
                clazz = ResolveType(hookClass);
            }
            catch (Exception e) {
                throw new ExprValidationException(
                    "Failed to resolve hook provider of hook type '" +
                    hookType +
                    "' import '" +
                    hookClass +
                    "' :" +
                    e.Message);
            }

            if (!IsImplementsInterface(clazz, interfaceExpected)) {
                throw new ExprValidationException(
                    "Hook provider for hook type '" +
                    hookType +
                    "' " +
                    "class '" +
                    clazz.CleanName() +
                    "' does not implement the required '" +
                    interfaceExpected.CleanName() +
                    "' interface");
            }

            try {
                return Activator.CreateInstance(clazz);
            }
            catch (Exception e) {
                throw new ExprValidationException(
                    "Failed to instantiate hook provider of hook type '" +
                    hookType +
                    "' " +
                    "class '" +
                    clazz.CleanName() +
                    "' :" +
                    e.Message);
            }
        }

        public static string GetInvocationMessage(
            string statementName,
            MethodInfo method,
            string classOrPropertyName,
            object[] args,
            Exception e)
        {
            var parameters =
                args == null ? "null" :
                args.Length == 0 ? "[]" :
                string.Join(" ", args);

            if (args != null) {
                var methodParameters = method.GetParameterTypes();
                for (var i = 0; i < methodParameters.Length; i++) {
                    if (methodParameters[i].IsValueType && args[i] == null) {
                        return "NullPointerException invoking method '" +
                               method.Name +
                               "' of class '" +
                               classOrPropertyName +
                               "' in parameter " +
                               i +
                               " passing parameters " +
                               parameters +
                               " for statement '" +
                               statementName +
                               "': The method expects a primitive " +
                               methodParameters[i].Name +
                               " value but received a null value";
                    }
                }
            }

            return "Invocation exception when invoking method '" +
                   method.Name +
                   "' of class '" +
                   classOrPropertyName +
                   "' passing parameters " +
                   parameters +
                   " for statement '" +
                   statementName +
                   "': " +
                   e.GetType().CleanName() +
                   " : " +
                   e.Message;
        }

        public static string GetMessageInvocationTarget(
            string statementName,
            MethodInfo method,
            string classOrPropertyName,
            object[] args,
            Exception e)
        {
            return GetMessageInvocationTarget(
                statementName,
                method.Name,
                method.GetParameterTypes(),
                classOrPropertyName,
                args,
                e);
        }

        public static string GetMessageInvocationTarget(
            string statementName,
            string methodName,
            Type[] methodParameters,
            string classOrPropertyName,
            object[] args,
            Exception e)
        {
            if (e is TargetInvocationException) {
                if (e.InnerException != null) {
                    e = e.InnerException;
                }
            }

            var parameters = args == null ? "null" : args.Render(",", "[]");
            if (args != null) {
                for (var i = 0; i < methodParameters.Length; i++) {
                    if (methodParameters[i].IsValueType && args[i] == null) {
                        return "NullPointerException invoking method '" +
                               methodName +
                               "' of class '" +
                               classOrPropertyName +
                               "' in parameter " +
                               i +
                               " passing parameters " +
                               parameters +
                               " for statement '" +
                               statementName +
                               "': The method expects a primitive " +
                               methodParameters[i].Name +
                               " value but received a null value";
                    }
                }
            }

            return "Invocation exception when invoking method '" +
                   methodName +
                   "' of class '" +
                   classOrPropertyName +
                   "' passing parameters " +
                   parameters +
                   " for statement '" +
                   statementName +
                   "': " +
                   e.GetType().CleanName() +
                   " : " +
                   e.Message;
        }

        public static IDictionary<string, object> GetClassObjectFromPropertyTypeNames(
            Properties properties,
            Func<string, Type> typeResolver)
        {
            IDictionary<string, object> propertyTypes = new Dictionary<string, object>();
            foreach (var entry in properties) {
                propertyTypes.Put(entry.Key, typeResolver.Invoke(entry.Value));
            }

            return propertyTypes;
        }

        public static IDictionary<string, object> GetClassObjectFromPropertyTypeNames(
            Properties properties,
            TypeResolver typeResolver)
        {
            return GetClassObjectFromPropertyTypeNames(
                properties,
                _ => typeResolver.ResolveType(_, false));
        }

        public static IDictionary<string, object> GetClassObjectFromPropertyTypeNames(Properties properties)
        {
            return GetClassObjectFromPropertyTypeNames(
                properties,
                className => {
                    if (className == "string") {
                        className = typeof(string).FullName;
                    }

                    // use the boxed type for primitives
                    var boxedClassName = GetBoxedTypeName(className);

                    try {
                        return ResolveType(boxedClassName, true);
                    }
                    catch (TypeLoadException ex) {
                        throw new ConfigurationException(
                            "Unable to load class '" + boxedClassName + "', class not found",
                            ex);
                    }
                });
        }

        public static object Boxed(this object value)
        {
            if (value == null) {
                return null;
            }

            if (value is Type type) {
                return type.GetBoxedType();
            }

            return value;
        }

        public static bool IsObjectCollectionCompatible(this object value)
        {
            if (value == null) {
                return false;
            }

            if (value is ICollection<object>) {
                return true;
            }

            if (value.GetType().IsGenericCollection()) {
                return true;
            }

            if (value is ICollection) {
                return true;
            }

            return false;
        }

        public static ICollection<object> AsObjectCollection(this object value)
        {
            if (value == null) {
                return null;
            }

            if (value is ICollection<object> objects) {
                return objects;
            }

            if (value.GetType().IsGenericCollection()) {
                return MagicMarker.SingletonInstance.GetCollectionFactory(value.GetType()).Invoke(value);
            }

            if (value is ICollection collection) {
                return collection.Cast<object>().ToList();
            }

            throw new ArgumentException("value is not a collection", "value");
        }

        public static bool IsArray(this Type type)
        {
            return type == typeof(Array) || type.IsArray;
        }

        public static bool IsSignatureCompatible(
            Type[] one,
            Type[] two)
        {
            if (one == two) {
                return true;
            }

            if (one.Length != two.Length) {
                return false;
            }

            for (var i = 0; i < one.Length; i++) {
                var oneClass = one[i];
                var twoClass = two[i];
                if (oneClass != twoClass && !oneClass.IsAssignmentCompatible(twoClass)) {
                    return false;
                }
            }

            return true;
        }

        public static MethodInfo FindRequiredMethod(
            Type clazz,
            string methodName)
        {
            var found = clazz.GetMethods().FirstOrDefault(m => m.Name == methodName);
            if (found == null) {
                throw new ArgumentException("Not found method '" + methodName + "'");
            }

            return found;
        }

        public static IList<MethodInfo> FindMethodsByNameStartsWith(
            Type clazz,
            string methodName)
        {
            var methods = clazz.GetMethods();
            return methods.Where(method => method.Name.StartsWith(methodName)).ToList();
        }

        public static IList<T> GetAnnotations<T>(Attribute[] annotations) where T : Attribute
        {
            return annotations
                .Where(a => a.GetType() == typeof(T))
                .Select(a => (T)a)
                .ToList();
        }

        public static IList<Attribute> GetAnnotations(
            Type annotationClass,
            Attribute[] annotations)
        {
            return annotations
                .Where(a => a.GetType() == annotationClass)
                .ToList();
        }

        public static bool IsAnnotationListed(
            Type annotationClass,
            Attribute[] annotations)
        {
            return !GetAnnotations(annotationClass, annotations).IsEmpty();
        }

        public static ICollection<FieldInfo> FindAnnotatedFields(
            Type targetClass,
            Type annotation)
        {
            var fields = new LinkedHashSet<FieldInfo>();
            FindFieldInternal(targetClass, annotation, fields);

            // superclass fields
            var clazz = targetClass;
            while (true) {
                clazz = clazz.BaseType;
                if (clazz == typeof(object) || clazz == null) {
                    break;
                }

                FindFieldInternal(clazz, annotation, fields);
            }

            return fields;
        }

        private static void FindFieldInternal(
            Type currentClass,
            Type annotation,
            ICollection<FieldInfo> fields)
        {
            foreach (var field in currentClass.GetFields(
                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                if (IsAnnotationListed(annotation, field.GetCustomAttributes(true).OfType<Attribute>().ToArray())) {
                    fields.Add(field);
                }
            }
        }

        public static ICollection<MethodInfo> FindAnnotatedMethods(
            Type targetClass,
            Type annotation)
        {
            ICollection<MethodInfo> methods = new LinkedHashSet<MethodInfo>();
            FindAnnotatedMethodsInternal(targetClass, annotation, methods);

            // superclass fields
            var clazz = targetClass;
            while (true) {
                clazz = clazz.BaseType;
                if (clazz == typeof(object) || clazz == null) {
                    break;
                }

                FindAnnotatedMethodsInternal(clazz, annotation, methods);
            }

            return methods;
        }

        private static void FindAnnotatedMethodsInternal(
            Type currentClass,
            Type annotation,
            ICollection<MethodInfo> methods)
        {
            foreach (var method in currentClass.GetMethods(
                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                if (IsAnnotationListed(annotation, method.GetCustomAttributes(true).OfType<Attribute>().ToArray())) {
                    methods.Add(method);
                }
            }
        }

        public static void SetFieldForAnnotation(
            object target,
            Type annotation,
            object value)
        {
            var found = SetFieldForAnnotation(target, annotation, value, target.GetType());
            if (!found) {
                var superClass = target.GetType().BaseType;
                while (!found) {
                    found = SetFieldForAnnotation(target, annotation, value, superClass);
                    if (!found) {
                        superClass = superClass.BaseType;
                    }

                    if (superClass == typeof(object) || superClass == null) {
                        break;
                    }
                }
            }
        }

        private static bool SetFieldForAnnotation(
            object target,
            Type annotation,
            object value,
            Type currentClass)
        {
            foreach (var field in currentClass.GetFields(
                         BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                if (IsAnnotationListed(annotation, field.GetCustomAttributes(true).OfType<Attribute>().ToArray())) {
                    field.SetValue(target, value);
                    return true;
                }
            }

            return false;
        }

        public static Pair<string, bool> IsGetArrayType(string type)
        {
            var index = type.IndexOf("[]", StringComparison.Ordinal);
            if (index == -1) {
                return new Pair<string, bool>(type, false);
            }

            var typeOnly = type.Substring(0, index);
            return new Pair<string, bool>(typeOnly.Trim(), true);
        }

        public static bool Is<T>(this object o)
        {
            return o is T;
        }

        public static bool Is<T1, T2>(this object o)
        {
            return o is T1 || o is T2;
        }

        public static bool Is<T1, T2, T3>(this object o)
        {
            return o is T1 || o is T2 || o is T3;
        }

        public static bool IsAttribute(this Type type)
        {
            return IsSubclassOrImplementsInterface<Attribute>(type);
        }

        public static bool IsDateTime(Type clazz)
        {
            if (clazz == null) {
                return false;
            }

            var clazzBoxed = clazz.GetBoxedType();
            return
                clazzBoxed == typeof(DateTimeEx) ||
                clazzBoxed == typeof(DateTimeOffset?) ||
                clazzBoxed == typeof(DateTime?) ||
                clazzBoxed == typeof(long?);
        }

        private static string Unescape(string name)
        {
            if (name.StartsWith("`") && name.EndsWith("`")) {
                return name.Substring(1, name.Length - 2);
            }

            return name;
        }

        public static Type GetComponentTypeOutermost(this Type clazz)
        {
            if (!clazz.IsArray) {
                return clazz;
            }

            return GetComponentTypeOutermost(clazz.GetElementType());
        }

        public static int GetNumberOfDimensions(this Type clazz)
        {
            if (clazz.GetElementType() == null) {
                return 0;
            }

            return GetNumberOfDimensions(clazz.GetElementType()) + 1;
        }

        /// <summary>
        ///     Determines whether the specified type is delegate.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsDelegate(this Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type.BaseType);
        }

        /// <summary>
        ///     Determines whether [is collection map or array] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsCollectionMapOrArray(this Type type)
        {
            return type != null && (type.IsGenericCollection() || type.IsGenericDictionary() || IsArray(type));
        }

        /// <summary>
        ///     Determines whether the target type and provided type are compatible in an array.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="provided">The provided.</param>
        /// <returns></returns>
        public static bool IsArrayTypeCompatible(
            this Type target,
            Type provided)
        {
            if (target == provided || target == typeof(object)) {
                return true;
            }

            var targetBoxed = target.GetBoxedType();
            var providedBoxed = provided.GetBoxedType();
            return targetBoxed == providedBoxed || IsSubclassOrImplementsInterface(providedBoxed, targetBoxed);
        }

        public static Type GetArrayComponentTypeInnermost(Type clazz)
        {
            if (clazz.IsArray) {
                return GetArrayComponentTypeInnermost(clazz.GetElementType());
            }

            return clazz;
        }

        /// <summary>
        ///     Returns the esper name for a type.  These names should be used when
        ///     referring to a type in a stream.  Normally, this is just the standard
        ///     type name.  However, for nested classes, we convert the '+' which is
        ///     embedded in the name into a '$' - this prevents the parser from being
        ///     unable to determine the difference between A+B which is an additive
        ///     function and A+B where B is a nested class of A.
        /// </summary>
        /// <returns></returns>
        public static string MaskTypeName<T>()
        {
            return MaskTypeName(typeof(T));
        }

        /// <summary>
        ///     Returns the esper name for a type.  These names should be used when
        ///     referring to a type in a stream.  Normally, this is just the standard
        ///     type name.  However, for nested classes, we convert the '+' which is
        ///     embedded in the name into a '$' - this prevents the parser from being
        ///     unable to determine the difference between A+B which is an additive
        ///     function and A+B where B is a nested class of A.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static string MaskTypeName(this Type type)
        {
            return type.FullName.Replace('+', '$');
        }

        public static string MaskTypeName(string typename)
        {
            return typename.Replace('+', '$');
        }

        /// <summary>
        ///     Unmasks the name of the stream.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string UnmaskTypeName(this string name)
        {
            return name.Replace('$', '+');
        }

        public static string GetDefaultTypeName(this Type type)
        {
#if false
            if (type.IsNullable())
            {
                // Nullables have been causing problems because their names arent properly
                // unique.  We generally dont want nullables automatically binding to public
                // namespaces, so be sure to provide an opaque name when they are automatically
                // bound so we can find them.

                return string.Format("__NULLABLE_{0}", Nullable.GetUnderlyingType(type).Name);
            }
#endif

            return type.FullName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNullTypeSafe(Type type)
        {
            return type == null;
        }

        public static bool IsTypeOrNull(
            Type type,
            Type expected)
        {
            if (IsNullTypeSafe(type)) {
                return true;
            }

            if (type == expected) {
                return true;
            }

            return type.GetBoxedType() == expected;
        }
    }

    public class TypeResolverEventArgs : EventArgs
    {
        /// <summary>
        ///     Constructs an event args object.
        /// </summary>
        /// <param name="typeName"></param>
        public TypeResolverEventArgs(string typeName)
        {
            TypeName = typeName;
            Handled = false;
        }

        public string TypeName { get; }
        public bool Handled { get; }
    }
}