///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    ///     Enum representing relational types of operation.
    /// </summary>
    public class BitWiseOpEnum
    {
        /// <summary>
        ///     Bitwise and.
        /// </summary>
        public static readonly BitWiseOpEnum BAND = new BitWiseOpEnum("&");

        /// <summary>
        ///     Bitwise or.
        /// </summary>
        public static readonly BitWiseOpEnum BOR = new BitWiseOpEnum("|");

        /// <summary>
        ///     Bitwise xor.
        /// </summary>
        public static readonly BitWiseOpEnum BXOR = new BitWiseOpEnum("^");

        private static readonly IDictionary<HashableMultiKey, Computer> computers;

        static BitWiseOpEnum()
        {
            computers = new Dictionary<HashableMultiKey, Computer>();
            computers.Put(new HashableMultiKey(new object[] {typeof(byte), BAND}), new BAndByte());
            computers.Put(new HashableMultiKey(new object[] {typeof(short), BAND}), new BAndShort());
            computers.Put(new HashableMultiKey(new object[] {typeof(int), BAND}), new BAndInt());
            computers.Put(new HashableMultiKey(new object[] {typeof(long), BAND}), new BAndLong());
            computers.Put(new HashableMultiKey(new object[] {typeof(bool), BAND}), new BAndBoolean());
            computers.Put(new HashableMultiKey(new object[] {typeof(byte), BOR}), new BOrByte());
            computers.Put(new HashableMultiKey(new object[] {typeof(short), BOR}), new BOrShort());
            computers.Put(new HashableMultiKey(new object[] {typeof(int), BOR}), new BOrInt());
            computers.Put(new HashableMultiKey(new object[] {typeof(long), BOR}), new BOrLong());
            computers.Put(new HashableMultiKey(new object[] {typeof(bool), BOR}), new BOrBoolean());
            computers.Put(new HashableMultiKey(new object[] {typeof(byte), BXOR}), new BXorByte());
            computers.Put(new HashableMultiKey(new object[] {typeof(short), BXOR}), new BXorShort());
            computers.Put(new HashableMultiKey(new object[] {typeof(int), BXOR}), new BXorInt());
            computers.Put(new HashableMultiKey(new object[] {typeof(long), BXOR}), new BXorLong());
            computers.Put(new HashableMultiKey(new object[] {typeof(bool), BXOR}), new BXorBoolean());
        }

        private BitWiseOpEnum(string expressionText)
        {
            ExpressionText = expressionText;
        }

        /// <summary>
        ///     Returns the operator as an expression text.
        /// </summary>
        /// <returns>text of operator</returns>
        public string ExpressionText { get; }

        /// <summary>
        ///     Returns string rendering of enum.
        /// </summary>
        /// <returns>bitwise operator string</returns>
        public string ComputeDescription => ExpressionText;

        /// <summary>
        ///     Returns number or boolean computation for the target coercion type.
        /// </summary>
        /// <param name="coercedType">target type</param>
        /// <returns>number cruncher</returns>
        public Computer GetComputer(Type coercedType)
        {
            coercedType = coercedType.GetBoxedType();
            if (coercedType != typeof(byte?) &&
                coercedType != typeof(short?) &&
                coercedType != typeof(int?) &&
                coercedType != typeof(long?) &&
                coercedType != typeof(bool?)) {
                throw new ArgumentException(
                    "Expected base numeric or boolean type for computation result but got type " + coercedType);
            }

            var key = new HashableMultiKey(new object[] {coercedType, this});
            return computers.Get(key);
        }

        /// <summary>
        ///     Computer for relational op.
        /// </summary>
        public interface Computer
        {
            /// <summary>
            ///     Computes using the 2 numbers or boolean a result object.
            /// </summary>
            /// <param name="objOne">is the first number or boolean</param>
            /// <param name="objTwo">is the second number or boolean</param>
            /// <returns>result</returns>
            object Compute(
                object objOne,
                object objTwo);
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        /// <summary>
        ///     Bit Wise And.
        /// </summary>
        public class BAndByte : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var n1 = (byte) objOne;
                var n2 = (byte) objTwo;
                return (byte) (n1 & n2);
            }
        }

        /// <summary>
        ///     Bit Wise Or.
        /// </summary>
        public class BOrByte : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var n1 = (byte) objOne;
                var n2 = (byte) objTwo;
                return (byte) (n1 | n2);
            }
        }

        /// <summary>
        ///     Bit Wise Xor.
        /// </summary>
        public class BXorByte : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var n1 = (byte) objOne;
                var n2 = (byte) objTwo;
                return (byte) (n1 ^ n2);
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        /// <summary>
        ///     Bit Wise And.
        /// </summary>
        public class BAndShort : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var n1 = (short) objOne;
                var n2 = (short) objTwo;
                return (short) (n1 & n2);
            }
        }

        /// <summary>
        ///     Bit Wise Or.
        /// </summary>
        public class BOrShort : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var n1 = (short) objOne;
                var n2 = (short) objTwo;
                return (short) (n1 | n2);
            }
        }

        /// <summary>
        ///     Bit Wise Xor.
        /// </summary>
        public class BXorShort : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var n1 = (short) objOne;
                var n2 = (short) objTwo;
                return (short) (n1 ^ n2);
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        /// <summary>
        ///     Bit Wise And.
        /// </summary>
        public class BAndInt : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var n1 = (int) objOne;
                var n2 = (int) objTwo;
                return n1 & n2;
            }
        }

        /// <summary>
        ///     Bit Wise Or.
        /// </summary>
        public class BOrInt : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var n1 = (int) objOne;
                var n2 = (int) objTwo;
                return n1 | n2;
            }
        }

        /// <summary>
        ///     Bit Wise Xor.
        /// </summary>
        public class BXorInt : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var n1 = (int) objOne;
                var n2 = (int) objTwo;
                return n1 ^ n2;
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        /// <summary>
        ///     Bit Wise And.
        /// </summary>
        public class BAndLong : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var n1 = (long) objOne;
                var n2 = (long) objTwo;
                return n1 & n2;
            }
        }

        /// <summary>
        ///     Bit Wise Or.
        /// </summary>
        public class BOrLong : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var n1 = (long) objOne;
                var n2 = (long) objTwo;
                return n1 | n2;
            }
        }

        /// <summary>
        ///     Bit Wise Xor.
        /// </summary>
        public class BXorLong : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var n1 = (long) objOne;
                var n2 = (long) objTwo;
                return n1 ^ n2;
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        /// <summary>
        ///     Bit Wise And.
        /// </summary>
        public class BAndBoolean : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var b1 = (bool) objOne;
                var b2 = (bool) objTwo;
                return b1 & b2;
            }
        }

        /// <summary>
        ///     Bit Wise Or.
        /// </summary>
        public class BOrBoolean : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var b1 = (bool) objOne;
                var b2 = (bool) objTwo;
                return b1 | b2;
            }
        }

        /// <summary>
        ///     Bit Wise Xor.
        /// </summary>
        public class BXorBoolean : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var b1 = (bool) objOne;
                var b2 = (bool) objTwo;
                return b1 ^ b2;
            }
        }
    }
} // end of namespace