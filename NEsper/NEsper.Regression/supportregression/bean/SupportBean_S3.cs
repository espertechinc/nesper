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
    public sealed class SupportBean_S3
    {
        private static int idCounter;

        public SupportBean_S3(int id)
        {
            Id = id;
        }

        public SupportBean_S3(int id, String p30)
        {
            Id = id;
            P30 = p30;
        }

        public SupportBean_S3(int id, String p30, String p31)
        {
            Id = id;
            P30 = p30;
            P31 = p31;
        }

        public int Id { get; set; }

        public String P30 { get; set; }

        public String P31 { get; set; }

        public String P32 { get; set; }

        public String P33 { get; set; }

        public static object[] MakeS3(String propOne, String[] propTwo)
        {
            idCounter++;

            var events = new Object[propTwo.Length];
            for (int i = 0; i < propTwo.Length; i++) {
                events[i] = new SupportBean_S3(idCounter, propOne, propTwo[i]);
            }
            return events;
        }
    }
}
