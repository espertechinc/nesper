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
    [Serializable]
    public class SupportBean_S5
    {
        private static int idCounter;

        public SupportBean_S5(int id)
        {
            Id = id;
        }

        public SupportBean_S5(
            int id,
            string p50)
        {
            Id = id;
            P50 = p50;
        }

        public SupportBean_S5(
            int id,
            string p50,
            string p51)
        {
            Id = id;
            P50 = p50;
            P51 = p51;
        }

        public int Id { get; set; }

        public string P50 { get; set; }

        public string P51 { get; set; }

        public string P52 { get; set; }

        public string P53 { get; set; }

        public static object[] MakeS5(
            string propOne,
            string[] propTwo)
        {
            idCounter++;

            var events = new object[propTwo.Length];
            for (var i = 0; i < propTwo.Length; i++)
            {
                events[i] = new SupportBean_S5(idCounter, propOne, propTwo[i]);
            }

            return events;
        }
    }
} // end of namespace