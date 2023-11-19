///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBean_S4
    {
        private static int _idCounter;

        private int _id;
        private string _p40;
        private string _p41;
        private string _p42;
        private string _p43;

        public SupportBean_S4(int id)
        {
            _id = id;
        }

        public SupportBean_S4(
            int id,
            string p40)
        {
            _id = id;
            _p40 = p40;
        }

        public SupportBean_S4(
            int id,
            string p40,
            string p41)
        {
            _id = id;
            _p40 = p40;
            _p41 = p41;
        }

        public int Id {
            get => _id;
            set => _id = value;
        }

        public string P40 {
            get => _p40;
            set => _p40 = value;
        }

        public string P41 {
            get => _p41;
            set => _p41 = value;
        }

        public string P42 {
            get => _p42;
            set => _p42 = value;
        }

        public string P43 {
            get => _p43;
            set => _p43 = value;
        }

        public static object[] MakeS4(
            string propOne,
            string[] propTwo)
        {
            _idCounter++;

            var events = new object[propTwo.Length];
            for (var i = 0; i < propTwo.Length; i++) {
                events[i] = new SupportBean_S4(_idCounter, propOne, propTwo[i]);
            }

            return events;
        }
    }
} // end of namespace