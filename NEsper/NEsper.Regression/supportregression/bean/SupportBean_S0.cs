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
    public class SupportBean_S0 
    {
        private static int _idCounter;

        public static object[] MakeS0(String propOne, String[] propTwo)
        {
            _idCounter++;
            var events = new Object[propTwo.Length];
            for (int i = 0; i < propTwo.Length; i++)
            {
                events[i] = new SupportBean_S0(_idCounter, propOne, propTwo[i]);
            }
            return events;
        }
    
        public SupportBean_S0(int id)
        {
            Id = id;
        }
    
        public SupportBean_S0(int id, String p00)
        {
            Id = id;
            P00 = p00;
        }
    
        public SupportBean_S0(int id, String p00,  String p01)
        {
            Id = id;
            P00 = p00;
            P01 = p01;
        }
    
        public SupportBean_S0(int id, String p00,  String p01, String p02)
        {
            Id = id;
            P00 = p00;
            P01 = p01;
            P02 = p02;
        }
    
        public SupportBean_S0(int id, String p00, String p01, String p02, String p03)
        {
            Id = id;
            P00 = p00;
            P01 = p01;
            P02 = p02;
            P03 = p03;
        }

        public int Id { get; set; }

        public string P00 { get; set; }

        public string P01 { get; set; }

        public string P02 { get; set; }

        public string P03 { get; set; }

        public override String ToString() {
            return "SupportBean_S0{" +
                    "id=" + Id +
                    ", p00='" + P00 + '\'' +
                    ", p01='" + P01 + '\'' +
                    ", p02='" + P02 + '\'' +
                    ", p03='" + P03 + '\'' +
                    '}';
        }
    
        public override bool Equals(Object o) {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
    
            var that = (SupportBean_S0) o;
    
            if (Id != that.Id) return false;
            if (P00 != null ? !P00.Equals(that.P00) : that.P00 != null) return false;
            if (P01 != null ? !P01.Equals(that.P01) : that.P01 != null) return false;
            if (P02 != null ? !P02.Equals(that.P02) : that.P02 != null) return false;
            if (P03 != null ? !P03.Equals(that.P03) : that.P03 != null) return false;
    
            return true;
        }

        public override int GetHashCode()
        {
            int result = Id;
            result = 31 * result + (P00 != null ? P00.GetHashCode() : 0);
            result = 31 * result + (P01 != null ? P01.GetHashCode() : 0);
            result = 31 * result + (P02 != null ? P02.GetHashCode() : 0);
            result = 31 * result + (P03 != null ? P03.GetHashCode() : 0);
            return result;
        }
    }
}
