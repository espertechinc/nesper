///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportEventWithLongArray
    {
        public SupportEventWithLongArray(string id, long[] coll)
        {
            Id = id;
            Coll = coll;
        }

        public long[] Coll { get; }
        
        public string Id { get; }

        protected bool Equals(SupportEventWithLongArray other)
        {
            return Id == other.Id && Arrays.AreEqual(Coll, other.Coll);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((SupportEventWithLongArray) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((Coll != null ? Coll.GetHashCode() : 0) * 397) ^ (Id != null ? Id.GetHashCode() : 0);
            }
        }
    }
} // end of namespace