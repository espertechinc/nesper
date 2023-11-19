///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class SupportBean_ST0
    {
        public SupportBean_ST0(
            string id,
            int p00)
        {
            Id = id;
            P00 = p00;
        }

        public SupportBean_ST0(
            string id,
            string key0,
            int p00)
        {
            Id = id;
            Key0 = key0;
            P00 = p00;
        }

        public SupportBean_ST0(
            string id,
            long? p01Long)
        {
            Id = id;
            P01Long = p01Long;
        }

        public SupportBean_ST0(
            string id,
            int p00,
            string pcommon)
        {
            Id = id;
            P00 = p00;
            Pcommon = pcommon;
        }

        public string Id { get; }

        public string Key0 { get; }

        public int P00 { get; }

        public long? P01Long { get; }

        public string Pcommon { get; set; }

        protected bool Equals(SupportBean_ST0 other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((SupportBean_ST0) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(P00)}: {P00}";
        }
    }
} // end of namespace
