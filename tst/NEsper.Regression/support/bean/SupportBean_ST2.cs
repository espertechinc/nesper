///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBean_ST2
    {
        public SupportBean_ST2(
            string id,
            string key2,
            int p20)
        {
            Id = id;
            Key2 = key2;
            P20 = p20;
        }

        public string Id { get; }

        public string Key2 { get; }

        public int P20 { get; }
    }
} // end of namespace