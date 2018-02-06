///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class SupportBean_ST0
    {
        public SupportBean_ST0(String id, int p00)
        {
            Id = id;
            P00 = p00;
        }

        public SupportBean_ST0(String id, String key0, int p00)
        {
            Id = id;
            Key0 = key0;
            P00 = p00;
        }

        public SupportBean_ST0(String id, long? p01Long)
        {
            Id = id;
            P01Long = p01Long;
        }

        public SupportBean_ST0(String id,int p00, String pcommon)
        {
            Id = id;
            P00 = p00;
            Pcommon = pcommon;
        }

        public string Id { get; private set; }

        public string Key0 { get; private set; }

        public int P00 { get; private set; }

        public long? P01Long { get; private set; }

        public string Pcommon { get; set; }

        public bool Equals(SupportBean_ST0 other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Id, Id);
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
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(SupportBean_ST0)) return false;
            return Equals((SupportBean_ST0) obj);
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
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}