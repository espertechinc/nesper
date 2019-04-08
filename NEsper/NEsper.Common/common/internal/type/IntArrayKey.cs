///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    public class IntArrayKey
    {
        private readonly int hashCode;
        private readonly int[] keys;

        public IntArrayKey(int[] keys)
        {
            if (keys == null) {
                throw new ArgumentException("The array of keys must not be null");
            }

            this.keys = keys;
            hashCode = CompatExtensions.Hash(keys);
        }

        public int Count {
            get { return keys.Length; }
        }

        public override bool Equals(object other)
        {
            if (other == this) {
                return true;
            }

            if (other is IntArrayKey) {
                var otherKeys = (IntArrayKey) other;
                return CompatExtensions.Equals(keys, otherKeys.keys);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override string ToString()
        {
            return "IntArrayKey" + CompatExtensions.AsList(keys);
        }

        public int[] GetKeys()
        {
            return keys;
        }
    }
} // end of namespace