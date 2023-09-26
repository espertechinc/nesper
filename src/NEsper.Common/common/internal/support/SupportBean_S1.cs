///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.support
{
    [Serializable]
    public class SupportBean_S1
    {
        private int id;
        private string p10;
        private string p11;
        private string p12;
        private string p13;

        public static object[] MakeS1(
            string propOne,
            string[] propTwo)
        {
            var events = new object[propTwo.Length];
            for (var i = 0; i < propTwo.Length; i++) {
                events[i] = new SupportBean_S1(-1, propOne, propTwo[i]);
            }

            return events;
        }

        public SupportBean_S1(int id)
        {
            this.id = id;
        }

        public SupportBean_S1(
            int id,
            string p10)
        {
            this.id = id;
            this.p10 = p10;
        }

        public SupportBean_S1(
            int id,
            string p10,
            string p11)
        {
            this.id = id;
            this.p10 = p10;
            this.p11 = p11;
        }

        public SupportBean_S1(
            int id,
            string p10,
            string p11,
            string p12)
        {
            this.id = id;
            this.p10 = p10;
            this.p11 = p11;
            this.p12 = p12;
        }

        public SupportBean_S1(
            int id,
            string p10,
            string p11,
            string p12,
            string p13)
        {
            this.id = id;
            this.p10 = p10;
            this.p11 = p11;
            this.p12 = p12;
            this.p13 = p13;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (SupportBean_S1)o;
            if (id != that.id) {
                return false;
            }

            if (!p10?.Equals(that.p10) ?? that.p10 != null) {
                return false;
            }

            if (!p11?.Equals(that.p11) ?? that.p11 != null) {
                return false;
            }

            if (!p12?.Equals(that.p12) ?? that.p12 != null) {
                return false;
            }

            return p13?.Equals(that.p13) ?? that.p13 == null;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, p10, p11, p12, p13);
        }

        public override string ToString()
        {
            return "SupportBean_S1{" +
                   "id=" +
                   id +
                   ", p10='" +
                   p10 +
                   '\'' +
                   ", p11='" +
                   p11 +
                   '\'' +
                   ", p12='" +
                   p12 +
                   '\'' +
                   ", p13='" +
                   p13 +
                   '\'' +
                   '}';
        }

        public int Id {
            get => id;

            set => id = value;
        }

        public string P10 {
            get => p10;

            set => p10 = value;
        }

        public string P11 {
            get => p11;

            set => p11 = value;
        }

        public string P12 {
            get => p12;

            set => p12 = value;
        }

        public string P13 {
            get => p13;

            set => p13 = value;
        }
    }
} // end of namespace