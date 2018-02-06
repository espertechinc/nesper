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
    public class SupportBeanInt
    {
        public SupportBeanInt(String id, int p00, int p01, int p02, int p03, int p04, int p05)
        {
            Id = id;
            P00 = p00;
            P01 = p01;
            P02 = p02;
            P03 = p03;
            P04 = p04;
            P05 = p05;
        }

        public string Id { get; private set; }

        public int P00 { get; private set; }

        public int P01 { get; private set; }

        public int P02 { get; private set; }

        public int P03 { get; private set; }

        public int P04 { get; private set; }

        public int P05 { get; private set; }
    }
}
