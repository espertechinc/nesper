///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.Json.Serialization;

namespace com.espertech.esper.common.@internal.support
{
    public class SupportBean_S2
    {
        private int id;
        private string p20;
        private string p21;
        private string p22;
        private string p23;

        public static object[] MakeS2(
            string propOne,
            string[] propTwo)
        {
            var events = new object[propTwo.Length];
            for (var i = 0; i < propTwo.Length; i++) {
                events[i] = new SupportBean_S2(-1, propOne, propTwo[i]);
            }

            return events;
        }

        public SupportBean_S2(int id)
        {
            this.id = id;
        }

        public SupportBean_S2(
            int id,
            string p20)
        {
            this.id = id;
            this.p20 = p20;
        }

        [JsonConstructor]
        public SupportBean_S2(
            int id,
            string p20,
            string p21)
        {
            this.id = id;
            this.p20 = p20;
            this.p21 = p21;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (SupportBean_S2)o;
            if (id != that.id) {
                return false;
            }

            if (!p20?.Equals(that.p20) ?? that.p20 != null) {
                return false;
            }

            if (!p21?.Equals(that.p21) ?? that.p21 != null) {
                return false;
            }

            if (!p22?.Equals(that.p22) ?? that.p22 != null) {
                return false;
            }

            return p23?.Equals(that.p23) ?? that.p23 == null;
        }

        public override int GetHashCode()
        {
            var result = id;
            result = 31 * result + (p20 != null ? p20.GetHashCode() : 0);
            result = 31 * result + (p21 != null ? p21.GetHashCode() : 0);
            result = 31 * result + (p22 != null ? p22.GetHashCode() : 0);
            result = 31 * result + (p23 != null ? p23.GetHashCode() : 0);
            return result;
        }

        public override string ToString()
        {
            return "SupportBean_S2{" +
                   "id=" +
                   id +
                   ", p20='" +
                   p20 +
                   '\'' +
                   ", p21='" +
                   p21 +
                   '\'' +
                   ", p22='" +
                   p22 +
                   '\'' +
                   ", p23='" +
                   p23 +
                   '\'' +
                   '}';
        }

        public int Id {
            get => id;

            set => id = value;
        }

        public string P20 {
            get => p20;

            set => p20 = value;
        }

        public string P21 {
            get => p21;

            set => p21 = value;
        }

        public string P22 {
            get => p22;

            set => p22 = value;
        }

        public string P23 {
            get => p23;

            set => p23 = value;
        }
    }
} // end of namespace