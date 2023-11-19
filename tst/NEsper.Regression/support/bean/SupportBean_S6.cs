///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBean_S6
    {
        private static int _idCounter;

        public SupportBean_S6(int id)
        {
            Id = id;
        }

        public SupportBean_S6(
            int id,
            string p60)
        {
            Id = id;
            P60 = p60;
        }

        public SupportBean_S6(
            int id,
            string p60,
            string p61)
        {
            Id = id;
            P60 = p60;
            P61 = p61;
        }

        public int Id { get; }

        public string P60 { get; }

        public string P61 { get; }

        public string P62 { get; }

        public string P63 { get; }

        public static object[] MakeS6(
            string propOne,
            string[] propTwo)
        {
            _idCounter++;

            var events = new object[propTwo.Length];
            for (var i = 0; i < propTwo.Length; i++) {
                events[i] = new SupportBean_S6(_idCounter, propOne, propTwo[i]);
            }

            return events;
        }
    }
} // end of namespace