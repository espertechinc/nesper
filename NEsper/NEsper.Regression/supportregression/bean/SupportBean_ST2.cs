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
    public class SupportBean_ST2
    {
        public SupportBean_ST2(String id,
                               String key2,
                               int p20)
        {
            Id = id;
            Key2 = key2;
            P20 = p20;
        }

        public string Id { get; private set; }

        public string Key2 { get; private set; }

        public int P20 { get; private set; }
    }
}