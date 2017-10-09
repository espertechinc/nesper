///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportunit.bean
{
    [Serializable]
    public class SupportBean_S1
    {
        private static int _idCounter;

        public SupportBean_S1(int id)
        {
            Id = id;
        }

        public SupportBean_S1(int id,
                              String p10)
        {
            Id = id;
            P10 = p10;
        }

        public SupportBean_S1(int id,
                              String p10,
                              String p11)
        {
            Id = id;
            P10 = p10;
            P11 = p11;
        }

        public SupportBean_S1(int id,
                              String p10,
                              String p11,
                              String p12)
        {
            Id = id;
            P10 = p10;
            P11 = p11;
            P12 = p12;
        }

        public SupportBean_S1(int id, String p10, String p11, String p12, String p13)
        {
            Id = id;
            P10 = p10;
            P11 = p11;
            P12 = p12;
            P13 = p13;
        }

        public int Id { get; set; }

        public string P10 { get; set; }

        public string P11 { get; set; }

        public string P12 { get; set; }

        public string P13 { get; set; }

        public static Object[] MakeS1(String propOne,
                                      String[] propTwo)
        {
            _idCounter++;

            var events = new Object[propTwo.Length];
            for (int i = 0; i < propTwo.Length; i++)
            {
                events[i] = new SupportBean_S1(_idCounter, propOne, propTwo[i]);
            }
            return events;
        }
    }
}