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
    public class SupportBean_S6
    {
        private static int idCounter;

        public SupportBean_S6(int id)
        {
            Id = id;
        }

        public SupportBean_S6(int id, String p60)
        {
            Id = id;
            P60 = p60;
        }

        public SupportBean_S6(int id, String p60, String p61)
        {
            Id = id;
            P60 = p60;
            P61 = p61;
        }

        public int Id { get; private set; }

        public String P60 { get; private set; }

        public String P61 { get; private set; }

        public String P62 { get; private set; }

        public String P63 { get; private set; }

        public static object[] MakeS6(String propOne, String[] propTwo)
        {
            idCounter++;

            var events = new Object[propTwo.Length];
            for (int i = 0; i < propTwo.Length; i++) {
                events[i] = new SupportBean_S6(idCounter, propOne, propTwo[i]);
            }
            return events;
        }
    }
}
