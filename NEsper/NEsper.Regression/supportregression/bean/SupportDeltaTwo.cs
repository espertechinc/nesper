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
    public class SupportDeltaTwo
    {
        private readonly String k0;
        private readonly String p0;
        private readonly String p2;
        private readonly String p3;
        private readonly String someOtherProp;
    
        public SupportDeltaTwo(String k0, String p0, String p2, String p3)
        {
            this.k0 = k0;
            this.p0 = p0;
            this.p2 = p2;
            this.p3 = p3;
            someOtherProp = "abc";
        }

        public string K0
        {
            get { return k0; }
        }

        public string P0
        {
            get { return p0; }
        }

        public string P2
        {
            get { return p2; }
        }

        public string P3
        {
            get { return p3; }
        }

        public string SomeOtherProp
        {
            get { return someOtherProp; }
        }
    }
}
