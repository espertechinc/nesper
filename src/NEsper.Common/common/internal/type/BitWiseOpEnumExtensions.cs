///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    ///     Operations on the BitWiseOpEnum.
    /// </summary>
    public static class BitWiseOpEnumExtensions
    {
        private static readonly IDictionary<BitWiseOpDesc, BitWiseComputer> COMPUTERS;

        static BitWiseOpEnumExtensions()
        {
            COMPUTERS = new Dictionary<BitWiseOpDesc, BitWiseComputer>();
            COMPUTERS.Put(new BitWiseOpDesc(typeof(byte?), BitWiseOpEnum.BAND), new BitWiseAndByte());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(short?), BitWiseOpEnum.BAND), new BitWiseAndShort());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(int?), BitWiseOpEnum.BAND), new BitWiseAndInt());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(long?),BitWiseOpEnum.BAND), new BitWiseAndLong());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(bool?), BitWiseOpEnum.BAND), new BitWiseAndBoolean());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(byte?), BitWiseOpEnum.BOR), new BitWiseOrByte());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(short?), BitWiseOpEnum.BOR), new BitWiseOrShort());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(int?), BitWiseOpEnum.BOR), new BitWiseOrInt());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(long?), BitWiseOpEnum.BOR), new BitWiseOrLong());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(bool?), BitWiseOpEnum.BOR), new BitWiseOrBoolean());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(byte?), BitWiseOpEnum.BXOR), new BitWiseXorByte());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(short?), BitWiseOpEnum.BXOR), new BitWiseXorShort());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(int?), BitWiseOpEnum.BXOR), new BitWiseXorInt());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(long?), BitWiseOpEnum.BXOR), new BitWiseXorLong());
            COMPUTERS.Put(new BitWiseOpDesc(typeof(bool?), BitWiseOpEnum.BXOR), new BitWiseXorBoolean());
        }

        /// <summary>
        ///     Returns the operator as an expression text.
        /// </summary>
        /// <returns>text of operator</returns>
        public static string GetExpressionText(this BitWiseOpEnum value)
        {
            return value switch {
                BitWiseOpEnum.BAND => "&",
                BitWiseOpEnum.BOR => "|",
                BitWiseOpEnum.BXOR => "^",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        /// <summary>
        ///     Returns string rendering of enum.
        /// </summary>
        /// <returns>bitwise operator string</returns>
        public static string GetComputeDescription(this BitWiseOpEnum value)
        {
            return GetExpressionText(value);
        }

        /// <summary>
        ///     Returns number or boolean computation for the target coercion type.
        /// </summary>
        /// <param name="coercedType">target type</param>
        /// <returns>number cruncher</returns>
        public static BitWiseComputer GetComputer(this BitWiseOpEnum value, Type coercedType)
        {
            coercedType = coercedType.GetBoxedType();
            if (coercedType != typeof(byte?) &&
                coercedType != typeof(short?) &&
                coercedType != typeof(int?) &&
                coercedType != typeof(long?) &&
                coercedType != typeof(bool?)) {
                throw new ArgumentException(
                    $"Expected base numeric or boolean type for computation result but got type {coercedType}");
            }

            return COMPUTERS.Get(new BitWiseOpDesc(coercedType, value));
        }
    }
} // end of namespace