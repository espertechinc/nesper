///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// Descriptor for an index requirement on a historical stream.
    /// <para/>
    /// Saves and compares the properties indexed and their types, as well as the types 
    /// of key properties to account for coercion.
    /// </summary>
    public class HistoricalStreamIndexDesc
    {
        /// <summary>Ctor. </summary>
        /// <param name="indexProperties">index properties</param>
        /// <param name="indexPropTypes">index property types</param>
        /// <param name="keyPropTypes">key property types</param>
        public HistoricalStreamIndexDesc(IList<string> indexProperties, Type[] indexPropTypes, Type[] keyPropTypes)
        {
            IndexProperties = indexProperties;
            IndexPropTypes = indexPropTypes;
            KeyPropTypes = keyPropTypes;
        }

        /// <summary>Returns index properties. </summary>
        /// <value>index properties</value>
        internal IList<string> IndexProperties; // { get; private set; }

        /// <summary>Returns index property types. </summary>
        /// <value>index property types</value>
        internal Type[] IndexPropTypes; // { get; private set; }

        /// <summary>Returns key property types. </summary>
        /// <value>key property types</value>
        internal Type[] KeyPropTypes; // { get; private set; }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;

            HistoricalStreamIndexDesc that = (HistoricalStreamIndexDesc)obj;

            if (!Collections.AreEqual(IndexPropTypes, that.IndexPropTypes)) return false;
            if (!Collections.AreEqual(IndexProperties, that.IndexProperties)) return false;
            if (!Collections.AreEqual(KeyPropTypes, that.KeyPropTypes)) return false;

            return true;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            int result = Collections.GetHashCode(IndexProperties);
            result = 31 * result + Collections.GetHashCode(IndexPropTypes);
            result = 31 * result + Collections.GetHashCode(KeyPropTypes);
            return result;
        }
    }
}
