///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.lookupplan
{
    public class SubordPropUtil
    {
        public static bool IsStrictKeyJoin(ICollection<SubordPropHashKeyForge> hashKeys)
        {
            foreach (var hashKey in hashKeys) {
                if (!(hashKey.HashKey is QueryGraphValueEntryHashKeyedForgeProp)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Returns the key stream numbers.
        /// </summary>
        /// <param name="descList">a list of descriptors</param>
        /// <returns>key stream numbers</returns>
        public static int[] GetKeyStreamNums(ICollection<SubordPropHashKeyForge> descList)
        {
            var streamIds = new int[descList.Count];
            var count = 0;
            foreach (var desc in descList) {
                if (!(desc.HashKey is QueryGraphValueEntryHashKeyedForgeProp)) {
                    throw new UnsupportedOperationException("Not a strict key compare");
                }

                if (!desc.OptionalKeyStreamNum.HasValue) {
                    throw new EPRuntimeException("keyStream does not have value");
                }

                streamIds[count++] = desc.OptionalKeyStreamNum.Value;
            }

            return streamIds;
        }

        /// <summary>
        ///     Returns the key property names.
        /// </summary>
        /// <param name="descList">a list of descriptors</param>
        /// <returns>key property names</returns>
        public static string[] GetKeyProperties(ICollection<SubordPropHashKeyForge> descList)
        {
            var result = new string[descList.Count];
            var count = 0;
            foreach (var desc in descList) {
                if (!(desc.HashKey is QueryGraphValueEntryHashKeyedForgeProp)) {
                    throw new UnsupportedOperationException("Not a strict key compare");
                }

                var keyed = (QueryGraphValueEntryHashKeyedForgeProp) desc.HashKey;
                result[count++] = keyed.KeyProperty;
            }

            return result;
        }

        /// <summary>
        ///     Returns the key property names.
        /// </summary>
        /// <param name="descList">a list of descriptors</param>
        /// <returns>key property names</returns>
        public static string[] GetKeyProperties(SubordPropHashKeyForge[] descList)
        {
            var result = new string[descList.Length];
            var count = 0;
            foreach (var desc in descList) {
                if (!(desc.HashKey is QueryGraphValueEntryHashKeyedForgeProp)) {
                    throw new UnsupportedOperationException("Not a strict key compare");
                }

                var keyed = (QueryGraphValueEntryHashKeyedForgeProp) desc.HashKey;
                result[count++] = keyed.KeyProperty;
            }

            return result;
        }

        /// <summary>
        ///     Returns the key coercion types.
        /// </summary>
        /// <param name="descList">a list of descriptors</param>
        /// <returns>key coercion types</returns>
        public static Type[] GetCoercionTypes(ICollection<SubordPropHashKeyForge> descList)
        {
            var result = new Type[descList.Count];
            var count = 0;
            foreach (var desc in descList) {
                result[count++] = desc.CoercionType;
            }

            return result;
        }

        /// <summary>
        ///     Returns the key coercion types.
        /// </summary>
        /// <param name="descList">a list of descriptors</param>
        /// <returns>key coercion types</returns>
        public static Type[] GetCoercionTypes(SubordPropHashKeyForge[] descList)
        {
            var result = new Type[descList.Length];
            var count = 0;
            foreach (var desc in descList) {
                result[count++] = desc.CoercionType;
            }

            return result;
        }
    }
} // end of namespace