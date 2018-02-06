///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.lookup
{
    public class IndexMultiKey
    {
        public IndexMultiKey(
            bool unique,
            IList<IndexedPropDesc> hashIndexedProps,
            IList<IndexedPropDesc> rangeIndexedProps,
            AdvancedIndexDesc advancedIndexDesc)
        {
            IsUnique = unique;
            HashIndexedProps = hashIndexedProps.ToArray();
            RangeIndexedProps = rangeIndexedProps.ToArray();
            AdvancedIndexDesc = advancedIndexDesc;
        }

        public bool IsUnique { get; private set; }
        public IndexedPropDesc[] HashIndexedProps { get; private set; }
        public IndexedPropDesc[] RangeIndexedProps { get; private set; }
        public AdvancedIndexDesc AdvancedIndexDesc { get; private set; }

        public String ToQueryPlan()
        {
            StringWriter writer = new StringWriter();
            writer.Write(IsUnique ? "unique " : "non-unique ");
            writer.Write("hash={");
            IndexedPropDesc.ToQueryPlan(writer, HashIndexedProps);
            writer.Write("} btree={");
            IndexedPropDesc.ToQueryPlan(writer, RangeIndexedProps);
            writer.Write("} advanced={");
            writer.Write(AdvancedIndexDesc == null ? "" : AdvancedIndexDesc.ToQueryPlan());
            writer.Write("}");
            return writer.ToString();
        }

        public bool Equals(IndexMultiKey other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (other.IsUnique.Equals(IsUnique)
                && Collections.AreEqual(other.HashIndexedProps, HashIndexedProps)
                && Collections.AreEqual(other.RangeIndexedProps, RangeIndexedProps))
            {
                if (AdvancedIndexDesc == null)
                {
                    return other.AdvancedIndexDesc == null;
                }
                else
                {
                    return other.AdvancedIndexDesc != null && AdvancedIndexDesc.EqualsAdvancedIndex(other.AdvancedIndexDesc);
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof (IndexMultiKey))
                return false;
            return Equals((IndexMultiKey) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = IsUnique.GetHashCode();
                result = (result*397) ^ (HashIndexedProps != null ? Collections.GetHashCode(HashIndexedProps) : 0);
                result = (result*397) ^ (RangeIndexedProps != null ? Collections.GetHashCode(RangeIndexedProps) : 0);
                return result;
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("IsUnique: {0}, HashIndexedProps: {1}, RangeIndexedProps: {2}", IsUnique, HashIndexedProps, RangeIndexedProps);
        }
    }
}