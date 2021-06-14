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
    [Serializable]
    public class SupportBean_ST0
    {
        private string _pcommon;

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
            _pcommon = pcommon;
        }

        public string Id { get; }

        public string Key0 { get; }

        public int P00 { get; }

        public long? P01Long { get; }

        public string Pcommon {
            get => _pcommon;
            set => _pcommon = value;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (SupportBean_ST0) o;

            if (!Id.Equals(that.Id)) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
} // end of namespace