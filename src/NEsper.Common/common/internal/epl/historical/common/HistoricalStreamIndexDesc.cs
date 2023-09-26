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
    /// <para/>Saves and compares the properties indexed and their types, as well as the types of key properties to
    /// account for coercion.
    /// </summary>
    public class HistoricalStreamIndexDesc
    {
        private readonly string[] indexProperties;
        private readonly Type[] indexPropTypes;
        private readonly Type[] keyPropTypes;

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
            this.indexProperties = indexProperties;
            this.indexPropTypes = indexPropTypes;
            this.keyPropTypes = keyPropTypes;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (HistoricalStreamIndexDesc)o;
            if (!Arrays.Equals(indexPropTypes, that.indexPropTypes)) {
                return false;
            }

            if (!Arrays.Equals(indexProperties, that.indexProperties)) {
                return false;
            }

            if (!Arrays.Equals(keyPropTypes, that.keyPropTypes)) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(indexProperties, indexPropTypes, keyPropTypes);
        }

        public string[] IndexProperties => indexProperties;

        public Type[] IndexPropTypes => indexPropTypes;

        public Type[] KeyPropTypes => keyPropTypes;
    }
} // end of namespace