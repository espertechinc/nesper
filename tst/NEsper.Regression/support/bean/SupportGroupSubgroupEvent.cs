///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportGroupSubgroupEvent
    {
        private double _value;

        public SupportGroupSubgroupEvent()
        {
        }

        public SupportGroupSubgroupEvent(
            string group,
            string subGroup,
            int type,
            double value)
        {
            Grp = group;
            SubGrp = subGroup;
            Type = type;
            _value = value;
        }

        public string Grp { get; set; }

        public string SubGrp { get; set; }

        public int Type { get; set; }

        public double Value {
            get => _value;
            set => _value = value;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (obj is SupportGroupSubgroupEvent) {
                var evt = (SupportGroupSubgroupEvent) obj;
                return Grp.Equals(evt.Grp) &&
                       SubGrp.Equals(evt.SubGrp) &&
                       Type == evt.Type &&
                       Math.Abs(_value - evt._value) < 1e-6;
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = Grp != null ? Grp.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (SubGrp != null ? SubGrp.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Type;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return "(" + Grp + ", " + SubGrp + ")@" + Type + "=" + _value;
        }
    }
} // end of namespace