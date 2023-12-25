///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.historical.common
{
    /// <summary>
    /// Descriptor for an index requirement on a historical stream.
    /// <para>
    /// Saves and compares the properties indexed and their types, as well as the types
    /// of key properties to account for coercion.
    /// </para>
    /// </summary>
    public class HistoricalStreamIndexDesc
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "indexProperties">index properties</param>
        /// <param name = "indexPropTypes">index property types</param>
        /// <param name = "keyPropTypes">key property types</param>
        public HistoricalStreamIndexDesc(
            string[] indexProperties,
            Type[] indexPropTypes,
            Type[] keyPropTypes)
        {
            IndexProperties = indexProperties;
            IndexPropTypes = indexPropTypes;
            KeyPropTypes = keyPropTypes;
        }
        
        public string[] IndexProperties { get; }

        public Type[] IndexPropTypes { get; }

        public Type[] KeyPropTypes { get; }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;

            var that = (HistoricalStreamIndexDesc) obj;
            
            if (!Arrays.AreEqual(IndexPropTypes, that.IndexPropTypes)) return false;
            if (!Arrays.AreEqual(IndexProperties, that.IndexProperties)) return false;
            if (!Arrays.AreEqual(KeyPropTypes, that.KeyPropTypes)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IndexProperties, IndexPropTypes, KeyPropTypes);
        }
    }
} // end of namespace