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
    public class SupportBean_S4
    {
        private static int idCounter;

        public SupportBean_S4(int id)
        {
            Id = id;
        }

        public SupportBean_S4(
            int id,
            string p40)
        {
            Id = id;
            P40 = p40;
        }

        public SupportBean_S4(
            int id,
            string p40,
            string p41)
        {
            Id = id;
            P40 = p40;
            P41 = p41;
        }

        public int Id { get; set; }

        public string P40 { get; set; }

        public string P41 { get; set; }

        public string P42 { get; set; }

        public string P43 { get; set; }

        public static object[] MakeS4(
            string propOne,
            string[] propTwo)
        {
            idCounter++;

            var events = new object[propTwo.Length];
            for (var i = 0; i < propTwo.Length; i++)
            {
                events[i] = new SupportBean_S4(idCounter, propOne, propTwo[i]);
            }

            return events;
        }
    }
} // end of namespace
