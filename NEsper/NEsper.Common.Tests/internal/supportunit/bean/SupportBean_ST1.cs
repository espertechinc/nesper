///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class SupportBean_ST1
    {
        public SupportBean_ST1(
            string id,
            string key1,
            int p10)
        {
            Id = id;
            Key1 = key1;
            P10 = p10;
        }

        public SupportBean_ST1(
            string id,
            int p10)
        {
            Id = id;
            P10 = p10;
        }

        public SupportBean_ST1(
            string id,
            long? p11Long)
        {
            Id = id;
            P11Long = p11Long;
        }

        public SupportBean_ST1(
            string id,
            int p10,
            string pcommon)
        {
            Id = id;
            P10 = p10;
            Pcommon = pcommon;
        }

        public string Id { get; }

        public string Key1 { get; }

        public int P10 { get; }

        public long? P11Long { get; }

        public string Pcommon { get; set; }
    }
} // end of namespace
