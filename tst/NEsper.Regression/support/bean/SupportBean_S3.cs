///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBean_S3
    {
        private static int _idCounter;

        private int _id;
        private string _p30;
        private string _p31;
        private string _p32;
        private string _p33;

        public SupportBean_S3(int id)
        {
            _id = id;
        }

        public SupportBean_S3(
            int id,
            string p30)
        {
            _id = id;
            _p30 = p30;
        }

        public SupportBean_S3(
            int id,
            string p30,
            string p31)
        {
            _id = id;
            _p30 = p30;
            _p31 = p31;
        }

        public int Id {
            get => _id;
            set => _id = value;
        }

        public string P30 {
            get => _p30;
            set => _p30 = value;
        }

        public string P31 {
            get => _p31;
            set => _p31 = value;
        }

        public string P32 {
            get => _p32;
            set => _p32 = value;
        }

        public string P33 {
            get => _p33;
            set => _p33 = value;
        }

        public static object[] MakeS3(
            string propOne,
            string[] propTwo)
        {
            _idCounter++;

            var events = new object[propTwo.Length];
            for (var i = 0; i < propTwo.Length; i++) {
                events[i] = new SupportBean_S3(_idCounter, propOne, propTwo[i]);
            }

            return events;
        }
    }
} // end of namespace