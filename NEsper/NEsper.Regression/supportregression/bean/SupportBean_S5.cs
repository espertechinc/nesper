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
    public class SupportBean_S5
    {
        private static int idCounter;

        public SupportBean_S5(int id)
        {
            Id = id;
            P50 = null;
            P51 = null;
            P52 = null;
            P53 = null;
        }

        public SupportBean_S5(int id, String p50)
        {
            Id = id;
            P50 = p50;
            P51 = null;
            P52 = null;
            P53 = null;
        }

        public SupportBean_S5(int id, String p50, String p51)
        {
            Id = id;
            P50 = p50;
            P51 = p51;
            P52 = null;
            P53 = null;
        }

        public int Id { get; set; }

        public String P50 { get; set; }

        public String P51 { get; private set; }

        public String P52 { get; private set; }

        public String P53 { get; private set; }

        public static object[] MakeS5(String propOne, String[] propTwo)
        {
            idCounter++;

            var events = new Object[propTwo.Length];
            for (int i = 0; i < propTwo.Length; i++) {
                events[i] = new SupportBean_S5(idCounter, propOne, propTwo[i]);
            }
            return events;
        }
    }
}
