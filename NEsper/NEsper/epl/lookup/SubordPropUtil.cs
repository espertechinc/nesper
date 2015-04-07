///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.compat;
using com.espertech.esper.epl.join.plan;


namespace com.espertech.esper.epl.lookup
{
    public class SubordPropUtil {

        public static bool IsStrictKeyJoin(ICollection<SubordPropHashKey> hashKeys)
        {
            return hashKeys.All(
                hashKey => (hashKey.HashKey is QueryGraphValueEntryHashKeyedProp));
        }

        /// <summary>Returns the key stream numbers. </summary>
        /// <param name="descList">a list of descriptors</param>
        /// <returns>key stream numbers</returns>
        public static int[] GetKeyStreamNums(ICollection<SubordPropHashKey> descList)
        {
            var streamIds = new int[descList.Count];
            var count = 0;
            foreach (SubordPropHashKey desc in descList)
            {
                if (!(desc.HashKey is QueryGraphValueEntryHashKeyedProp)) {
                    throw new UnsupportedOperationException("Not a strict key compare");
                }
                streamIds[count++] = desc.OptionalKeyStreamNum.Value;
            }
            return streamIds;
        }
    
        /// <summary>Returns the key property names. </summary>
        /// <param name="descList">a list of descriptors</param>
        /// <returns>key property names</returns>
        public static String[] GetKeyProperties(ICollection<SubordPropHashKey> descList)
        {
            var result = new String[descList.Count];
            var count = 0;
            foreach (var desc in descList)
            {
                if (!(desc.HashKey is QueryGraphValueEntryHashKeyedProp)) {
                    throw new UnsupportedOperationException("Not a strict key compare");
                }
                var keyed = (QueryGraphValueEntryHashKeyedProp) desc.HashKey;
                result[count++] = keyed.KeyProperty;
            }
            return result;
        }
    
        /// <summary>Returns the key property names. </summary>
        /// <param name="descList">a list of descriptors</param>
        /// <returns>key property names</returns>
        public static String[] GetKeyProperties(SubordPropHashKey[] descList)
        {
            var result = new String[descList.Length];
            var count = 0;
            foreach (var desc in descList)
            {
                if (!(desc.HashKey is QueryGraphValueEntryHashKeyedProp)) {
                    throw new UnsupportedOperationException("Not a strict key compare");
                }
                var keyed = (QueryGraphValueEntryHashKeyedProp) desc.HashKey;
                result[count++] = keyed.KeyProperty;
            }
            return result;
        }
    
        /// <summary>Returns the key coercion types. </summary>
        /// <param name="descList">a list of descriptors</param>
        /// <returns>key coercion types</returns>
        public static Type[] GetCoercionTypes(ICollection<SubordPropHashKey> descList)
        {
            var result = new Type[descList.Count];
            var count = 0;
            foreach (var desc in descList)
            {
                result[count++] = desc.CoercionType;
            }
            return result;
        }
    
        /// <summary>Returns the key coercion types. </summary>
        /// <param name="descList">a list of descriptors</param>
        /// <returns>key coercion types</returns>
        public static Type[] GetCoercionTypes(SubordPropHashKey[] descList)
        {
            return descList.Select(desc => desc.CoercionType).ToArray();
        }
    }
}
