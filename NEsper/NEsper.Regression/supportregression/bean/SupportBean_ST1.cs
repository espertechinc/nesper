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
    public class SupportBean_ST1
    {
        public SupportBean_ST1(String id,
                               String key1,
                               int p10)
        {
            Id = id;
            Key1 = key1;
            P10 = p10;
        }

        public SupportBean_ST1(String id,
                               int p10)
        {
            Id = id;
            P10 = p10;
        }

        public SupportBean_ST1(String id,
                               long? p11Long)
        {
            Id = id;
            P11Long = p11Long;
        }

        public SupportBean_ST1(String id,
                               int p10,
                               String pcommon)
        {
            Id = id;
            P10 = p10;
            Pcommon = pcommon;
        }

        public string Id { get; private set; }

        public string Key1 { get; private set; }

        public int P10 { get; private set; }

        public long? P11Long { get; private set; }

        public string Pcommon { get; set; }
    }
}