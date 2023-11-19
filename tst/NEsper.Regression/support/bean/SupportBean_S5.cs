///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBean_S5
    {
        private static int _idCounter;

        private int _id;
        private string _p50;
        private string _p51;
        private string _p52;
        private string _p53;

        public SupportBean_S5(int id)
        {
            _id = id;
        }

        public SupportBean_S5(
            int id,
            string p50)
        {
            _id = id;
            _p50 = p50;
        }

        public SupportBean_S5(
            int id,
            string p50,
            string p51)
        {
            _id = id;
            _p50 = p50;
            _p51 = p51;
        }

        public int Id {
            get => _id;
            set => _id = value;
        }

        public string P50 {
            get => _p50;
            set => _p50 = value;
        }

        public string P51 {
            get => _p51;
            set => _p51 = value;
        }

        public string P52 {
            get => _p52;
            set => _p52 = value;
        }

        public string P53 {
            get => _p53;
            set => _p53 = value;
        }

        public static object[] MakeS5(
            string propOne,
            string[] propTwo)
        {
            _idCounter++;

            var events = new object[propTwo.Length];
            for (var i = 0; i < propTwo.Length; i++) {
                events[i] = new SupportBean_S5(_idCounter, propOne, propTwo[i]);
            }

            return events;
        }
    }
} // end of namespace