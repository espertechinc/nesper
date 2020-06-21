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
    [Serializable]
    public partial class BitWiseOpEnum
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

        private static readonly IDictionary<BitWiseOpDesc, Computer> computers;

        static BitWiseOpEnum()
        {
            computers = new Dictionary<BitWiseOpDesc, Computer>();
            computers.Put(new BitWiseOpDesc(typeof(byte?), BAND), new BAndByte());
            computers.Put(new BitWiseOpDesc(typeof(short?), BAND), new BAndShort());
            computers.Put(new BitWiseOpDesc(typeof(int?), BAND), new BAndInt());
            computers.Put(new BitWiseOpDesc(typeof(long?), BAND), new BAndLong());
            computers.Put(new BitWiseOpDesc(typeof(bool?), BAND), new BAndBoolean());
            computers.Put(new BitWiseOpDesc(typeof(byte?), BOR), new BOrByte());
            computers.Put(new BitWiseOpDesc(typeof(short?), BOR), new BOrShort());
            computers.Put(new BitWiseOpDesc(typeof(int?), BOR), new BOrInt());
            computers.Put(new BitWiseOpDesc(typeof(long?), BOR), new BOrLong());
            computers.Put(new BitWiseOpDesc(typeof(bool?), BOR), new BOrBoolean());
            computers.Put(new BitWiseOpDesc(typeof(byte?), BXOR), new BXorByte());
            computers.Put(new BitWiseOpDesc(typeof(short?), BXOR), new BXorShort());
            computers.Put(new BitWiseOpDesc(typeof(int?), BXOR), new BXorInt());
            computers.Put(new BitWiseOpDesc(typeof(long?), BXOR), new BXorLong());
            computers.Put(new BitWiseOpDesc(typeof(bool?), BXOR), new BXorBoolean());
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

            return computers.Get(new BitWiseOpDesc(coercedType, this));
        }
    }
} // end of namespace