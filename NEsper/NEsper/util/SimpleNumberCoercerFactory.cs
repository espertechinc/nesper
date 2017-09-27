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
using com.espertech.esper.compat.logging;

using java.math;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Factory for conversion/coercion and widening implementations for numbers.
    /// </summary>
    public class SimpleNumberCoercerFactory {
        private static SimpleNumberCoercerNull nullCoerce = new SimpleNumberCoercerNull();
        private static SimpleNumberCoercerDouble doubleCoerce = new SimpleNumberCoercerDouble();
        private static SimpleNumberCoercerLong longCoerce = new SimpleNumberCoercerLong();
        private static SimpleNumberCoercerFloat floatCoerce = new SimpleNumberCoercerFloat();
        private static SimpleNumberCoercerInt intCoerce = new SimpleNumberCoercerInt();
        private static SimpleNumberCoercerShort shortCoerce = new SimpleNumberCoercerShort();
        private static SimpleNumberCoercerByte byteCoerce = new SimpleNumberCoercerByte();
        private static SimpleNumberCoercerBigInt bigIntCoerce = new SimpleNumberCoercerBigInt();
        private static SimpleNumberCoercerBigIntNull bigIntCoerceNull = new SimpleNumberCoercerBigIntNull();
        private static SimpleNumberCoercerBigDecLong bigDecCoerceLong = new SimpleNumberCoercerBigDecLong();
        private static SimpleNumberCoercerBigDecDouble bigDecCoerceDouble = new SimpleNumberCoercerBigDecDouble();
        private static SimpleNumberCoercerBigDecNull bigDecCoerceNull = new SimpleNumberCoercerBigDecNull();
    
        /// <summary>
        /// Returns a coercer/widener to BigDecimal for a given type.
        /// </summary>
        /// <param name="fromType">to widen</param>
        /// <returns>widener</returns>
        public static SimpleNumberBigDecimalCoercer GetCoercerBigDecimal(Type fromType) {
            if (fromType == Typeof(BigDecimal)) {
                return bigDecCoerceNull;
            }
            if (JavaClassHelper.IsFloatingPointClass(fromType)) {
                return bigDecCoerceDouble;
            }
            return bigDecCoerceLong;
        }
    
        /// <summary>
        /// Returns a coercer/widener to BigInteger for a given type.
        /// </summary>
        /// <param name="fromType">to widen</param>
        /// <returns>widener</returns>
        public static SimpleNumberBigIntegerCoercer GetCoercerBigInteger(Type fromType) {
            if (fromType == Typeof(BigInteger)) {
                return bigIntCoerceNull;
            }
            return bigIntCoerce;
        }
    
        /// <summary>
        /// Returns a coercer/widener/narrower to a result number type from a given type.
        /// </summary>
        /// <param name="fromType">to widen/narrow, can be null to indicate that no shortcut-coercer is used</param>
        /// <param name="resultBoxedType">type to widen/narrow to</param>
        /// <returns>widener/narrower</returns>
        public static SimpleNumberCoercer GetCoercer(Type fromType, Type resultBoxedType) {
            if (fromType == resultBoxedType) {
                return nullCoerce;
            }
            if (resultBoxedType == Typeof(double?)) {
                return doubleCoerce;
            }
            if (resultBoxedType == Typeof(long)) {
                return longCoerce;
            }
            if (resultBoxedType == Typeof(Float)) {
                return floatCoerce;
            }
            if (resultBoxedType == Typeof(int?)) {
                return intCoerce;
            }
            if (resultBoxedType == Typeof(Short)) {
                return shortCoerce;
            }
            if (resultBoxedType == Typeof(Byte)) {
                return byteCoerce;
            }
            if (resultBoxedType == Typeof(BigInteger)) {
                return bigIntCoerce;
            }
            if (resultBoxedType == Typeof(BigDecimal)) {
                if (JavaClassHelper.IsFloatingPointClass(fromType)) {
                    return bigDecCoerceDouble;
                }
                return bigDecCoerceLong;
            }
            throw new IllegalArgumentException("Cannot coerce to number subtype " + resultBoxedType.Name);
        }
    
        private static class SimpleNumberCoercerNull : SimpleNumberCoercer {
            public Number CoerceBoxed(Number numToCoerce) {
                return numToCoerce;
            }
    
            public Type GetReturnType() {
                return Typeof(Number);
            }
        }
    
        private static class SimpleNumberCoercerDouble : SimpleNumberCoercer {
            public Number CoerceBoxed(Number numToCoerce) {
                return NumToCoerce.DoubleValue();
            }
    
            public Type GetReturnType() {
                return Typeof(double?);
            }
        }
    
        private static class SimpleNumberCoercerLong : SimpleNumberCoercer {
            public Number CoerceBoxed(Number numToCoerce) {
                return NumToCoerce.LongValue();
            }
    
            public Type GetReturnType() {
                return Typeof(long);
            }
        }
    
        private static class SimpleNumberCoercerInt : SimpleNumberCoercer {
            public Number CoerceBoxed(Number numToCoerce) {
                return NumToCoerce.IntValue();
            }
    
            public Type GetReturnType() {
                return Typeof(int?);
            }
        }
    
        private static class SimpleNumberCoercerFloat : SimpleNumberCoercer {
            public Number CoerceBoxed(Number numToCoerce) {
                return NumToCoerce.FloatValue();
            }
    
            public Type GetReturnType() {
                return Typeof(Float);
            }
        }
    
        private static class SimpleNumberCoercerShort : SimpleNumberCoercer {
            public Number CoerceBoxed(Number numToCoerce) {
                return NumToCoerce.ShortValue();
            }
    
            public Type GetReturnType() {
                return Typeof(Short);
            }
        }
    
        private static class SimpleNumberCoercerByte : SimpleNumberCoercer {
            public Number CoerceBoxed(Number numToCoerce) {
                return NumToCoerce.ByteValue();
            }
    
            public Type GetReturnType() {
                return Typeof(Byte);
            }
        }
    
        private static class SimpleNumberCoercerBigInt : SimpleNumberCoercer, SimpleNumberBigIntegerCoercer {
            public Number CoerceBoxed(Number numToCoerce) {
                return BigInteger.ValueOf(numToCoerce.LongValue());
            }
    
            public BigInteger CoerceBoxedBigInt(Number numToCoerce) {
                return BigInteger.ValueOf(numToCoerce.LongValue());
            }
    
            public Type GetReturnType() {
                return Typeof(long);
            }
        }
    
        private static class SimpleNumberCoercerBigDecLong : SimpleNumberCoercer, SimpleNumberBigDecimalCoercer {
            public Number CoerceBoxed(Number numToCoerce) {
                return new BigDecimal(numToCoerce.LongValue());
            }
    
            public BigDecimal CoerceBoxedBigDec(Number numToCoerce) {
                return new BigDecimal(numToCoerce.LongValue());
            }
    
            public Type GetReturnType() {
                return Typeof(long);
            }
        }
    
        private static class SimpleNumberCoercerBigDecDouble : SimpleNumberCoercer, SimpleNumberBigDecimalCoercer {
            public Number CoerceBoxed(Number numToCoerce) {
                return new BigDecimal(numToCoerce.DoubleValue());
            }
    
            public BigDecimal CoerceBoxedBigDec(Number numToCoerce) {
                return new BigDecimal(numToCoerce.DoubleValue());
            }
    
            public Type GetReturnType() {
                return Typeof(double?);
            }
        }
    
        private static class SimpleNumberCoercerBigIntNull : SimpleNumberCoercer, SimpleNumberBigIntegerCoercer {
            public Number CoerceBoxed(Number numToCoerce) {
                return numToCoerce;
            }
    
            public BigInteger CoerceBoxedBigInt(Number numToCoerce) {
                return (BigInteger) numToCoerce;
            }
    
            public Type GetReturnType() {
                return Typeof(Number);
            }
        }
    
        private static class SimpleNumberCoercerBigDecNull : SimpleNumberCoercer, SimpleNumberBigDecimalCoercer {
            public Number CoerceBoxed(Number numToCoerce) {
                return numToCoerce;
            }
    
            public BigDecimal CoerceBoxedBigDec(Number numToCoerce) {
                return (BigDecimal) numToCoerce;
            }
    
            public Type GetReturnType() {
                return Typeof(Number);
            }
        }
    }
} // end of namespace
