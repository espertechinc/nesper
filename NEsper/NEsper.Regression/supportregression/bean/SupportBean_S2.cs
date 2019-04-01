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
    public sealed class SupportBean_S2
    {
        private static int idCounter;

        public SupportBean_S2(int id)
        {
            Id = id;
        }

        public SupportBean_S2(int id, String p20)
        {
            Id = id;
            P20 = p20;
        }

        public SupportBean_S2(int id, String p20, String p21)
        {
            Id = id;
            P20 = p20;
            P21 = p21;
        }

        public int Id { get; set; }

        public String P20 { get; set; }

        public String P21 { get; set; }

        public String P22 { get; set; }

        public String P23 { get; set; }

        public static object[] MakeS2(String propOne, String[] propTwo)
        {
            idCounter++;

            var events = new Object[propTwo.Length];
            for (int i = 0; i < propTwo.Length; i++) {
                events[i] = new SupportBean_S2(idCounter, propOne, propTwo[i]);
            }
            return events;
        }
    }
}
