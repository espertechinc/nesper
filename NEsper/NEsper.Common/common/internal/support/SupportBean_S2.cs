///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.support
{
    [Serializable]
    public sealed class SupportBean_S2
    {
        private static int idCounter;

        public SupportBean_S2(int id)
        {
            Id = id;
        }

        public SupportBean_S2(
            int id,
            string p20)
        {
            Id = id;
            P20 = p20;
        }

        public SupportBean_S2(
            int id,
            string p20,
            string p21)
        {
            Id = id;
            P20 = p20;
            P21 = p21;
        }

        public int Id { get; set; }

        public string P20 { get; set; }

        public string P21 { get; set; }

        public string P22 { get; set; }

        public string P23 { get; set; }

        public static object[] MakeS2(
            string propOne,
            string[] propTwo)
        {
            idCounter++;

            var events = new object[propTwo.Length];
            for (int i = 0; i < propTwo.Length; i++) {
                events[i] = new SupportBean_S2(idCounter, propOne, propTwo[i]);
            }

            return events;
        }
    }
}